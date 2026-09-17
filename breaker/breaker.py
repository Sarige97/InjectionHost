#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
注塑工厂 Modbus 数据桩 —— 断路器代理（单设备掉线模拟）
======================================================
架在 上位机 与 mbserver 之间：
  上位机 → 对外端口(public-base 502~519) → 本代理 → 内部端口(internal-base 15502~15519) → mbserver

背景：mbserver 只有整个工程的整体启停，设备脚本跑在独立 Python 进程里只能
读写寄存器、控制不了 TCP 连接，所以在外面包一层可编程断路器来实现
"某台设备单独掉线"（不改 mbserver、不重编译；上位机端口表不变）。

掉线模式：
  refuse  断电式(默认)：关监听 + 掐断所有连接 → 上位机连接被拒/重置，最接近设备断电
  silent  黑洞式：连接保留但请求全部吞掉 → 上位机读超时，模拟"网线拔了设备没死"

触发方式（叠加生效）：
  1. 手动控制台  status / off <设备> [模式] [秒] / on <设备> / off all / on all / help / quit
  2. 定时窗口    breaker_rules.json → "schedule": {"every_min": 30, "offline_sec": 60}
  3. 随机触发    breaker_rules.json → "random": {"prob_per_min": 5, "dur_min": 8, "dur_max": 30}

用法：
  python breaker.py [--public-base 502] [--internal-base 15502]
                    [--internal-host 127.0.0.1] [--rules breaker_rules.json]

设备清单来自同目录 make_stub.py 的 STATION_PLAN（与 stub.mbs 一端口一设备同序）。
mbserver 的规则模板 breaker_rules.json 由 make_stub.py 自动生成。
"""

import argparse
import asyncio
import json
import os
import random
import socket
import sys
import threading
import time
from datetime import datetime

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
try:
    from make_stub import STATION_PLAN, PUBLIC_BASE
except ImportError:
    print("错误: 找不到 make_stub.py（需与 breaker.py 同目录，从中读取设备清单 STATION_PLAN）")
    sys.exit(1)

ONLINE, OFFLINE = "在线", "掉线"

_print_lock = threading.Lock()
_log_sinks = []  # 额外日志接收器（如 GUI），签名 sink(line)，收到带时间戳的整行


def add_log_sink(sink):
    """注册日志接收器，返回移除函数。"""
    _log_sinks.append(sink)

    def remove():
        if sink in _log_sinks:
            _log_sinks.remove(sink)
    return remove


def log(msg):
    line = "[%s] %s" % (datetime.now().strftime("%H:%M:%S"), msg)
    with _print_lock:
        print(line, flush=True)
        for sink in list(_log_sinks):
            try:
                sink(line)
            except Exception:
                pass


def _make_listen_sock(port):
    """手动建监听 socket：开 SO_REUSEADDR，保证掉线后立即恢复时同一端口可重新绑定
    （Windows 上 asyncio 默认不开 reuse，快速 off/on 会 WSAEADDRINUSE）。"""
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    sock.bind(("0.0.0.0", port))
    sock.listen(128)
    sock.setblocking(False)
    return sock


class Pair(object):
    """一条经代理的连接：client(上位机) <-> backend(mbserver)"""

    def __init__(self, dev, creader, cwriter, breaker):
        self.dev = dev
        self.breaker = breaker
        self.creader = creader
        self.cwriter = cwriter
        self.breader = None
        self.bwriter = None
        self.b2c_task = None
        self._set_nodelay(cwriter)

    @staticmethod
    def _set_nodelay(writer):
        sock = writer.get_extra_info("socket")
        if sock is not None:
            try:
                sock.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
            except OSError:
                pass

    async def connect_backend(self):
        try:
            self.breader, self.bwriter = await asyncio.open_connection(
                self.breaker.internal_host, self.dev.internal_port)
        except OSError:
            return False
        self._set_nodelay(self.bwriter)
        return True

    def start_b2c(self):
        self.b2c_task = asyncio.create_task(self._b2c())

    async def _b2c(self):
        """后端 → 客户端方向。"""
        try:
            while True:
                data = await self.breader.read(4096)
                if not data:
                    break
                self.cwriter.write(data)
                await self.cwriter.drain()
        except (ConnectionError, OSError):
            pass
        finally:
            # 在线时后端断开（如 mbserver 30s 空闲超时）→ 关客户端让上位机重连；
            # silent 掉线期间是我们主动关的后端 → 保持客户端悬挂（黑洞效果）
            if self.dev.state == ONLINE:
                self.close()

    async def close_backend(self):
        """silent 掉线用：只掐后端半边，客户端连接保留但不再有任何响应。"""
        if self.bwriter is not None:
            try:
                self.bwriter.close()
            except OSError:
                pass
            self.breader = self.bwriter = None

    def close(self):
        if self.b2c_task is not None:
            self.b2c_task.cancel()
            self.b2c_task = None
        for w in (self.bwriter, self.cwriter):
            if w is not None:
                try:
                    w.close()
                except OSError:
                    pass

    async def run(self):
        """客户端 → 后端方向（c2b），并在状态切换时维护连接生命周期。"""
        dev = self.dev
        dev.conns.add(self)
        try:
            if dev.state == OFFLINE and dev.mode == "refuse":
                return  # 掐断刚撞上掉线的新连接
            if dev.state == ONLINE:
                if not await self.connect_backend():
                    return  # mbserver 不可达（如未启动）：放弃，让上位机重试
                self.start_b2c()
            while True:
                data = await self.creader.read(4096)
                if not data:
                    return
                if dev.state == ONLINE and self.bwriter is not None:
                    self.bwriter.write(data)
                    await self.bwriter.drain()
                elif dev.state == OFFLINE and dev.mode == "silent":
                    continue  # 黑洞：吞掉请求，不给任何响应
                else:
                    return  # refuse 掉线：关闭
        except (ConnectionError, OSError):
            pass
        finally:
            self.close()
            dev.conns.discard(self)


class Device(object):
    def __init__(self, idx, name, unit, typ, public_port, internal_port):
        self.idx = idx
        self.name = name
        self.unit = unit
        self.typ = typ
        self.public_port = public_port      # 上位机连的端口
        self.internal_port = internal_port  # 转发到 mbserver 的端口
        self.state = ONLINE
        self.mode = "refuse"                # refuse=断电式 / silent=黑洞式
        self.reason = "-"                   # 掉线原因
        self.offline_since = None
        self.server = None                  # asyncio.Server（refuse 掉线时关闭）
        self.conns = set()                  # 活动连接 Pair 集合
        self.recover_task = None            # 定时自动恢复任务
        self.auto_tasks = []                # schedule / random 后台任务
        self.auto_desc = ""                 # 自动规则描述（status 展示）

    @property
    def label(self):
        return "%s(:%d)" % (self.name, self.public_port)


class Breaker(object):
    def __init__(self, args, console=True):
        self.public_base = args.public_base
        self.internal_base = args.internal_base
        self.internal_host = args.internal_host
        self.rules_path = args.rules
        self.console = console  # False = GUI 模式，不启动控制台线程
        self.devices = []
        for i, entry in enumerate(STATION_PLAN):
            name, unit, _pts, _init, _loop, _final, typ = entry
            self.devices.append(Device(i + 1, name, unit, typ,
                                       self.public_base + i, self.internal_base + i))
        self.stop_event = asyncio.Event()
        self.loop = None

    def by_token(self, token):
        """按 名称 / 对外端口 / 序号 找设备，找不到返回 None。"""
        t = token.strip().upper()
        for d in self.devices:
            if d.name.upper() == t:
                return [d]
        if t.isdigit():
            n = int(t)
            for d in self.devices:
                if d.public_port == n:
                    return [d]
            if 1 <= n <= len(self.devices):
                return [self.devices[n - 1]]
        return None

    # ---------------- 状态切换 ----------------

    async def start_listener(self, dev):
        try:
            sock = _make_listen_sock(dev.public_port)
        except OSError as e:
            log("错误: %s 绑定对外端口 %d 失败: %s（端口被占用？可能有残留的 mbserver/设备脚本进程，"
                "设备脚本是 mbserver 的子进程，会继承监听 fd，杀 mbserver 时要一并清理）"
                % (dev.label, dev.public_port, e))
            raise SystemExit(1)
        dev.server = await asyncio.start_server(
            lambda r, w: Pair(dev, r, w, self).run(), sock=sock)

    def _arm_recover(self, dev, seconds):
        if dev.recover_task is not None:
            dev.recover_task.cancel()
        dev.recover_task = asyncio.create_task(self._recover_later(dev, seconds))

    async def _recover_later(self, dev, seconds):
        try:
            await asyncio.sleep(seconds)
            dev.recover_task = None
            if dev.state == OFFLINE:
                await self.go_online(dev, reason="到时自动恢复")
        except asyncio.CancelledError:
            pass

    @staticmethod
    def _mode_text(mode):
        return "断电式" if mode == "refuse" else "黑洞式"

    async def _apply_offline_effect(self, dev):
        """按 dev.mode 施加掉线副作用。

        refuse：关掉监听（新连接直接被拒）+ 掐断所有已有连接，最接近设备断电。
        silent：保持监听（后续新连接也要能被接住并进入黑洞）+ 只掐后端半边，
                客户端连接悬挂、请求被吞，模拟"网线拔了但设备没死"。

        切换模式时必须重新施加一遍副作用，不能只改 dev.mode —— 两种模式对监听和
        已有连接的处理正好相反（关监听 vs 保持监听），只改字段会让实际行为和界面
        显示的模式对不上。
        """
        if dev.mode == "refuse":
            if dev.server is not None:
                dev.server.close()  # 不再 accept → 上位机新连接直接被拒
                dev.server = None
            for pair in list(dev.conns):
                pair.close()
        else:
            if dev.server is None:
                # 从 refuse 切过来时监听已被关掉，需重开，否则新连接进不来、黑洞失效
                await self.start_listener(dev)
            for pair in list(dev.conns):
                await pair.close_backend()  # silent：掐后端半边，客户端悬挂

    async def go_offline(self, dev, reason, mode=None, duration=None):
        if mode not in (None, "refuse", "silent"):
            mode = None
        if dev.state == OFFLINE:
            # 已掉线时仍允许切换掉线模式；否则"先断成 refuse、再想改成 silent"会被
            # 静默忽略（mode 参数丢失），界面上表现为模式切不过去
            if mode is not None and mode != dev.mode:
                dev.mode = mode
                await self._apply_offline_effect(dev)
                msg = "%s 掉线模式切换 ← %s [%s]" % (dev.label, reason, self._mode_text(mode))
                if duration is not None:
                    self._arm_recover(dev, duration)
                    msg += "，%gs 后自动恢复" % duration
                log(msg)
                return True
            if duration is not None:
                self._arm_recover(dev, duration)
                log("%s 已掉线(%s)，自动恢复时间重设为 %ds" % (dev.label, dev.reason, duration))
            else:
                log("%s 已掉线(%s)，忽略" % (dev.label, dev.reason))
            return False
        if mode:
            dev.mode = mode
        dev.state = OFFLINE
        dev.reason = reason
        dev.offline_since = time.time()
        await self._apply_offline_effect(dev)
        msg = "%s 掉线 ← %s [%s]" % (dev.label, reason, self._mode_text(dev.mode))
        if duration is not None:
            self._arm_recover(dev, duration)
            msg += "，%gs 后自动恢复" % duration
        log(msg)
        return True

    async def go_online(self, dev, reason="手动"):
        if dev.state == ONLINE:
            log("%s 已在线，忽略" % dev.label)
            return False
        held = time.time() - (dev.offline_since or time.time())
        dev.state = ONLINE
        dev.reason = "-"
        dev.offline_since = None
        if dev.recover_task is not None and dev.recover_task is not asyncio.current_task():
            dev.recover_task.cancel()
        dev.recover_task = None
        for pair in list(dev.conns):
            pair.close()  # 关掉悬挂的黑洞连接，让上位机重新建立
        if dev.server is None:
            await self.start_listener(dev)
        log("%s 恢复在线 ← %s（掉线 %.0f 秒）" % (dev.label, reason, held))
        return True

    # ---------------- 自动触发规则 ----------------

    @staticmethod
    def _num(cfg, key, default):
        try:
            v = float(cfg.get(key))
            return v if v > 0 else default
        except (TypeError, ValueError):
            return default

    def apply_rules(self):
        """读取 breaker_rules.json：defaults.mode + 每设备 mode/schedule/random。"""
        if not self.rules_path or not os.path.exists(self.rules_path):
            log("无规则文件(%s)，仅手动控制（make_stub.py 可生成模板）" % self.rules_path)
            return
        try:
            with open(self.rules_path, encoding="utf-8") as f:
                data = json.load(f)
        except (OSError, ValueError) as e:
            log("规则文件 %s 解析失败: %s（本次仅手动控制）" % (self.rules_path, e))
            return
        if not isinstance(data, dict):
            log("规则文件格式应为 JSON 对象，已忽略")
            return
        default_mode = data.get("defaults", {}).get("mode")
        for dev in self.devices:
            if default_mode in ("refuse", "silent"):
                dev.mode = default_mode
        for name, cfg in (data.get("devices") or {}).items():
            if not isinstance(cfg, dict):
                continue
            ds = self.by_token(name)
            if ds is None:
                log("规则文件中的未知设备: %s（忽略）" % name)
                continue
            dev = ds[0]
            mode = cfg.get("mode", default_mode)
            if mode in ("refuse", "silent"):
                dev.mode = mode
            sch = cfg.get("schedule")
            if isinstance(sch, dict):
                every_min = self._num(sch, "every_min", 0)
                offline_sec = self._num(sch, "offline_sec", 0)
                if every_min and offline_sec:
                    dev.auto_tasks.append(asyncio.create_task(
                        self._schedule_loop(dev, every_min, offline_sec)))
                    dev.auto_desc = "定时:每%g分掉%gs" % (every_min, offline_sec)
            rnd = cfg.get("random")
            if isinstance(rnd, dict):
                prob = self._num(rnd, "prob_per_min", 0)
                dur_min = self._num(rnd, "dur_min", 0)
                dur_max = self._num(rnd, "dur_max", 0)
                if prob:
                    dev.auto_tasks.append(asyncio.create_task(
                        self._random_loop(dev, prob, dur_min or 8, dur_max or 30)))
                    dev.auto_desc = "随机:%g%%/分,%g~%gs" % (prob, dur_min, dur_max)
        n_auto = sum(1 for d in self.devices if d.auto_tasks)
        log("规则已加载: %s（%d 台设备带自动触发）" % (self.rules_path, n_auto))

    async def _schedule_loop(self, dev, every_min, offline_sec):
        while True:
            await asyncio.sleep(every_min * 60)
            if dev.state == ONLINE:
                await self.go_offline(dev, "定时窗口", duration=offline_sec)

    async def _random_loop(self, dev, prob_per_min, dur_min, dur_max):
        """与设备脚本"随机偶发故障"同风格：每分钟掷一次骰子。prob_per_min 为百分数。"""
        while True:
            await asyncio.sleep(60)
            if dev.state == ONLINE and random.random() * 100 < prob_per_min:
                await self.go_offline(dev, "随机故障",
                                      duration=random.uniform(dur_min, dur_max))

    # ---------------- 控制台 ----------------

    HELP = """命令：
  status                     查看所有设备断路器状态 (简写 s / ls)
  off <设备> [模式] [秒]      断开设备；模式 silent=黑洞式 / refuse=断电式(默认)；
                             带秒数则到时自动恢复，如: off IM-02 30 / off 2 silent 60
  on <设备>                  恢复设备
  off all / on all           全部断开 / 全部恢复
  quit                       退出 (q / exit)
设备可用 名称(IM-02)、对外端口(503)、序号(2) 指定；all 表示全部。"""

    def print_status(self):
        rows = []
        for d in self.devices:
            if d.state == OFFLINE:
                note = "掉线:%s 已%.0fs" % (d.reason, time.time() - (d.offline_since or time.time()))
            else:
                note = d.auto_desc or "-"
            rows.append("%-4d %-8s %-4d %-6d %-6s %-7s %-4d %s"
                        % (d.idx, d.name, d.unit, d.public_port, d.state, d.mode,
                           len(d.conns), note))
        with _print_lock:
            print("-" * 76)
            print("%-4s %-8s %-4s %-6s %-6s %-7s %-4s %s"
                  % ("序号", "设备", "站号", "外端口", "状态", "模式", "连接", "说明/规则"))
            for r in rows:
                print(r)
            print("-" * 76)

    def console_thread(self):
        while True:
            try:
                line = input()
            except EOFError:
                return  # 后台/管道运行时无控制台，保持代理继续跑
            line = line.strip()
            if not line:
                continue
            low = line.lower()
            if low in ("q", "quit", "exit"):
                self.loop.call_soon_threadsafe(self.stop_event.set)
                return
            if low in ("h", "help", "?"):
                with _print_lock:
                    print(self.HELP)
                continue
            if low in ("s", "status", "ls"):
                self.print_status()
                continue
            parts = line.split()
            cmd = parts[0].lower()
            if cmd in ("off", "on") and len(parts) >= 2:
                targets = self.devices if parts[1].lower() == "all" else self.by_token(parts[1])
                if targets is None:
                    with _print_lock:
                        print("找不到设备: %s（用 status 查看清单）" % parts[1])
                    continue
                mode, duration = None, None
                for tok in parts[2:]:
                    if tok.lower() in ("silent", "refuse"):
                        mode = tok.lower()
                    elif tok.replace(".", "", 1).isdigit():
                        duration = float(tok)
                    else:
                        with _print_lock:
                            print("忽略无法识别的参数: %s" % tok)
                for d in targets:
                    if cmd == "off":
                        coro = self.go_offline(d, "手动", mode=mode, duration=duration)
                    else:
                        coro = self.go_online(d)
                    asyncio.run_coroutine_threadsafe(coro, self.loop)
            else:
                with _print_lock:
                    print("未知命令: %s（输入 help）" % line)

    # ---------------- 主循环 ----------------

    async def amain(self):
        self.loop = asyncio.get_running_loop()
        for dev in self.devices:
            await self.start_listener(dev)
        self.apply_rules()
        if self.console:
            asyncio.create_task(asyncio.to_thread(self.console_thread))
        log("断路器就绪：%d 台设备 | 上位机连 %d~%d → mbserver %s:%d~%d%s"
            % (len(self.devices), self.public_base, self.public_base + len(self.devices) - 1,
               self.internal_host, self.internal_base, self.internal_base + len(self.devices) - 1,
               "" if self.console else " | 图形界面模式"))
        try:
            await self.stop_event.wait()
        finally:
            await self.acleanup()

    async def acleanup(self):
        for dev in self.devices:
            for t in [dev.recover_task] + dev.auto_tasks:
                if t is not None:
                    t.cancel()
            if dev.server is not None:
                dev.server.close()
            for pair in list(dev.conns):
                pair.close()
        log("断路器已停止")


def main():
    ap = argparse.ArgumentParser(
        description="注塑工厂 Modbus 数据桩断路器代理（单设备掉线模拟）")
    ap.add_argument("--public-base", type=int, default=PUBLIC_BASE,
                    help="对外监听端口基准（上位机连这里），默认 %d" % PUBLIC_BASE)
    ap.add_argument("--internal-base", type=int, default=15502,
                    help="mbserver 内部端口基准，默认 15502（= 对外端口 + 15000）（需与 make_stub.py --internal-base 一致）")
    ap.add_argument("--internal-host", default="127.0.0.1",
                    help="mbserver 地址，默认 127.0.0.1")
    ap.add_argument("--rules", default="breaker_rules.json",
                    help="掉线规则 JSON 文件（不存在则仅手动控制）")
    args = ap.parse_args()

    breaker = Breaker(args)
    try:
        asyncio.run(breaker.amain())
    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    main()
