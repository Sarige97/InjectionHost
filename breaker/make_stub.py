#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
注塑工厂 Modbus 数据桩生成器 v4（标准 Modbus 区域语义）
======================================================
单数据源生成：stub.mbs（18从站, TCP 502）+ 点表.xlsx

v4 区域规范（符合标准 Modbus 语义）：
  0x 线圈    RW  控制输出(电源/运行/自动/加热/风机/启动取件/合闸)
  1x 离散输入 RO  状态反馈(就绪/生产中/报警/合模/夹持/通讯/需量超限)
  3x 输入寄存器 RO  传感器/测量值(温度/压力/位置/电压/电流/功率) + 状态字 + 报警码
  4x 保持寄存器 RW  控制参数/设定值(工作模式/目标产量/设定温度/保压时间)

特性：
  * 控制点脚本只读不覆盖，实际值带热惯性向设定靠拢
  * 电源关=断电、运行关=停机、达产自动停机
  * 随机偶发故障作为背景(5%/分钟/机制)
  * 生成前自动备份旧 stub.mbs / make_stub.py 到 mbs_bak/

用法：python3 make_stub.py [-o stub.mbs] [--excel 点表.xlsx] [--internal-base 1502] [--no-excel]
      默认代理模式: mbserver 监听内部端口 1502~1519，由 breaker.py 代理为对外 502~519（支持单设备掉线模拟）；
      --internal-base 502 回退直连模式。
"""

import argparse
import json
import os
import shutil
import datetime
import xml.etree.ElementTree as ET

BAK_DIR = "mbs_bak"

# ---------------------------------------------------------------------------
# 端口规划（掉线模拟架构，见 breaker.py / 交接文档）：
#   mbserver 实际监听 INTERNAL_BASE 起的 18 个内部端口；
#   breaker.py(断路器代理) 监听 PUBLIC_BASE 起的 18 个对外端口转发给 mbserver，
#   上位机连对外端口（表不变），并支持"某台设备单独掉线"模拟。
#   内部端口 = 对外端口 + 15000。
#   ※ 基准必须避开 Windows 动态端口范围（本机为 1024~15000，netsh int ipv4
#     show dynamicport tcp 可查），否则监听端口可能被系统临时连接随机占用。
#   --internal-base 502 可回退直连模式（mbserver 直接监听对外端口，无需代理）。
# ---------------------------------------------------------------------------
PUBLIC_BASE = 502
INTERNAL_BASE = 15502

# ===========================================================================
# 一、点表定义
#   字段: (数据区, 偏移, 含义, 类型, 缩放, 读写, 初值, 模拟规则, 异常模拟)
#   读写: RW=上位机可写(控制点，脚本只读不覆盖), R=只读(脚本输出)
# ===========================================================================

IM_POINTS = [
    # 0x 线圈 - 控制输出
    ("0x", 0, "电源",       "bit", "", "RW", "1", "控制: 0=断电整机停", ""),
    ("0x", 1, "运行",       "bit", "", "RW", "1", "控制: 0=停机待机", ""),
    ("0x", 2, "自动模式",   "bit", "", "RW", "1", "控制: 0=手动", ""),
    # 1x 离散输入 - 状态反馈
    ("1x", 0, "合模",       "bit", "", "R",  "0", "合模/开模阶段置位", ""),
    ("1x", 1, "就绪",       "bit", "", "R",  "1", "电源开且无故障", ""),
    ("1x", 2, "生产中",     "bit", "", "R",  "1", "生产中", ""),
    ("1x", 3, "报警",       "bit", "", "R",  "0", "", "故障时置位"),
    # 3x 输入寄存器 - 传感器/状态
    ("3x", 0,  "运行状态字", "uint16", "",        "R", "0",    "bit0电源 bit1合模 bit2注射 bit3保压 bit4熔胶 bit5冷却 bit6开模 bit7顶出 bit8故障", "故障时置bit8"),
    ("3x", 1,  "报警码",     "uint16", "",        "R", "0",    "0无/1油温过高/2模具报警/4射胶异常", "随机故障时写入"),
    ("3x", 2,  "报警组字",   "uint16", "",        "R", "0",    "bit0生产报警 bit1设备报警", "故障时置位"),
    ("3x", 3,  "总模数",     "uint32", "2寄存器",  "R", "0",    "每完成一个生产循环 +1", "达产后自动停机"),
    ("3x", 5,  "合格品数",   "uint32", "2寄存器",  "R", "0",    "合格 +1", ""),
    ("3x", 7,  "次品数",     "uint32", "2寄存器",  "R", "0",    "约 0.4% 概率次品 +1", "偶发次品"),
    ("3x", 9,  "当前周期时间", "uint16", "x0.1s",  "R", "300",  "约30s 注塑周期, 含噪声", ""),
    ("3x", 10, "平均周期时间", "uint16", "x0.1s",  "R", "300",  "周期滑动平均", ""),
    ("3x", 11, "稼动率",     "uint16", "%",        "R", "92",   "停机/故障时下降", ""),
    ("3x", 19, "料筒1段温度", "int16", "x0.1C",    "R", "2200", "实际温度, 向设定热惯性靠拢", "故障时波动"),
    ("3x", 20, "料筒2段温度", "int16", "x0.1C",    "R", "2100", "同上", "同上"),
    ("3x", 21, "料筒3段温度", "int16", "x0.1C",    "R", "2000", "同上", "同上"),
    ("3x", 22, "料筒4段温度", "int16", "x0.1C",    "R", "1900", "同上", "同上"),
    ("3x", 23, "料筒5段温度", "int16", "x0.1C",    "R", "1800", "同上", "同上"),
    ("3x", 29, "模具实际温度", "int16", "x0.1C",   "R", "850",  "向模具设定热惯性靠拢", "模具报警时异常升高"),
    ("3x", 30, "液压油温",    "int16", "x0.1C",    "R", "450",  "稳定 45C 波动", "油温过高报警(>58C)"),
    ("3x", 31, "冷却水温度",  "int16", "x0.1C",    "R", "280",  "28C 波动", ""),
    ("3x", 39, "射胶压力",    "uint16", "bar",     "R", "0",    "注射期 130~175, 保压期 60~90, 其余 0", "射胶异常时尖峰"),
    ("3x", 40, "射胶速度",    "uint16", "%",       "R", "0",    "注射期 70~95", ""),
    ("3x", 41, "保压压力",    "uint16", "bar",     "R", "0",    "保压期 55~85", ""),
    ("3x", 42, "螺杆转速",    "uint16", "rpm",     "R", "0",    "熔胶期 150~240", ""),
    ("3x", 43, "熔胶背压",    "uint16", "bar",     "R", "0",    "熔胶期 8~20", ""),
    ("3x", 44, "合模压力",    "uint16", "bar",     "R", "0",    "合模/开模期 30~60", ""),
    ("3x", 45, "注射位置",    "uint16", "mm",      "R", "0",    "注射期递减, 合模后回 1000", ""),
    ("3x", 46, "螺杆位置",    "uint16", "mm",      "R", "0",    "熔胶期递增, 注射期递减", ""),
    ("3x", 47, "泵电机电流",  "uint16", "x0.1A",   "R", "0",    "随负载 20~90", ""),
    ("3x", 48, "泵电机转速",  "uint16", "rpm",     "R", "0",    "900~1500", ""),
    ("3x", 49, "液压泵压力",  "uint16", "bar",     "R", "0",    "注射/保压期 30~60", ""),
    ("3x", 50, "瞬时功率",    "uint16", "kW",      "R", "0",    "随阶段 8~45kW, 断电0", ""),
    ("3x", 51, "当班产量",    "uint16", "",        "R", "0",    "每循环 +1", ""),
    ("3x", 52, "当班开机时长", "uint32", "s,2寄存器", "R", "0", "自启动累计", ""),
    # 4x 保持寄存器 - 控制参数
    ("4x", 0, "工作模式",     "uint16", "",        "RW", "3",   "控制: 0停止/1手动/2半自动/3全自动", ""),
    ("4x", 1, "目标产量",     "uint16", "",        "RW", "50000", "控制: 总模数达到后自动停机", ""),
    ("4x", 2, "料筒1段设定",  "int16",  "x0.1C",   "RW", "2200", "控制: 实际温度跟随", ""),
    ("4x", 3, "料筒2段设定",  "int16",  "x0.1C",   "RW", "2100", "控制: 实际温度跟随", ""),
    ("4x", 4, "料筒3段设定",  "int16",  "x0.1C",   "RW", "2000", "控制: 实际温度跟随", ""),
    ("4x", 5, "料筒4段设定",  "int16",  "x0.1C",   "RW", "1900", "控制: 实际温度跟随", ""),
    ("4x", 6, "料筒5段设定",  "int16",  "x0.1C",   "RW", "1800", "控制: 实际温度跟随", ""),
    ("4x", 7, "模具设定温度", "int16",  "x0.1C",   "RW", "850",  "控制: 实际温度跟随", ""),
    ("4x", 8, "保压时间",     "uint16", "x0.1s",   "RW", "25",   "控制: 影响生产周期时长", ""),
]

MTC_POINTS = [
    ("0x", 0, "电源", "bit", "", "RW", "1", "控制: 0=断电温度冷却泵停", ""),
    ("0x", 1, "运行", "bit", "", "RW", "1", "控制: 0=停机泵停", ""),
    ("0x", 2, "加热", "bit", "", "RW", "1", "控制: 0=停止加热(降温), 1=加热向设定升", ""),
    ("1x", 0, "就绪", "bit", "", "R", "1", "电源开", ""),
    ("1x", 1, "加热中", "bit", "", "R", "0", "加热且实际<设定", ""),
    ("1x", 2, "报警", "bit", "", "R", "0", "", "故障时置位"),
    ("3x", 0, "运行状态字", "uint16", "",        "R", "3",   "bit0电源 bit1运行 bit2加热 bit3冷却 bit4报警", ""),
    ("3x", 1, "报警码",     "uint16", "",        "R", "0",   "0无/1超温/2泵故障", "随机故障时写入"),
    ("3x", 2, "实际温度",   "int16",  "x0.1C",   "R", "1000", "向设定热惯性靠拢", "偶发超温 5~12C"),
    ("3x", 3, "回水温度",   "int16",  "x0.1C",   "R", "950",  "跟随实际-4C", ""),
    ("3x", 4, "泵压力",     "uint16", "bar",     "R", "3",    "2~5 波动", "泵故障时归零"),
    ("3x", 5, "泵流量",     "uint16", "L/min",   "R", "25",   "15~40 波动", "泵故障时归零"),
    ("3x", 6, "泵电流",     "uint16", "x0.1A",   "R", "45",   "3~6A 波动", ""),
    ("4x", 0, "设定温度",   "int16",  "x0.1C",   "RW", "1000", "控制: 实际温度跟随", ""),
]

DRY_POINTS = [
    ("0x", 0, "电源",     "bit", "", "RW", "1", "控制: 0=断电", ""),
    ("0x", 1, "运行",     "bit", "", "RW", "1", "控制: 0=停机", ""),
    ("0x", 2, "风机",     "bit", "", "RW", "1", "控制: 0=风机关", ""),
    ("0x", 3, "加热",     "bit", "", "RW", "1", "控制: 0=停止加热", ""),
    ("1x", 0, "就绪",     "bit", "", "R", "1", "电源开", ""),
    ("1x", 1, "干燥中",   "bit", "", "R", "1", "电源/运行/风机/加热均开", ""),
    ("1x", 2, "料位低",   "bit", "", "R", "0", "料位<25%", ""),
    ("1x", 3, "报警",     "bit", "", "R", "0", "", "故障时置位"),
    ("3x", 0, "运行状态字", "uint16", "",        "R", "7",   "bit0电源 bit1运行 bit2风机 bit3加热 bit4料位低 bit5报警", ""),
    ("3x", 1, "报警码",     "uint16", "",        "R", "0",   "0无/1露点高/2超温", "随机故障时写入"),
    ("3x", 2, "干燥温度实际", "int16", "x0.1C",   "R", "800", "向设定热惯性靠拢", "偶发超温"),
    ("3x", 3, "露点",       "int16",  "x0.1C",   "R", "-350", "-31~-37C 缓慢漂移", "偶发除湿失效升高"),
    ("3x", 4, "料位",       "uint16", "%",       "R", "85",   "缓降, 低于20%后补料回90%", ""),
    ("3x", 5, "加热功率",   "uint16", "kW",      "R", "5",    "3~8 波动", ""),
    ("3x", 6, "风机电流",   "uint16", "x0.1A",   "R", "12",   "0.8~2.0A 波动", ""),
    ("4x", 0, "干燥温度设定", "int16", "x0.1C",   "RW", "800", "控制: 实际温度跟随", ""),
]

ROB_POINTS = [
    ("0x", 0, "电源",     "bit", "", "RW", "1", "控制: 0=断电停", ""),
    ("0x", 1, "自动",     "bit", "", "RW", "1", "控制: 1=自动跟随生产, 0=手动等指令", ""),
    ("0x", 2, "启动取件", "bit", "", "RW", "0", "控制: 手动模式下写1触发一次取件", ""),
    ("1x", 0, "就绪",     "bit", "", "R", "1", "电源开", ""),
    ("1x", 1, "夹持到位", "bit", "", "R", "0", "取件/放件阶段", ""),
    ("1x", 2, "报警",     "bit", "", "R", "0", "", "故障时置位"),
    ("1x", 3, "安全门关闭", "bit", "", "R", "1", "常开", ""),
    ("3x", 0,  "运行状态字", "uint16", "",       "R", "3",  "bit0电源 bit1就绪 bit2取件 bit3放件 bit4回位 bit5故障 bit6自动", "故障时置bit5"),
    ("3x", 1,  "报警码",     "uint16", "",       "R", "0",  "0无/1取件失败/2机械故障", "随机故障时写入"),
    ("3x", 2,  "循环时间",   "uint16", "x0.1s",  "R", "30", "取件动作周期 约3s", ""),
    ("3x", 3,  "累计取件数", "uint32", "2寄存器", "R", "0",  "每完成一次取件 +1", ""),
    ("3x", 5,  "取出成功数", "uint32", "2寄存器", "R", "0",  "成功 +1", ""),
    ("3x", 7,  "取出失败数", "uint32", "2寄存器", "R", "0",  "约 2% 概率失败", "偶发"),
    ("3x", 9,  "X轴位置",    "uint16", "mm",     "R", "0",  "待机缓慢漂移 0~8, 动作期动画", ""),
    ("3x", 10, "Y轴位置",    "uint16", "mm",     "R", "0",  "同上", ""),
    ("3x", 11, "Z轴位置",    "uint16", "mm",     "R", "0",  "同上", ""),
    ("3x", 12, "抓取状态",   "uint16", "",       "R", "0",  "0空/1夹持", ""),
    ("3x", 13, "瞬时功率",   "uint16", "kW",     "R", "0",  "0.2~0.8kW", ""),
    ("3x", 14, "当前动作号", "uint16", "",       "R", "0",  "0待机 1取件 2放件 3回位", ""),
]

EM_POINTS = [
    ("0x", 0, "远程合闸", "bit", "", "RW", "1", "控制: 0=断电, 电压/电流/功率归零电能冻结", ""),
    ("1x", 0, "通讯状态", "bit", "", "R", "1", "常开", ""),
    ("1x", 1, "需量超限", "bit", "", "R", "0", "功率因数<0.8 置位", ""),
    ("3x", 0,  "累计电能",   "uint32", "0.01kWh,2寄存器", "R", "0",   "按功率累计递增, 断电冻结", ""),
    ("3x", 2,  "正向有功电能", "uint32", "0.01kWh,2寄存器", "R", "0", "同累计递增", ""),
    ("3x", 4,  "A相电压",    "uint16", "V",      "R", "380",  "375~385 波动, 断电0", "偶发暂降-8%"),
    ("3x", 5,  "B相电压",    "uint16", "V",      "R", "381",  "同上", "偶发暂降-8%"),
    ("3x", 6,  "C相电压",    "uint16", "V",      "R", "379",  "同上", "偶发暂降-8%"),
    ("3x", 7,  "A相电流",    "uint16", "x0.1A",  "R", "1500", "120~220A 波动, 断电0", ""),
    ("3x", 8,  "B相电流",    "uint16", "x0.1A",  "R", "1480", "同上", ""),
    ("3x", 9,  "C相电流",    "uint16", "x0.1A",  "R", "1510", "同上", ""),
    ("3x", 10, "总有功功率", "uint16", "kW",     "R", "0",    "80~150kW 平滑波动, 断电0", ""),
    ("3x", 11, "总无功功率", "uint16", "kvar",   "R", "0",    "20~40kvar", ""),
    ("3x", 12, "功率因数",   "uint16", "x0.001", "R", "900",  "0.86~0.95", "需量超限时 0.70~0.82"),
    ("3x", 13, "频率",       "uint16", "x0.01Hz", "R", "5000", "50.00Hz 波动, 断电0", ""),
    ("3x", 14, "报警码",     "uint16", "",        "R", "0",    "0无/1电压暂降/2需量超限", "随机故障时写入"),
    ("3x", 15, "实时需量",   "uint32", "kW,2寄存器", "R", "0", "跟随总有功", ""),
]

ENV_POINTS = [
    ("1x", 0, "高温报警", "bit", "", "R", "0", "高温故障或温度>32C", "高温故障置位"),
    ("1x", 1, "高湿报警", "bit", "", "R", "0", "高湿故障或湿度>70", "高湿故障置位"),
    ("3x", 0, "车间温度", "int16",  "x0.1C", "R", "260",  "向空调设定靠拢 + 随机波动", "偶发高温报警"),
    ("3x", 1, "湿度",     "uint16", "%RH",   "R", "55",   "向目标湿度靠拢 + 随机波动", "偶发高湿报警"),
    ("3x", 2, "露点",     "int16",  "x0.1C", "R", "-50",  "随温湿度变化", ""),
    ("3x", 3, "噪音",     "uint16", "dB",    "R", "72",   "70~80 波动", ""),
    ("4x", 0, "空调设定温度", "int16", "x0.1C", "RW", "260", "控制: 环境温度向此靠拢", ""),
    ("4x", 1, "目标湿度",     "uint16", "%RH",   "RW", "55",  "控制: 环境湿度向此靠拢", ""),
]

# ===========================================================================
# 二、脚本
#   控制点(0x线圈/4x设定): Init 播种, Loop 只读不覆盖
#   传感器(3x) / 状态(1x): 脚本输出
# ===========================================================================

# ---------------- 注塑机 ----------------
IM_INIT = """import random, math
_mb_start = time()
nm = mbdevice.getname()
try:
    idx = int(nm.rsplit('-', 1)[1])
except Exception:
    idx = 0
random.seed(idx * 1009 + 7)
DUR = [20, 30, 25, 150, 40, 15, 20]
TOTAL = sum(DUR)
POFF = (idx % 4) * 75
_p_fault = 0.05
_f1_left = 0
_f2_left = 0
_f3_left = 0
_prev_stage = -1
SET_T = [2200, 2100, 2000, 1900, 1800]
mem4x.setuint16(0, 3)
mem4x.setuint16(1, 50000)
for i in range(5):
    mem4x.setint16(2 + i, SET_T[i])
mem4x.setint16(7, 850)
mem4x.setuint16(8, 25)
mem3x.setuint32(3, 0)
mem3x.setuint32(5, 0)
mem3x.setuint32(7, 0)
mem3x.setuint16(9, TOTAL)
mem3x.setuint16(10, TOTAL)
mem3x.setuint16(11, 92)
for i in range(5):
    mem3x.setint16(19 + i, SET_T[i])
mem3x.setint16(29, 850)
mem3x.setint16(30, 450)
mem3x.setint16(31, 280)
mem3x.setuint32(52, 0)
mem0x.setbit(0, True)
mem0x.setbit(1, True)
mem0x.setbit(2, True)
mem1x.setbit(1, True)
mem1x.setbit(2, True)"""

IM_LOOP = """c = mbdevice.getpycycle()
power = mem0x.getbit(0)
run = mem0x.getbit(1)
auto = mem0x.getbit(2)
mode = mem4x.getuint16(0)
target = mem4x.getuint16(1)
setT = [mem4x.getint16(2 + i) for i in range(5)]
setMold = mem4x.getint16(7)
DUR[2] = mem4x.getuint16(8)
TOTAL = sum(DUR)
p = (c + POFF) % TOTAL
stage = 0
acc = 0
for i, d in enumerate(DUR):
    if p < acc + d:
        stage = i
        break
    acc += d
if _f1_left > 0:
    _f1_left -= 1
else:
    if (c + POFF) % 600 == 0 and random.random() < _p_fault:
        _f1_left = random.randint(80, 200)
f1 = _f1_left > 0
if _f2_left > 0:
    _f2_left -= 1
else:
    if (c + POFF + 200) % 600 == 0 and random.random() < _p_fault:
        _f2_left = random.randint(80, 200)
f2 = _f2_left > 0
if _f3_left > 0:
    _f3_left -= 1
else:
    if (c + POFF + 400) % 600 == 0 and random.random() < _p_fault:
        _f3_left = random.randint(80, 200)
f3 = _f3_left > 0
fault = f1 or f2 or f3
producing = power and run and auto and mode != 0 and not fault and (target <= 0 or mem3x.getuint32(3) < target)
if _prev_stage == 6 and stage == 0 and producing:
    if random.random() < 0.004:
        mem3x.setuint32(7, mem3x.getuint32(7) + 1)
    else:
        mem3x.setuint32(5, mem3x.getuint32(5) + 1)
    mem3x.setuint32(3, mem3x.getuint32(3) + 1)
    mem3x.setuint16(51, mem3x.getuint16(51) + 1)
    cyc = int(TOTAL * random.uniform(0.95, 1.05))
    mem3x.setuint16(9, cyc)
    mem3x.setuint16(10, int((mem3x.getuint16(10) + cyc) / 2))
_prev_stage = stage
st = 0
if power:
    st |= 1
    if producing:
        st |= (1 << (stage + 1))
    if fault:
        st |= (1 << 8)
mem3x.setuint16(0, st)
for i in range(5):
    cur = mem3x.getint16(19 + i)
    if cur <= 0:
        cur = setT[i]
    if not power:
        target_t = 260
    elif not producing:
        target_t = int(setT[i] * 0.82)
    elif fault:
        target_t = setT[i] + random.randint(-30, 10)
    else:
        target_t = setT[i] + random.randint(-8, 8)
    mem3x.setint16(19 + i, int(cur + (target_t - cur) * 0.15))
curm = mem3x.getint16(29)
if not power:
    tm = 260
elif not producing:
    tm = int(setMold * 0.85)
elif f2:
    tm = 1250 + random.randint(-50, 50)
else:
    tm = setMold + random.randint(-5, 5)
mem3x.setint16(29, int(curm + (tm - curm) * 0.1))
curo = mem3x.getint16(30)
if f1:
    mem3x.setint16(30, 580 + random.randint(0, 20))
elif not power:
    mem3x.setint16(30, int(curo + (300 - curo) * 0.05))
else:
    mem3x.setint16(30, int(curo + (450 + random.randint(-10, 10) - curo) * 0.1))
mem3x.setint16(31, 280 + random.randint(-10, 10))
if not power or not producing:
    mem3x.setuint16(39, 0)
    mem3x.setuint16(40, 0)
    mem3x.setuint16(41, 0)
    mem3x.setuint16(42, 0)
    mem3x.setuint16(43, 0)
    mem3x.setuint16(44, 0)
    mem3x.setuint16(48, 0)
    mem3x.setuint16(49, 0)
    mem3x.setuint16(47, 0)
    if f3:
        mem3x.setuint16(39, random.randint(200, 260))
else:
    if stage == 1:
        mem3x.setuint16(39, random.randint(130, 175))
        mem3x.setuint16(40, random.randint(70, 95))
        mem3x.setuint16(45, max(0, mem3x.getuint16(45) - random.randint(1, 5)))
    elif stage == 2:
        mem3x.setuint16(39, random.randint(60, 90))
        mem3x.setuint16(41, random.randint(55, 85))
    elif stage == 4:
        mem3x.setuint16(42, random.randint(150, 240))
        mem3x.setuint16(43, random.randint(8, 20))
        mem3x.setuint16(46, min(200, mem3x.getuint16(46) + random.randint(1, 3)))
    else:
        mem3x.setuint16(39, 0)
        mem3x.setuint16(40, 0)
        mem3x.setuint16(41, 0)
        mem3x.setuint16(42, 0)
        mem3x.setuint16(43, 0)
    if stage in (0, 5, 6):
        mem3x.setuint16(44, random.randint(30, 60))
        mem3x.setuint16(45, 1000)
    mem3x.setuint16(49, random.randint(30, 60) if stage in (1, 2) else random.randint(0, 20))
    mem3x.setuint16(48, random.randint(900, 1500))
    mem3x.setuint16(47, random.randint(20, 90))
mem3x.setuint16(1, (1 if f1 else 0) + (2 if f2 else 0) + (4 if f3 else 0))
mem3x.setuint16(2, 1 if fault else 0)
mem1x.setbit(0, (stage in (0, 5)) and producing)
mem1x.setbit(1, power and not fault)
mem1x.setbit(2, producing)
mem1x.setbit(3, fault)
u = mem3x.getuint16(11)
if fault or (power and not producing):
    if u > 10:
        mem3x.setuint16(11, u - 1)
else:
    if u < 92:
        mem3x.setuint16(11, u + 1)
if power:
    pw = {0: 8, 1: 45, 2: 30, 3: 12, 4: 22, 5: 10, 6: 9}.get(stage, 10)
    mem3x.setuint16(50, (pw + random.randint(-2, 3)) if producing else 3)
    mem3x.setuint32(52, int(time() - _mb_start))
else:
    mem3x.setuint16(50, 0)"""

IM_FINAL = """print(f"{mbdevice.getname()} stopped, total={mem3x.getuint32(3)}")"""

# ---------------- 模温机 ----------------
MTC_INIT = """import random, math
_mb_start = time()
nm = mbdevice.getname()
try:
    idx = int(nm.rsplit('-', 1)[1])
except Exception:
    idx = 0
random.seed(idx * 3001 + 11)
SETT = random.choice([800, 900, 1000, 1100, 1200])
_p_fault = 0.05
_f1_left = 0
_f2_left = 0
_fp = (idx % 4) * 150
mem4x.setuint16(0, SETT)
mem3x.setint16(2, SETT)
mem3x.setint16(3, SETT - random.randint(30, 80))
mem3x.setuint16(4, 3)
mem3x.setuint16(5, 25)
mem0x.setbit(0, True)
mem0x.setbit(1, True)
mem0x.setbit(2, True)
mem1x.setbit(0, True)
mem3x.setuint16(6, 45)"""

MTC_LOOP = """c = mbdevice.getpycycle()
power = mem0x.getbit(0)
run = mem0x.getbit(1)
heat = mem0x.getbit(2)
setT = mem4x.getuint16(0)
if _f1_left > 0:
    _f1_left -= 1
else:
    if (c + _fp) % 600 == 0 and random.random() < _p_fault:
        _f1_left = random.randint(100, 250)
f1 = _f1_left > 0
if _f2_left > 0:
    _f2_left -= 1
else:
    if (c + _fp + 200) % 600 == 0 and random.random() < _p_fault:
        _f2_left = random.randint(100, 250)
f2 = _f2_left > 0
cur = mem3x.getint16(2)
if f1:
    target = setT + random.randint(50, 120)
elif not power:
    target = 250
elif not run:
    target = 300
elif heat:
    target = setT + random.randint(-10, 10)
else:
    target = max(250, cur - 10)
mem3x.setint16(2, int(cur + (target - cur) * 0.08))
curb = mem3x.getint16(3)
mem3x.setint16(3, int(curb + (mem3x.getint16(2) - 40 - curb) * 0.1))
dev = mem3x.getint16(2) - setT
st = 0
if power:
    st |= 1
    if run:
        st |= 2
    if power and run and heat and dev < -20:
        st |= 4
    elif dev > 20:
        st |= 8
    if f1 or f2:
        st |= 0x10
mem3x.setuint16(0, st)
mem3x.setuint16(1, (1 if f1 else 0) + (2 if f2 else 0))
mem1x.setbit(2, f1 or f2)
mem1x.setbit(0, power)
mem1x.setbit(1, power and run and heat and dev < -20)
if not power or not run or f2:
    mem3x.setuint16(4, 0)
    mem3x.setuint16(5, 0)
else:
    mem3x.setuint16(4, 2 + random.randint(0, 3))
    mem3x.setuint16(5, random.randint(15, 40))
mem3x.setuint16(6, random.randint(30, 60) if power else 0)"""

MTC_FINAL = """print(f"{mbdevice.getname()} stopped")"""

# ---------------- 干燥机 ----------------
DRY_INIT = """import random, math
_mb_start = time()
nm = mbdevice.getname()
try:
    idx = int(nm.rsplit('-', 1)[1])
except Exception:
    idx = 0
random.seed(idx * 4021 + 17)
SETD = int(random.choice([750, 800, 850, 900]))
_p_fault = 0.05
_f1_left = 0
_f2_left = 0
_fp = (idx % 4) * 150
_level = 85.0
mem4x.setuint16(0, SETD)
mem3x.setint16(2, SETD)
mem3x.setint16(3, -350)
mem3x.setuint16(4, 85)
mem3x.setuint16(5, 5)
mem0x.setbit(0, True)
mem0x.setbit(1, True)
mem0x.setbit(2, True)
mem0x.setbit(3, True)
mem1x.setbit(0, True)
mem1x.setbit(1, True)"""

DRY_LOOP = """c = mbdevice.getpycycle()
power = mem0x.getbit(0)
run = mem0x.getbit(1)
fan = mem0x.getbit(2)
heat = mem0x.getbit(3)
setD = mem4x.getuint16(0)
if _f1_left > 0:
    _f1_left -= 1
else:
    if (c + _fp) % 600 == 0 and random.random() < _p_fault:
        _f1_left = random.randint(100, 250)
f1 = _f1_left > 0
if _f2_left > 0:
    _f2_left -= 1
else:
    if (c + _fp + 200) % 600 == 0 and random.random() < _p_fault:
        _f2_left = random.randint(100, 250)
f2 = _f2_left > 0
cur = mem3x.getint16(2)
if f2:
    target = setD + random.randint(30, 60)
elif not power:
    target = 250
elif not run or not heat:
    target = max(250, cur - 10)
else:
    target = setD + random.randint(-6, 6)
mem3x.setint16(2, int(cur + (target - cur) * 0.08))
curd = mem3x.getint16(3)
if f1:
    tde = -100 + random.randint(-40, 20)
else:
    tde = -340 + random.randint(-30, 30)
mem3x.setint16(3, int(curd + (tde - curd) * 0.08))
if power and run and not f1 and not f2:
    _level -= random.uniform(0.01, 0.05)
    if _level < 20:
        _level = 90
    mem3x.setuint16(4, int(_level))
st = 0
if power:
    st |= 1
    if run:
        st |= 2
    if fan:
        st |= 4
    if heat:
        st |= 8
    if mem3x.getuint16(4) < 25:
        st |= 0x10
    if f1 or f2:
        st |= 0x20
mem3x.setuint16(0, st)
mem3x.setuint16(1, (1 if f1 else 0) + (2 if f2 else 0))
mem1x.setbit(2, mem3x.getuint16(4) < 25)
mem1x.setbit(3, f1 or f2)
mem1x.setbit(0, power)
mem1x.setbit(1, power and run and fan and heat)
mem3x.setuint16(5, random.randint(3, 8) if heat else 0)
mem3x.setuint16(6, random.randint(8, 20) if fan else 0)"""

DRY_FINAL = """print(f"{mbdevice.getname()} stopped, level={mem3x.getuint16(4)}")"""

# ---------------- 机械手 ----------------
ROB_INIT = """import random, math
_mb_start = time()
nm = mbdevice.getname()
try:
    idx = int(nm.rsplit('-', 1)[1])
except Exception:
    idx = 0
random.seed(idx * 2027 + 3)
TOTAL = 310
WAIT = 270
POFF = (idx % 4) * 75
_p_fault = 0.05
_f1_left = 0
_f2_left = 0
_prev_stage = -1
_manual_left = 0
_start_prev = False
_px = 0.0
_py = 0.0
_pz = 0.0
mem3x.setuint32(3, 0)
mem3x.setuint32(5, 0)
mem3x.setuint32(7, 0)
mem3x.setuint16(2, 30)
mem0x.setbit(0, True)
mem0x.setbit(1, True)
mem0x.setbit(2, False)
mem1x.setbit(3, True)
mem1x.setbit(0, True)"""

ROB_LOOP = """c = mbdevice.getpycycle()
power = mem0x.getbit(0)
auto = mem0x.getbit(1)
start_req = mem0x.getbit(2)
if (not auto) and start_req and (not _start_prev) and _manual_left <= 0:
    _manual_left = 35
_start_prev = start_req
p = (c + POFF) % TOTAL
if _f1_left > 0:
    _f1_left -= 1
else:
    if (c + POFF) % 600 == 0 and random.random() < _p_fault:
        _f1_left = random.randint(60, 150)
f1 = _f1_left > 0
if _f2_left > 0:
    _f2_left -= 1
else:
    if (c + POFF + 200) % 600 == 0 and random.random() < _p_fault:
        _f2_left = random.randint(60, 150)
f2 = _f2_left > 0
fault = f1 or f2
if auto:
    if p < WAIT:
        stage = 0
    elif p < WAIT + 20:
        stage = 1
    elif p < WAIT + 35:
        stage = 2
    else:
        stage = 3
else:
    if _manual_left > 0:
        _manual_left -= 1
        if _manual_left > 20:
            stage = 1
        elif _manual_left > 5:
            stage = 2
        else:
            stage = 3
    else:
        stage = 0
if _prev_stage == 3 and stage == 0 and not fault:
    mem3x.setuint32(3, mem3x.getuint32(3) + 1)
    if random.random() < 0.02:
        mem3x.setuint32(7, mem3x.getuint32(7) + 1)
    else:
        mem3x.setuint32(5, mem3x.getuint32(5) + 1)
_prev_stage = stage
st = 0
if power:
    st |= 1
    if not fault:
        if stage == 0:
            st |= 2
        if stage == 1:
            st |= 4
        if stage == 2:
            st |= 8
        if stage == 3:
            st |= 16
        if auto:
            st |= 64
    else:
        st |= 32
mem3x.setuint16(0, st)
_px += random.uniform(-0.5, 0.5)
_py += random.uniform(-0.5, 0.5)
_pz += random.uniform(-0.5, 0.5)
if stage == 0:
    x = max(0, min(8, int(_px)))
    y = max(0, min(8, int(_py)))
    z = max(0, min(8, int(_pz)))
elif stage == 1:
    x = 100 + random.randint(-5, 5)
    y = 300 + random.randint(-5, 5)
    z = 80 + random.randint(-5, 5)
elif stage == 2:
    x = 0
    y = 400 + random.randint(-5, 5)
    z = 200 + random.randint(-5, 5)
else:
    x = random.randint(0, 10)
    y = random.randint(0, 10)
    z = random.randint(0, 10)
mem3x.setuint16(9, x)
mem3x.setuint16(10, y)
mem3x.setuint16(11, z)
mem3x.setuint16(12, 1 if (stage in (1, 2) and not fault) else 0)
if fault:
    mem3x.setuint16(1, (1 if f1 else 0) + (2 if f2 else 0))
    mem1x.setbit(2, True)
    mem1x.setbit(0, False)
else:
    mem3x.setuint16(1, 0)
    mem1x.setbit(2, False)
    mem1x.setbit(0, power)
mem1x.setbit(1, (stage in (1, 2) and not fault))
mem3x.setuint16(13, random.randint(2, 8) if power else 0)
mem3x.setuint16(14, stage if power else 0)
mem3x.setuint16(2, int(30 * random.uniform(0.9, 1.1)))"""

ROB_FINAL = """print(f"{mbdevice.getname()} stopped, taken={mem3x.getuint32(3)}")"""

# ---------------- 总电表 ----------------
EM_INIT = """import random, math
_mb_start = time()
random.seed(999)
_p_fault = 0.05
_f1_left = 0
_f2_left = 0
mem3x.setuint32(0, 0)
mem3x.setuint32(2, 0)
mem3x.setuint16(4, 380)
mem3x.setuint16(5, 381)
mem3x.setuint16(6, 379)
mem3x.setuint16(7, 1500)
mem3x.setuint16(8, 1480)
mem3x.setuint16(9, 1510)
mem3x.setuint16(10, 100)
mem3x.setuint16(11, 30)
mem3x.setuint16(12, 900)
mem3x.setuint16(13, 5000)
mem3x.setuint16(14, 0)
mem0x.setbit(0, True)
mem1x.setbit(0, True)"""

EM_LOOP = """c = mbdevice.getpycycle()
on = mem0x.getbit(0)
if _f1_left > 0:
    _f1_left -= 1
else:
    if c % 600 == 0 and random.random() < _p_fault:
        _f1_left = random.randint(80, 200)
f1 = _f1_left > 0
if _f2_left > 0:
    _f2_left -= 1
else:
    if (c + 300) % 600 == 0 and random.random() < _p_fault:
        _f2_left = random.randint(80, 200)
f2 = _f2_left > 0
if not on:
    mem3x.setuint16(4, 0)
    mem3x.setuint16(5, 0)
    mem3x.setuint16(6, 0)
    mem3x.setuint16(7, 0)
    mem3x.setuint16(8, 0)
    mem3x.setuint16(9, 0)
    mem3x.setuint16(10, 0)
    mem3x.setuint16(11, 0)
    mem3x.setuint16(12, 0)
    mem3x.setuint16(13, 0)
    mem3x.setuint16(14, 0)
    mem1x.setbit(1, False)
else:
    va = 380 + random.randint(-5, 5)
    vb = 381 + random.randint(-5, 5)
    vc = 379 + random.randint(-5, 5)
    if f1:
        va = int(va * 0.92)
        vb = int(vb * 0.92)
        vc = int(vc * 0.92)
    mem3x.setuint16(4, va)
    mem3x.setuint16(5, vb)
    mem3x.setuint16(6, vc)
    base = random.randint(120, 220)
    mem3x.setuint16(7, base * 10 + random.randint(-15, 15))
    mem3x.setuint16(8, base * 10 - random.randint(0, 20) + random.randint(-10, 10))
    mem3x.setuint16(9, base * 10 + random.randint(-10, 20) + random.randint(-10, 10))
    cur_kw = mem3x.getuint16(10)
    tgt_kw = random.randint(80, 150)
    mem3x.setuint16(10, int(cur_kw + (tgt_kw - cur_kw) * 0.3))
    mem3x.setuint16(11, random.randint(20, 40))
    if f2:
        mem3x.setuint16(12, random.randint(700, 820))
    else:
        mem3x.setuint16(12, random.randint(860, 950))
    mem3x.setuint16(13, 5000 + random.randint(-10, 10))
    inc = max(1, int(mem3x.getuint16(10) * 0.1 / 36))
    mem3x.setuint32(0, mem3x.getuint32(0) + inc)
    mem3x.setuint32(2, mem3x.getuint32(2) + inc)
    mem3x.setuint16(14, (1 if f1 else 0) + (2 if f2 else 0))
    mem1x.setbit(1, f2 or mem3x.getuint16(12) < 800)
mem3x.setuint32(15, mem3x.getuint32(0) if on else 0)"""

EM_FINAL = """print(f"{mbdevice.getname()} stopped, energy={mem3x.getuint32(0)}")"""

# ---------------- 车间环境 ----------------
ENV_INIT = """import random, math
_mb_start = time()
random.seed(555)
_p_fault = 0.05
_f1_left = 0
_f2_left = 0
_t = 260.0
_h = 55.0
mem4x.setint16(0, 260)
mem4x.setuint16(1, 55)
mem3x.setint16(0, 260)
mem3x.setuint16(1, 55)
mem3x.setint16(2, -50)
mem3x.setuint16(3, 72)
mem1x.setbit(0, False)
mem1x.setbit(1, False)"""

ENV_LOOP = """c = mbdevice.getpycycle()
setT = mem4x.getint16(0)
setH = mem4x.getuint16(1)
if _f1_left > 0:
    _f1_left -= 1
else:
    if c % 600 == 0 and random.random() < _p_fault:
        _f1_left = random.randint(150, 300)
f1 = _f1_left > 0
if _f2_left > 0:
    _f2_left -= 1
else:
    if (c + 300) % 600 == 0 and random.random() < _p_fault:
        _f2_left = random.randint(150, 300)
f2 = _f2_left > 0
if f1:
    _t += 0.6
else:
    _t += (setT - _t) * 0.02
_t += random.uniform(-0.3, 0.3)
if _t > 600:
    _t = 600
if _t < -300:
    _t = -300
mem3x.setint16(0, int(_t))
if f2:
    _h += 1.0
else:
    _h += (setH - _h) * 0.03
_h += random.uniform(-1.0, 1.0)
if _h > 100:
    _h = 100
if _h < 0:
    _h = 0
mem3x.setuint16(1, int(_h))
mem3x.setint16(2, int(_t - (100 - _h) * 2))
mem3x.setuint16(3, 70 + random.randint(0, 8))
mem1x.setbit(0, f1 or mem3x.getint16(0) > 320)
mem1x.setbit(1, f2 or mem3x.getuint16(1) > 70)"""

ENV_FINAL = """print(f"{mbdevice.getname()} stopped")"""

# ===========================================================================
# 三、站号规划 与 设备实例
# ===========================================================================
STATION_PLAN = []
for i in range(1, 5):
    base = (i - 1) * 4 + 1
    STATION_PLAN.append(("IM-%02d" % i, base,     IM_POINTS, IM_INIT, IM_LOOP, IM_FINAL, "注塑机"))
    STATION_PLAN.append(("MTC-%02d" % i, base + 1, MTC_POINTS, MTC_INIT, MTC_LOOP, MTC_FINAL, "模温机"))
    STATION_PLAN.append(("DRY-%02d" % i, base + 2, DRY_POINTS, DRY_INIT, DRY_LOOP, DRY_FINAL, "干燥机"))
    STATION_PLAN.append(("ROB-%02d" % i, base + 3, ROB_POINTS, ROB_INIT, ROB_LOOP, ROB_FINAL, "机械手"))
STATION_PLAN.append(("EM-01", 61, EM_POINTS, EM_INIT, EM_LOOP, EM_FINAL, "总电表"))
STATION_PLAN.append(("ENV-01", 71, ENV_POINTS, ENV_INIT, ENV_LOOP, ENV_FINAL, "车间环境"))

# 3x/1x 按需容量（传感器数量决定的实际大小）
COUNT3X = {"注塑机": 200, "模温机": 20, "干燥机": 20, "机械手": 40, "总电表": 40, "车间环境": 20}
COUNT1X = 20

DEVICE_TYPES = {
    "注塑机": IM_POINTS, "模温机": MTC_POINTS, "干燥机": DRY_POINTS,
    "机械手": ROB_POINTS, "总电表": EM_POINTS, "车间环境": ENV_POINTS,
}

def make_ports(base=INTERNAL_BASE):
    ports = []
    for i, (name, unit, _, _, _, _, _) in enumerate(STATION_PLAN):
        ports.append({
            "name": "P-%s" % name,
            "type": "TCP",
            "host": "0.0.0.0",
            "port": str(base + i),
            "maxconn": "32",
            "timeout": "30000",
            "timeoutFirstByte": "1000",
            "timeoutInterByte": "50",
            "isBroadcastEnabled": "false",
            "_device": name,
            "_unit": str(unit),
        })
    return ports

PORTS = make_ports()

# ===========================================================================
# 四、生成 XML
# ===========================================================================

def device_xml(name, init, loop, final, devtype):
    dev = ET.Element("device")
    ET.SubElement(dev, "byteArrayFormat").text = "Hex"
    ET.SubElement(dev, "byteArraySeparator").text = r"\s"
    ET.SubElement(dev, "count0x").text = "16"
    ET.SubElement(dev, "count1x").text = str(COUNT1X)
    ET.SubElement(dev, "count3x").text = str(COUNT3X[devtype])
    ET.SubElement(dev, "count4x").text = "16"
    ET.SubElement(dev, "delay").text = "0"
    ET.SubElement(dev, "exceptionStatusAddress").text = "1"
    ET.SubElement(dev, "isEnableScript").text = "true"
    ET.SubElement(dev, "isReadOnly").text = "false"
    ET.SubElement(dev, "isSaveData").text = "false"
    ET.SubElement(dev, "maxReadCoils").text = "2040"
    ET.SubElement(dev, "maxReadDiscreteInputs").text = "2040"
    ET.SubElement(dev, "maxReadHoldingRegisters").text = "125"
    ET.SubElement(dev, "maxReadInputRegisters").text = "125"
    ET.SubElement(dev, "maxWriteMultipleCoils").text = "127"
    ET.SubElement(dev, "maxWriteMultipleRegisters").text = "123"
    ET.SubElement(dev, "name").text = name
    ET.SubElement(dev, "registerOrder").text = "R0R1R2R3"
    ET.SubElement(dev, "scriptFinal").text = final
    ET.SubElement(dev, "scriptInit").text = init
    ET.SubElement(dev, "scriptLoop").text = loop
    ET.SubElement(dev, "stringEncoding").text = "UTF-8"
    ET.SubElement(dev, "stringLengthType").text = "ZerroEnded"
    ET.SubElement(dev, "swapBytes").text = "SwapNo"
    return dev


def build_xml():
    root = ET.Element("project", version="0.5.0", editnum="1")
    ET.SubElement(root, "name").text = "注塑工厂数据桩"
    ET.SubElement(root, "author").text = "modbus-stub"
    ET.SubElement(root, "comment").text = (
        "注塑工厂 Modbus 模拟: 18 设备各自独立 TCP 端口(%s~%s) + 标准区域语义(0x控制/1x状态/3x传感器/4x设定) + 可交互控制"
        % (PORTS[0]["port"], PORTS[-1]["port"]))
    ports = ET.SubElement(root, "ports")
    for p in PORTS:
        port = ET.SubElement(ports, "port")
        for k, v in p.items():
            if k.startswith("_"):
                continue
            ET.SubElement(port, k).text = v
        refs = ET.SubElement(port, "deviceref")
        ET.SubElement(refs, "deviceref", name=p["_device"]).text = p["_unit"]
    devices = ET.SubElement(root, "devices")
    for name, unit, _, init, loop, final, devtype in STATION_PLAN:
        devices.append(device_xml(name, init, loop, final, devtype))
    return root

# ===========================================================================
# 五、生成 Excel 点表
# ===========================================================================

def addr_str(area, offset):
    base = {"4x": 40001, "3x": 30001, "0x": 1, "1x": 10001}[area]
    return "%d" % (base + offset)

EXCEL_HEADERS = ["站号", "设备", "数据区", "地址", "偏移", "含义", "英文命名", "数据类型", "缩放/单位", "读写", "初值", "模拟规则", "异常模拟"]

# 含义 -> 英文变量名（snake_case，工业组态常用术语），点表 G 列即由此生成
EN_NAME = {
    # 通用
    "电源": "power", "运行": "running", "就绪": "ready", "报警": "alarm",
    "报警码": "alarm_code", "运行状态字": "run_status_word",
    "瞬时功率": "instantaneous_power",
    # 注塑机
    "自动模式": "auto_mode", "合模": "mold_close", "生产中": "producing",
    "报警组字": "alarm_group_word", "总模数": "total_shots",
    "合格品数": "good_parts", "次品数": "reject_parts",
    "当前周期时间": "current_cycle_time", "平均周期时间": "avg_cycle_time",
    "稼动率": "operating_rate",
    "料筒1段温度": "barrel_temp_1", "料筒2段温度": "barrel_temp_2",
    "料筒3段温度": "barrel_temp_3", "料筒4段温度": "barrel_temp_4",
    "料筒5段温度": "barrel_temp_5",
    "模具实际温度": "mold_temp_actual", "液压油温": "hydraulic_oil_temp",
    "冷却水温度": "cooling_water_temp", "射胶压力": "injection_pressure",
    "射胶速度": "injection_speed", "保压压力": "holding_pressure",
    "螺杆转速": "screw_speed", "熔胶背压": "melt_back_pressure",
    "合模压力": "clamp_pressure", "注射位置": "injection_position",
    "螺杆位置": "screw_position", "泵电机电流": "pump_motor_current",
    "泵电机转速": "pump_motor_speed", "液压泵压力": "hydraulic_pump_pressure",
    "当班产量": "shift_output", "当班开机时长": "shift_uptime",
    "工作模式": "work_mode", "目标产量": "target_output",
    "料筒1段设定": "barrel_setpoint_1", "料筒2段设定": "barrel_setpoint_2",
    "料筒3段设定": "barrel_setpoint_3", "料筒4段设定": "barrel_setpoint_4",
    "料筒5段设定": "barrel_setpoint_5",
    "模具设定温度": "mold_temp_setpoint", "保压时间": "holding_time",
    # 模温机
    "加热": "heater_on", "加热中": "heating", "实际温度": "temp_actual",
    "回水温度": "return_water_temp", "泵压力": "pump_pressure",
    "泵流量": "pump_flow", "泵电流": "pump_current", "设定温度": "temp_setpoint",
    # 干燥机
    "风机": "fan_on", "干燥中": "drying", "料位低": "level_low",
    "干燥温度实际": "dry_temp_actual", "露点": "dew_point",
    "料位": "material_level", "加热功率": "heating_power",
    "风机电流": "fan_current", "干燥温度设定": "dry_temp_setpoint",
    # 机械手
    "自动": "auto", "启动取件": "start_pick", "夹持到位": "part_gripped",
    "安全门关闭": "safety_door_closed", "循环时间": "cycle_time",
    "累计取件数": "total_picks", "取出成功数": "pick_success",
    "取出失败数": "pick_fail",
    "X轴位置": "x_position", "Y轴位置": "y_position", "Z轴位置": "z_position",
    "抓取状态": "grip_status", "当前动作号": "current_action_id",
    # 总电表
    "远程合闸": "remote_close", "通讯状态": "comm_status",
    "需量超限": "demand_over_limit", "累计电能": "total_energy",
    "正向有功电能": "forward_active_energy",
    "A相电压": "voltage_a", "B相电压": "voltage_b", "C相电压": "voltage_c",
    "A相电流": "current_a", "B相电流": "current_b", "C相电流": "current_c",
    "总有功功率": "total_active_power", "总无功功率": "total_reactive_power",
    "功率因数": "power_factor", "频率": "frequency", "实时需量": "realtime_demand",
    # 车间环境
    "高温报警": "high_temp_alarm", "高湿报警": "high_humidity_alarm",
    "车间温度": "workshop_temp", "湿度": "humidity", "噪音": "noise_level",
    "空调设定温度": "hvac_temp_setpoint", "目标湿度": "humidity_setpoint",
}


def write_sheet(wb, title, dtype, points, stations):
    ws = wb.create_sheet(title)
    ws.append(EXCEL_HEADERS)
    for cell in ws[1]:
        cell.font = cell.font.copy(bold=True)
    for (name, unit, _, _, _, _, _) in stations:
        for p in points:
            area, off, desc, typ, scale, rw, init, rule, anomaly = p
            ws.append([unit, name, area.upper(), addr_str(area, off), off, desc,
                       EN_NAME.get(desc, ""), typ, scale, rw, init, rule, anomaly])
    ws.freeze_panes = "A2"
    widths = [6, 12, 6, 8, 6, 16, 22, 9, 12, 6, 8, 34, 24]
    for i, w in enumerate(widths, 1):
        ws.column_dimensions[chr(64 + i)].width = w


def build_excel(path, internal_base=INTERNAL_BASE):
    import openpyxl
    wb = openpyxl.Workbook()
    ws = wb.active
    ws.title = "总览"
    ws.append(["端口", "站号", "设备", "类型"])
    for cell in ws[1]:
        cell.font = cell.font.copy(bold=True)
    for i, (name, unit, _, _, _, _, typ) in enumerate(STATION_PLAN):
        ws.append([PUBLIC_BASE + i, unit, name, typ])
    ws.append([])
    ws.append(["说明", "18 个设备各占一个独立 TCP 端口，端口即设备；站号保留 1~16/61/71。"])
    if internal_base != PUBLIC_BASE:
        ws.append(["说明", "mbserver 实际监听内部端口 %d~%d，经 breaker.py 断路器代理映射为对外 %d~%d；"
                           "单设备掉线模拟见 breaker_rules.json 与交接文档。" % (
                               internal_base, internal_base + len(STATION_PLAN) - 1,
                               PUBLIC_BASE, PUBLIC_BASE + len(STATION_PLAN) - 1)])
    else:
        ws.append(["说明", "直连模式：mbserver 直接监听 0.0.0.0:%d~%d（无代理）。" % (
            PUBLIC_BASE, PUBLIC_BASE + len(STATION_PLAN) - 1)])
    ws.append(["说明", "0x线圈=控制输出(RW), 1x离散输入=状态反馈(RO), 3x输入寄存器=传感器/测量值/状态字/报警码(RO), 4x保持寄存器=控制参数设定值(RW)。"])
    ws.append(["说明", "控制点(RW)：脚本只读不覆盖，上位机写入即生效——设定温度/目标产量/工作模式(4x) + 电源/运行/加热/自动等线圈(0x)。实际值带热惯性向设定靠拢。"])
    ws.append(["说明", "随机偶发故障作为背景保留(5%/分钟/机制)。故障概率 _p_fault 可在脚本开头调整。"])
    ws.append(["说明", "uint32 读时序: 低字在前(如总模数 30004=低16位, 30005=高16位)。"])
    for c in ("A", "B", "C", "D"):
        ws.column_dimensions[c].width = 12 if c != "D" else 90
    for typ, points in DEVICE_TYPES.items():
        stations = [s for s in STATION_PLAN if s[6] == typ]
        write_sheet(wb, typ, typ, points, stations)
    wb.save(path)


# ===========================================================================
# 六、备份 + 主流程
# ===========================================================================

def backup_files(output, excel):
    ts = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    os.makedirs(BAK_DIR, exist_ok=True)
    made = []
    if os.path.exists(output):
        dst = os.path.join(BAK_DIR, "stub_%s.mbs" % ts)
        shutil.copy2(output, dst)
        made.append(dst)
    if os.path.exists(__file__):
        dst = os.path.join(BAK_DIR, "make_stub_%s.py" % ts)
        shutil.copy2(__file__, dst)
        made.append(dst)
    for m in made:
        print("备份:", m)


def write_rules_template(path):
    """生成断路器代理的掉线规则模板（已存在则保留用户编辑，不覆盖）。"""
    if os.path.exists(path):
        print("保留已有:", path)
        return
    devices = {}
    for (name, _unit, _pts, _init, _loop, _final, _typ) in STATION_PLAN:
        devices[name] = {"mode": "refuse"}
    data = {
        "_说明": ("breaker.py 断路器代理的单设备掉线规则。"
                 "schedule=定时窗口: 每 every_min 分钟掉线一次、持续 offline_sec 秒（支持小数，测试可写 0.1）；"
                 "random=随机: 每分钟 prob_per_min% 概率掉线、持续 dur_min~dur_max 秒随机；"
                 "mode: refuse=断电式(连接被拒/重置) silent=黑洞式(连接保留但无响应)。"
                 "删除 devices 里对应键或字段即停用；手动控制：运行 breaker.py 后输入 help。"),
        "defaults": {"mode": "refuse"},
        "devices": devices,
    }
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")
    print("OK:", path)


def main():
    ap = argparse.ArgumentParser(description="生成注塑工厂 Modbus 数据桩")
    ap.add_argument("-o", "--output", default="stub.mbs")
    ap.add_argument("--excel", default="点表.xlsx")
    ap.add_argument("--internal-base", type=int, default=INTERNAL_BASE,
                    help="mbserver 监听端口基准，默认 %d（对外由 breaker.py 代理为 %d~%d；"
                         "写 %d 可回退直连模式）" % (INTERNAL_BASE, PUBLIC_BASE, PUBLIC_BASE + 17, PUBLIC_BASE))
    ap.add_argument("--rules", default="breaker_rules.json",
                    help="断路器规则模板输出路径（已存在则不覆盖）")
    ap.add_argument("--no-excel", action="store_true", help="跳过生成点表.xlsx")
    args = ap.parse_args()

    global PORTS
    PORTS = make_ports(args.internal_base)

    backup_files(args.output, args.excel)

    root = build_xml()
    ET.indent(root, space="    ")
    xml = '<?xml version="1.0" encoding="UTF-8"?>\n' + ET.tostring(root, encoding="unicode") + "\n"
    with open(args.output, "w", encoding="utf-8") as f:
        f.write(xml)
    if not args.no_excel:
        build_excel(args.excel, args.internal_base)
    write_rules_template(args.rules)

    print("OK: %s (18 从站, mbserver 监听 %d~%d)"
          % (args.output, args.internal_base, args.internal_base + len(PORTS) - 1))
    if args.internal_base != PUBLIC_BASE:
        print("   代理模式: 上位机连 %s~%s（需先启动 breaker.py），mbserver 内部端口 %d~%d"
              % (PUBLIC_BASE, PUBLIC_BASE + len(PORTS) - 1, args.internal_base, args.internal_base + len(PORTS) - 1))
    for (name, unit, _, _, _, _, typ) in STATION_PLAN:
        print("  station %3d  %-7s  %s" % (unit, name, typ))


if __name__ == "__main__":
    main()
