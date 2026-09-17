#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""breaker.go_offline 掉线模式切换回归测试（对应 bak_20260917_breaker_mode_fix）。

修复前的两个问题：
  1. 设备已掉线时 go_offline 直接早退，mode 参数被静默忽略 —— 先断成 refuse、
     再想改成 silent 不起作用，表现为"模式切不过去"。
  2. 模式切换必须重放副作用（refuse 关监听 / silent 保持监听），只改 dev.mode
     字段会让实际行为与显示的模式不符。

全部使用假对象，不需要监听端口、不依赖 mbserver。运行：python _test_breaker_mode.py
"""
import argparse
import asyncio
import sys

import breaker as B
from breaker import OFFLINE, ONLINE

PASS = 0
FAIL = 0


def check(name, cond):
    global PASS, FAIL
    if cond:
        PASS += 1
        print("PASS:", name)
    else:
        FAIL += 1
        print("FAIL:", name)


class FakeServer(object):
    def __init__(self):
        self.closed = False

    def close(self):
        self.closed = True


class FakePair(object):
    """假连接：记录 close / close_backend 各自是否被调用。"""

    def __init__(self):
        self.closed = False
        self.backend_closed = False

    def close(self):
        self.closed = True

    async def close_backend(self):
        self.backend_closed = True


def new_breaker():
    args = argparse.Namespace(public_base=502, internal_base=15502,
                              internal_host="127.0.0.1", rules="")
    return B.Breaker(args, console=False)


async def scenario():
    br = new_breaker()
    dev = br.devices[0]  # IM-01
    opened = []

    async def fake_start_listener(d):
        opened.append(d.name)
        d.server = FakeServer()

    real_start_listener = B.Breaker.start_listener
    B.Breaker.start_listener = lambda self, d: fake_start_listener(d)

    try:
        # ---------- 在线 → refuse ----------
        dev.server = FakeServer()
        pair = FakePair()
        dev.conns.add(pair)
        r = await br.go_offline(dev, "手动", mode="refuse")
        check("在线→refuse 返回 True", r is True)
        check("在线→refuse 状态为掉线", dev.state == OFFLINE)
        check("在线→refuse 模式为 refuse", dev.mode == "refuse")
        check("refuse 关闭了监听", dev.server is None)
        check("refuse 掐断了已有连接", pair.closed is True)

        # ---------- 核心回归：已掉线时 refuse → silent ----------
        pair2 = FakePair()
        dev.conns.add(pair2)
        opened[:] = []
        r = await br.go_offline(dev, "手动", mode="silent")
        check("已掉线时 refuse→silent 返回 True（修复前 False）", r is True)
        check("已掉线时 refuse→silent 模式真的变了（修复前被忽略）", dev.mode == "silent")
        check("切到 silent 时重新打开了监听（黑洞需要接住新连接）",
              dev.server is not None and opened == ["IM-01"])
        check("silent 只掐后端半边、不关客户端连接",
              pair2.backend_closed is True and pair2.closed is False)
        check("切换模式不改掉线起始时间", dev.offline_since is not None)

        # ---------- 反向：已掉线时 silent → refuse ----------
        pair3 = FakePair()
        dev.conns.add(pair3)
        r = await br.go_offline(dev, "手动", mode="refuse")
        check("已掉线时 silent→refuse 返回 True", r is True)
        check("已掉线时 silent→refuse 模式真的变了", dev.mode == "refuse")
        check("切回 refuse 时关闭了监听", dev.server is None)
        check("切回 refuse 时掐断客户端连接", pair3.closed is True)

        # ---------- 同模式重复调用仍是 no-op ----------
        before = dev.offline_since
        r = await br.go_offline(dev, "手动", mode="refuse")
        check("已掉线且模式相同时返回 False", r is False)
        check("no-op 不影响掉线起始时间", dev.offline_since == before)
        check("no-op 后模式不变", dev.mode == "refuse")

        # ---------- mode=None（定时/随机规则路径）不得改动模式 ----------
        dev.mode = "silent"
        r = await br.go_offline(dev, "定时窗口", duration=30)
        check("mode=None 返回 False", r is False)
        check("mode=None 保持原模式不变（规则路径不应清掉手动设的模式）", dev.mode == "silent")
        check("mode=None 带秒数时武装了自动恢复任务", dev.recover_task is not None)

        # ---------- 非法 mode 字符串被忽略 ----------
        r = await br.go_offline(dev, "手动", mode="blackhole")
        check("非法 mode 被忽略、返回 False", r is False and dev.mode == "silent")

        # ---------- 恢复在线后再断，模式参数依然生效 ----------
        await br.go_online(dev)
        check("恢复在线", dev.state == ONLINE)
        r = await br.go_offline(dev, "手动", mode="silent")
        check("在线→silent 生效", r is True and dev.mode == "silent" and dev.state == OFFLINE)

        # 收尾：取消定时任务，避免 asyncio 退出时报 pending task
        for d in br.devices:
            for t in [d.recover_task] + d.auto_tasks:
                if t is not None:
                    t.cancel()
    finally:
        B.Breaker.start_listener = real_start_listener


if __name__ == "__main__":
    asyncio.run(scenario())
    print("== 结果: PASS=%d FAIL=%d ==" % (PASS, FAIL))
    sys.exit(1 if FAIL else 0)
