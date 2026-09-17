# -*- coding: utf-8 -*-
"""临时真机验证客户端：原始 socket Modbus TCP（FC03/FC04/FC06）。用法：
python test_mb_client.py <port> <unit> <cmd>
  cmd: r4 <addr> [qty] | r3 <addr> [qty] | w4 <addr> <value> | wait3 <addr> <expect_lo> <expect_hi> <timeout_s>
"""
import socket
import struct
import sys
import time


def frame(tid, unit, pdu):
    return struct.pack(">HHHB", tid, 0, len(pdu) + 1, unit) + pdu


def send(sock, unit, pdu):
    sock.sendall(frame(1, unit, pdu))
    hdr = sock.recv(7)
    while len(hdr) < 7:
        hdr += sock.recv(7 - len(hdr))
    tid, proto, ln = struct.unpack(">HHH", hdr[:6])
    body = b""
    while len(body) < ln - 1:
        chunk = sock.recv(ln - 1 - len(body))
        if not chunk:
            break
        body += chunk
    return body


def main():
    port, unit = int(sys.argv[1]), int(sys.argv[2])
    cmd = sys.argv[3]
    s = socket.create_connection(("127.0.0.1", port), timeout=5)
    if cmd == "r4":
        addr, qty = int(sys.argv[4]), int(sys.argv[5]) if len(sys.argv) > 5 else 1
        body = send(s, unit, struct.pack(">BHH", 3, addr - 40001, qty))
        vals = struct.unpack(">%dH" % qty, body[2:2 + 2 * qty])
        print(vals)
    elif cmd == "r3":
        addr, qty = int(sys.argv[4]), int(sys.argv[5]) if len(sys.argv) > 5 else 1
        body = send(s, unit, struct.pack(">BHH", 4, addr - 30001, qty))
        vals = struct.unpack(">%dH" % qty, body[2:2 + 2 * qty])
        print(vals)
    elif cmd == "w4":
        addr, val = int(sys.argv[4]), int(sys.argv[5])
        body = send(s, unit, struct.pack(">BHH", 6, addr - 40001, val))
        print("ok" if body[0] == 6 else body)
    elif cmd == "r1":
        addr, qty = int(sys.argv[4]), int(sys.argv[5]) if len(sys.argv) > 5 else 1
        body = send(s, unit, struct.pack(">BHH", 2, addr - 10001, qty))
        print([(body[2 + i // 8] >> (i % 8)) & 1 for i in range(qty)])
    elif cmd == "w0":
        addr, val = int(sys.argv[4]), int(sys.argv[5])
        body = send(s, unit, struct.pack(">BHH", 5, addr - 1, 0xFF00 if val else 0x0000))
        print("ok" if body[0] == 5 else body)
    elif cmd == "wait3":
        addr, lo, hi, to = int(sys.argv[4]), int(sys.argv[5]), int(sys.argv[6]), float(sys.argv[7])
        t0 = time.time()
        last = None
        while time.time() - t0 < to:
            body = send(s, unit, struct.pack(">BHH", 4, addr - 30001, 1))
            v = struct.unpack(">h", body[2:4])[0]
            last = v
            if lo <= v <= hi:
                print("REACHED %d in %.1fs" % (v, time.time() - t0))
                return
            time.sleep(1.0)
        print("TIMEOUT last=%d want [%d,%d]" % (last, lo, hi))
        sys.exit(1)
    s.close()


if __name__ == "__main__":
    main()
