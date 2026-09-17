#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
注塑工厂 Modbus 数据桩 —— 断路器图形界面
========================================
breaker.py（控制台版）的图形前端：复用同一个 Breaker 代理核心（asyncio 跑在
后台线程），Tk 主线程显示设备状态、掉线/恢复按钮和事件日志。两者并存：

  python breaker.py       # 控制台交互版
  python breaker_gui.py   # 图形界面版（参数与 breaker.py 相同）

界面只覆盖"手动断开/恢复"；定时窗口与随机触发仍由 breaker_rules.json 配置，
加载后显示在各设备"说明/规则"列。
"""

import argparse
import queue
import threading
import time
import tkinter as tk
from tkinter import ttk

import breaker
from breaker import Breaker, OFFLINE, ONLINE
from make_stub import PUBLIC_BASE

MODE_TEXT = {"refuse": "refuse(断电式)", "silent": "silent(黑洞式)"}
MODE_FROM_TEXT = {v: k for k, v in MODE_TEXT.items()}


def device_note(dev, now=None):
    """状态列右侧的说明文案：掉线显示原因与已掉秒数，在线显示自动规则。"""
    if now is None:
        now = time.time()
    if dev.state == OFFLINE:
        held = 0 if dev.offline_since is None else now - dev.offline_since
        return "掉线:%s 已%ds" % (dev.reason, int(held))
    return dev.auto_desc or "-"


def device_rows(devices, now=None):
    """表格行快照：(序号, 设备, 外端口, 状态, 模式, 连接数, 说明)。"""
    if now is None:
        now = time.time()
    return [(d.idx, d.name, d.public_port, d.state, d.mode, len(d.conns),
             device_note(d, now)) for d in devices]


def parse_duration(text):
    """自动恢复秒数输入 → 时长；空/0/非法/负数 = 不自动恢复（None）。"""
    try:
        v = float(text)
    except (TypeError, ValueError):
        return None
    return v if v > 0 else None


class BreakerGUI(object):
    def __init__(self, root, args):
        self.root = root
        self.breaker = Breaker(args, console=False)
        self.log_queue = queue.Queue()
        self.remove_sink = breaker.add_log_sink(self.log_queue.put)
        self.dead = False  # 代理线程启动失败（如端口被占）
        self._ready_shown = False
        self._prog_selection = ()  # _poll 程序化恢复的选中行，用于区分用户改选
        self._build()
        self._start_breaker_thread()
        self._poll()

    # ---------------- 界面 ----------------

    def _build(self):
        self.root.title("Modbus 数据桩断路器（单设备掉线模拟）")
        self.root.geometry("660x560")
        self.root.minsize(600, 480)

        # 设备表格
        cols = ("idx", "name", "port", "state", "mode", "conns", "note")
        heads = ("序号", "设备", "外端口", "状态", "模式", "连接", "说明/规则")
        widths = (42, 68, 56, 48, 118, 44, 218)
        frame = ttk.Frame(self.root)
        frame.pack(fill="both", expand=True, padx=8, pady=(8, 2))
        self.tree = ttk.Treeview(frame, columns=cols, show="headings", selectmode="browse")
        for c, h, w in zip(cols, heads, widths):
            self.tree.column(c, width=w, minwidth=36, anchor="w" if c == "note" else "center")
            self.tree.heading(c, text=h)
        ys = ttk.Scrollbar(frame, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscrollcommand=ys.set)
        self.tree.pack(side="left", fill="both", expand=True)
        ys.pack(side="right", fill="y")
        self.tree.tag_configure("on", foreground="#1a7f37")
        self.tree.tag_configure("off", foreground="#c62828")
        self.tree.bind("<<TreeviewSelect>>", self._on_select)

        # 操作区
        ops = ttk.LabelFrame(self.root, text="对选中设备")
        ops.pack(fill="x", padx=8, pady=2)
        ttk.Label(ops, text="掉线模式").grid(row=0, column=0, padx=(8, 2), pady=6)
        self.mode_box = ttk.Combobox(ops, values=list(MODE_TEXT.values()),
                                     width=16, state="readonly")
        self.mode_box.current(0)
        self.mode_box.grid(row=0, column=1, padx=2)
        ttk.Label(ops, text="自动恢复秒(0=不自动)").grid(row=0, column=2, padx=(14, 2))
        self.seconds = tk.StringVar(value="0")
        ttk.Spinbox(ops, from_=0, to=86400, textvariable=self.seconds,
                    width=7).grid(row=0, column=3, padx=2)
        self.btn_off = ttk.Button(ops, text="断开", command=self._off_selected)
        self.btn_off.grid(row=0, column=4, padx=(14, 2))
        self.btn_on = ttk.Button(ops, text="恢复", command=self._on_selected)
        self.btn_on.grid(row=0, column=5, padx=2)
        self.btn_on_all = ttk.Button(ops, text="全部恢复", command=self._on_all)
        self.btn_on_all.grid(row=0, column=6, padx=(14, 8))
        for w in (self.btn_off, self.btn_on, self.btn_on_all):
            w.state(["disabled"])

        # 日志区
        logf = ttk.LabelFrame(self.root, text="事件日志")
        logf.pack(fill="both", expand=True, padx=8, pady=2)
        self.log_text = tk.Text(logf, height=8, state="disabled", font=("Consolas", 9))
        ls = ttk.Scrollbar(logf, orient="vertical", command=self.log_text.yview)
        self.log_text.configure(yscrollcommand=ls.set)
        self.log_text.pack(side="left", fill="both", expand=True, padx=(4, 0), pady=4)
        ls.pack(side="right", fill="y", padx=(0, 4), pady=4)

        # 状态栏
        self.status = ttk.Label(self.root, anchor="w", relief="sunken", padding=(6, 3))
        self.status.pack(fill="x", side="bottom")
        self.status["text"] = ("代理启动中… 上位机连 %d~%d → mbserver %s:%d~%d"
                               % (self.breaker.public_base,
                                  self.breaker.public_base + len(self.breaker.devices) - 1,
                                  self.breaker.internal_host,
                                  self.breaker.internal_base,
                                  self.breaker.internal_base + len(self.breaker.devices) - 1))

    # ---------------- 代理线程 ----------------

    def _start_breaker_thread(self):
        def run():
            try:
                breaker_instance = self.breaker
                import asyncio
                asyncio.run(breaker_instance.amain())
            except SystemExit as e:
                self.dead = True
                self.log_queue.put("[错误] 代理启动失败（端口被占用？）: %s" % e)
            except Exception as e:  # 线程内异常要浮出到界面
                self.dead = True
                self.log_queue.put("[错误] 代理线程异常退出: %r" % e)
        threading.Thread(target=run, daemon=True).start()

    # ---------------- 状态刷新 ----------------

    def _poll(self):
        now = time.time()
        # 表格
        selected = self.tree.selection()
        for row, dev in zip(device_rows(self.breaker.devices, now), self.breaker.devices):
            iid = dev.name
            vals = tuple(row)
            if self.tree.exists(iid):
                self.tree.item(iid, values=vals)
            else:
                self.tree.insert("", "end", iid=iid, values=vals)
            self.tree.item(iid, tags=("on" if dev.state == ONLINE else "off",))
        if selected and self.tree.exists(selected[0]):
            # 恢复选中前记下这一行：Tk 对程序化的 selection_set 也会发
            # <<TreeviewSelect>>，且事件是延迟投递的（用瞬时标志位挡不住），
            # 所以在 _on_select 里按选中值比对来区分是否用户改选
            self._prog_selection = tuple(selected)
            self.tree.selection_set(selected)
        # 按钮
        ready = (not self.dead and self.breaker.loop is not None)
        has_sel = bool(self.tree.selection())
        for btn, cond in ((self.btn_off, ready and has_sel),
                          (self.btn_on, ready and has_sel),
                          (self.btn_on_all, ready)):
            btn.state(["!disabled"] if cond else ["disabled"])
        # 日志
        try:
            while True:
                self._append_log(self.log_queue.get_nowait())
        except queue.Empty:
            pass
        if self.dead:
            self.status["text"] = "代理已停止（见日志）。请排查后重启本程序。"
        elif not self._ready_shown and self.breaker.loop is not None:
            self._ready_shown = True
            self.status["text"] = ("代理运行中 | 上位机连 %d~%d → mbserver %s:%d~%d"
                                   % (self.breaker.public_base,
                                      self.breaker.public_base + len(self.breaker.devices) - 1,
                                      self.breaker.internal_host,
                                      self.breaker.internal_base,
                                      self.breaker.internal_base + len(self.breaker.devices) - 1))
        self.root.after(500, self._poll)

    def _append_log(self, line):
        self.log_text.configure(state="normal")
        self.log_text.insert("end", line + "\n")
        if float(self.log_text.index("end-1c").split(".")[0]) > 800:
            self.log_text.delete("1.0", "200.0")  # 防止无限增长
        self.log_text.see("end")
        self.log_text.configure(state="disabled")

    # ---------------- 操作 ----------------

    def _selected_dev(self):
        sel = self.tree.selection()
        if not sel:
            return None
        for d in self.breaker.devices:
            if d.name == sel[0]:
                return d
        return None

    def _on_select(self, event):
        """仅在用户真正改选设备时同步下拉框。

        _poll 每 500ms 会恢复一次选中，Tk 为此同样会发 <<TreeviewSelect>> 且事件
        延迟投递；若不加区分，用户刚选好的掉线模式会在 500ms 内被按回 dev.mode，
        表现为"模式切不过去"。这里比对选中值：等于程序化恢复的那一行就跳过。
        """
        if tuple(self.tree.selection()) == self._prog_selection:
            return
        self._sync_mode_box()

    def _sync_mode_box(self):
        dev = self._selected_dev()
        if dev is not None and dev.mode in MODE_TEXT:
            self.mode_box.set(MODE_TEXT[dev.mode])

    def _submit(self, coro_factory):
        if self.dead or self.breaker.loop is None:
            return
        import asyncio
        asyncio.run_coroutine_threadsafe(coro_factory(), self.breaker.loop)

    def _off_selected(self):
        dev = self._selected_dev()
        if dev is None:
            return
        mode = MODE_FROM_TEXT.get(self.mode_box.get())
        duration = parse_duration(self.seconds.get())
        self._submit(lambda: self.breaker.go_offline(dev, "手动", mode=mode, duration=duration))

    def _on_selected(self):
        dev = self._selected_dev()
        if dev is None:
            return
        self._submit(lambda: self.breaker.go_online(dev))

    def _on_all(self):
        for dev in list(self.breaker.devices):
            self._submit(lambda d=dev: self.breaker.go_online(d))

    def on_close(self):
        self.remove_sink()
        if self.breaker.loop is not None and not self.dead:
            self.breaker.loop.call_soon_threadsafe(self.breaker.stop_event.set)
        self.root.after(300, self.root.destroy)

    # ---------------- 冒烟自测（--selftest）----------------

    def selftest(self):
        """真实 Tk 进程内自动执行：选中→断开→2s 自动恢复→关闭，打印 SELFTEST 结果。"""
        dev = self.breaker.devices[4]  # IM-02（STATION_PLAN 第 5 个）

        def step_off():
            self.tree.selection_set(dev.name)
            self.mode_box.set(MODE_TEXT["refuse"])
            self.seconds.set("0")
            self._off_selected()
        def step_rearm():
            self.results.append(("断开→掉线(手动)", dev.state == OFFLINE and dev.reason == "手动"))
            self.seconds.set("2")
            self._off_selected()  # 已掉线 → 重设 2s 自动恢复
        def step_verify():
            self.results.append(("2s 后自动恢复→在线", dev.state == ONLINE))
            ok = all(v for _, v in self.results)
            print("SELFTEST:%s %s" % ("PASS" if ok else "FAIL",
                                      "; ".join("%s=%s" % (k, "PASS" if v else "FAIL")
                                                for k, v in self.results)), flush=True)
            self.root.after(300, self.on_close)

        self.results = []
        self.root.after(800, step_off)
        self.root.after(2000, step_rearm)
        self.root.after(5000, step_verify)


def main():
    ap = argparse.ArgumentParser(
        description="注塑工厂 Modbus 数据桩断路器（图形界面，单设备掉线模拟）")
    ap.add_argument("--public-base", type=int, default=PUBLIC_BASE,
                    help="对外监听端口基准（上位机连这里），默认 %d" % PUBLIC_BASE)
    ap.add_argument("--internal-base", type=int, default=15502,
                    help="mbserver 内部端口基准，默认 15502（= 对外端口 + 15000）")
    ap.add_argument("--internal-host", default="127.0.0.1",
                    help="mbserver 地址，默认 127.0.0.1")
    ap.add_argument("--rules", default="breaker_rules.json",
                    help="掉线规则 JSON 文件（不存在则仅手动控制）")
    ap.add_argument("--selftest", action="store_true",
                    help="冒烟自测：自动断开/恢复一台设备后退出")
    args = ap.parse_args()

    root = tk.Tk()
    app = BreakerGUI(root, args)
    root.protocol("WM_DELETE_WINDOW", app.on_close)
    if args.selftest:
        app.selftest()
    root.mainloop()


if __name__ == "__main__":
    main()
