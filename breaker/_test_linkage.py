#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
RW 设定值联动回归测试（不依赖 mbserver / breaker，直接在本地执行 make_stub.py 的设备脚本）。

覆盖点（= 上位机可写点 → 期望联动）：
  ENV 空调设定温度 / 目标湿度 → 车间温度 / 湿度向设定靠拢（含旧版钳制 [240,320]/[40,80] 之外的值）
  IM 料筒1段设定 / 保压时间 / 自动模式 → 实际温度跟随 / 周期时长变化 / 手动停产出料
  MTC 设定温度 / 加热线圈 → 实际温度跟随 / 停加热降温
  DRY 干燥温度设定 → 实际温度跟随
  ROB 自动 / 启动取件 → 手动取件计数
  EM 远程合闸 → 断电测量归零

用法：python _test_linkage.py
"""

import math
import random
import time as _time_mod

import make_stub as ms


# ---------------------------------------------------------------------------
# mbserver 脚本宿主的最小 mock（语义对齐 src/server/python/mbserver.py：
# getint16 有符号 / getuint16 无符号 / 越界返回 0 / uint32 低字在前）
# ---------------------------------------------------------------------------
class MemBits:
    def __init__(self, n):
        self.bits = [0] * n

    def setbit(self, off, v):
        self.bits[off] = 1 if v else 0

    def getbit(self, off):
        return bool(self.bits[off])


def _i16(v):
    v &= 0xFFFF
    return v - 0x10000 if v >= 0x8000 else v


class MemRegs:
    def __init__(self, n):
        self.regs = [0] * n

    def setint16(self, off, v):
        self.regs[off] = _i16(v)

    def getint16(self, off):
        return _i16(self.regs[off])

    def setuint16(self, off, v):
        self.regs[off] = v & 0xFFFF

    def getuint16(self, off):
        return self.regs[off] & 0xFFFF

    def setuint32(self, off, v):
        v &= 0xFFFFFFFF
        self.regs[off] = v & 0xFFFF
        self.regs[off + 1] = (v >> 16) & 0xFFFF

    def getuint32(self, off):
        return self.regs[off] | (self.regs[off + 1] << 16)


class MbDevice:
    def __init__(self, name):
        self._name = name
        self._cyc = 0

    def getname(self):
        return self._name

    def getpycycle(self):
        return self._cyc

    def _incpycycle(self):
        self._cyc += 1


def run_device(name, init, loop, n_cycles, writers=None, seed=None):
    """执行 init + n_cycles 次 loop。writers: {cycle: [(area, off, value)]}。
    返回 (mem, g)。"""
    writers = writers or {}
    mem = {"0x": MemBits(16), "1x": MemBits(20), "3x": MemRegs(200), "4x": MemRegs(16)}
    dev = MbDevice(name)
    g = {
        "time": _time_mod.time, "sleep": lambda s: None,
        "random": random, "math": math,
        "mbdevice": dev,
        "mem0x": mem["0x"], "mem1x": mem["1x"],
        "mem3x": mem["3x"], "mem4x": mem["4x"],
    }
    if seed is not None:
        random.seed(seed)
    exec(init, g)
    for cyc in range(n_cycles):
        for area, off, val in writers.get(cyc, []):
            m = mem[area]
            if hasattr(m, "setbit"):
                m.setbit(off, val)
            else:
                m.setint16(off, val) if area == "4x" else m.setuint16(off, val)
        exec(loop, g)
        dev._incpycycle()
    return mem, g


# ---------------------------------------------------------------------------
# 断言工具
# ---------------------------------------------------------------------------
_results = []


def check(label, cond, detail=""):
    _results.append((label, bool(cond), detail))
    print("  %s  %s%s" % ("PASS" if cond else "FAIL", label,
                           ("  [%s]" % detail) if detail else ""))


def tail_mean(mem, area, off, n=40):
    """最后 n 次逐步读取的均值（重放 loop 不现实，这里直接取终值近似）。"""
    return mem[area].getint16(off) if area == "3x" else mem[area].getuint16(off)


# ---------------------------------------------------------------------------
def test_env():
    print("== ENV-01 车间环境 ==")
    # 空调设定 220 (22.0C) —— 旧钳制下限 240 之下
    mem, _ = run_device("ENV-01", ms.ENV_INIT, ms.ENV_LOOP, 300,
                        {50: [("4x", 0, 220)]})
    t = mem["3x"].getint16(0)
    check("空调设定 22.0C → 车间温度靠拢(终值 %d, 目标 220)" % t, 210 <= t <= 230)

    # 空调设定 160 (16.0C) —— 典型空调低温
    mem, _ = run_device("ENV-01", ms.ENV_INIT, ms.ENV_LOOP, 400,
                        {50: [("4x", 0, 160)]})
    t = mem["3x"].getint16(0)
    check("空调设定 16.0C → 车间温度靠拢(终值 %d, 目标 160)" % t, 150 <= t <= 170)

    # 空调设定 300 (30.0C) —— 范围内回归
    mem, _ = run_device("ENV-01", ms.ENV_INIT, ms.ENV_LOOP, 300,
                        {50: [("4x", 0, 300)]})
    t = mem["3x"].getint16(0)
    check("空调设定 30.0C → 车间温度靠拢(终值 %d, 目标 300)" % t, 290 <= t <= 310)

    # 目标湿度 30% —— 旧钳制下限 40 之下
    mem, _ = run_device("ENV-01", ms.ENV_INIT, ms.ENV_LOOP, 300,
                        {50: [("4x", 1, 30)]})
    h = mem["3x"].getuint16(1)
    check("目标湿度 30%% → 湿度靠拢(终值 %d)" % h, 24 <= h <= 36)

    # 目标湿度 90% —— 旧钳制上限 80 之上
    mem, _ = run_device("ENV-01", ms.ENV_INIT, ms.ENV_LOOP, 400,
                        {50: [("4x", 1, 90)]})
    h = mem["3x"].getuint16(1)
    check("目标湿度 90%% → 湿度靠拢(终值 %d)" % h, 82 <= h <= 96)

    # 高温报警阈值仍为温度>32C
    mem, _ = run_device("ENV-01", ms.ENV_INIT, ms.ENV_LOOP, 400,
                        {50: [("4x", 0, 340)]})
    alarm = mem["1x"].getbit(0)
    t = mem["3x"].getint16(0)
    check("设定 34C 稳定后高温报警置位(温度 %d)" % t, alarm and t > 320)


def test_im():
    print("== IM-01 注塑机 ==")
    # 料筒1段设定 2400 (240C) → 3x19 跟随
    mem, _ = run_device("IM-01", ms.IM_INIT, ms.IM_LOOP, 300,
                        {50: [("4x", 2, 2400)]})
    t = mem["3x"].getint16(19)
    check("料筒1段设定 240C → 实际温度靠拢(终值 %d)" % t, 2300 <= t <= 2500)

    # 保压时间 60 (6s)：TOTAL 300→335，一轮循环后当前周期时间 ≈ 335±5%
    mem, _ = run_device("IM-01", ms.IM_INIT, ms.IM_LOOP, 700,
                        {50: [("4x", 8, 60)]})
    cyc = mem["3x"].getuint16(9)
    shots = mem["3x"].getuint32(3)
    check("保压时间 6s → 周期≈33.5s(报告 %d, 需 >315)" % cyc, cyc > 315,
          "cyc=%d shots=%d" % (cyc, shots))
    check("保压加长后仍正常出模计数", shots >= 1)

    # 自动模式线圈置 0 → 停止生产（1x2 生产中=0, 功率=3）
    mem, _ = run_device("IM-01", ms.IM_INIT, ms.IM_LOOP, 250,
                        {100: [("0x", 2, 0)]})
    prod = mem["1x"].getbit(2)
    pw = mem["3x"].getuint16(50)
    check("自动模式断开 → 停止生产(生产中=%d, 功率=%d)" % (prod, pw), (not prod) and pw <= 3)

    # 工作模式 0 → 停止生产（回归）
    mem, _ = run_device("IM-01", ms.IM_INIT, ms.IM_LOOP, 250,
                        {100: [("4x", 0, 0)]})
    prod = mem["1x"].getbit(2)
    check("工作模式=停止 → 停止生产(生产中=%d)" % prod, not prod)


def test_mtc():
    print("== MTC-01 模温机 ==")
    mem, _ = run_device("MTC-01", ms.MTC_INIT, ms.MTC_LOOP, 250,
                        {50: [("4x", 0, 600)]})
    t = mem["3x"].getint16(2)
    check("设定 60C → 实际温度靠拢(终值 %d)" % t, 570 <= t <= 630)

    mem, _ = run_device("MTC-01", ms.MTC_INIT, ms.MTC_LOOP, 250,
                        {50: [("4x", 0, 600)], 150: [("0x", 2, 0)]})
    t = mem["3x"].getint16(2)
    check("加热关断 → 温度下降(终值 %d, 需 <560)" % t, t < 560)


def test_dry():
    print("== DRY-01 干燥机 ==")
    mem, _ = run_device("DRY-01", ms.DRY_INIT, ms.DRY_LOOP, 250,
                        {50: [("4x", 0, 600)]})
    t = mem["3x"].getint16(2)
    check("设定 60C → 干燥温度靠拢(终值 %d)" % t, 570 <= t <= 630)


def test_rob():
    print("== ROB-01 机械手 ==")
    # 手动模式触发一次取件 → 累计取件数 +1
    mem, _ = run_device("ROB-01", ms.ROB_INIT, ms.ROB_LOOP, 120,
                        {10: [("0x", 1, 0)], 20: [("0x", 2, 1)]})
    picks = mem["3x"].getuint32(3)
    check("手动触发启动取件 → 累计取件数≥1(实际 %d)" % picks, picks >= 1)


def test_em():
    print("== EM-01 总电表 ==")
    mem, _ = run_device("EM-01", ms.EM_INIT, ms.EM_LOOP, 150,
                        {100: [("0x", 0, 0)]})
    va = mem["3x"].getuint16(4)
    kw = mem["3x"].getuint16(10)
    check("远程合闸断开 → 电压/功率归零(V=%d, kW=%d)" % (va, kw), va == 0 and kw == 0)


def main():
    test_env()
    test_im()
    test_mtc()
    test_dry()
    test_rob()
    test_em()
    n_fail = sum(1 for _, ok, _ in _results if not ok)
    print("\n%d 项, %d 失败" % (len(_results), n_fail))
    if n_fail:
        for label, ok, detail in _results:
            if not ok:
                print("  FAIL:", label, detail)
        raise SystemExit(1)
    print("全部通过")


if __name__ == "__main__":
    main()
