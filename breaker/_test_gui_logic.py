#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""breaker_gui 逻辑层测试（TDD，先于实现运行应为红）。"""
import argparse
import sys
import time

import breaker
from breaker import Device, OFFLINE, ONLINE

from breaker_gui import device_note, device_rows, parse_duration

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


def mkdev(**kw):
    d = Device(1, "IM-01", 1, "注塑机", 502, 15502)
    for k, v in kw.items():
        setattr(d, k, v)
    return d


def test_note():
    now = time.time()
    d = mkdev(state=OFFLINE, reason="手动", offline_since=now - 45)
    check("掉线说明含原因与秒数", device_note(d, now) == "掉线:手动 已45s")
    d2 = mkdev(state=OFFLINE, reason="定时窗口", offline_since=now - 2.4)
    check("掉线说明秒数取整", device_note(d2, now) == "掉线:定时窗口 已2s")
    d3 = mkdev(state=OFFLINE, reason="手动", offline_since=None)
    check("掉线缺 offline_since 不炸", "掉线:手动" in device_note(d3, now))
    d4 = mkdev(state=ONLINE, auto_desc="定时:每30分掉60s")
    check("在线说明显示规则", device_note(d4, now) == "定时:每30分掉60s")
    d5 = mkdev(state=ONLINE, auto_desc="")
    check("在线无规则显示-", device_note(d5, now) == "-")


def test_rows():
    args = argparse.Namespace(public_base=502, internal_base=15502,
                              internal_host="127.0.0.1", rules="")
    b = breaker.Breaker(args)
    rows = device_rows(b.devices, time.time())
    check("行数=设备数", len(rows) == 18)
    first = rows[0]
    check("首行 IM-01 502", first[:4] == (1, "IM-01", 502, "在线"))
    check("行含模式/连接/说明", len(first) == 7 and first[4] == "refuse" and first[5] == 0)
    last = rows[-1]
    check("末行 ENV-01 519", last[1] == "ENV-01" and last[2] == 519)


def test_parse_duration():
    check("空→None", parse_duration("") is None)
    check("0→None", parse_duration("0") is None)
    check("非法→None", parse_duration("abc") is None)
    check("负数→None", parse_duration("-3") is None)
    check("30→30.0", parse_duration("30") == 30.0)
    check("2.5→2.5", parse_duration("2.5") == 2.5)


def test_log_sink():
    got = []
    remove = breaker.add_log_sink(got.append)
    breaker.log("测试消息XYZ")
    remove()
    breaker.log("不应收到")
    check("sink 收到带时间戳日志", any("测试消息XYZ" in line for line in got))
    check("移除后不再收到", not any("不应收到" in line for line in got))
    check("日志行带时间戳", got and got[0].startswith("["))


if __name__ == "__main__":
    test_note()
    test_rows()
    test_parse_duration()
    test_log_sink()
    print("== 结果: PASS=%d FAIL=%d ==" % (PASS, FAIL))
    sys.exit(1 if FAIL else 0)
