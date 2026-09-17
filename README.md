# 注塑车间设备监控上位机（InjectionHost）

面向注塑车间的设备监控上位机。通过 **Modbus TCP** 实时采集 6 类共 18 台设备的运行状态与工艺参数，提供机组总览、设备详情、报警管理、历史趋势与工艺参数下发。

通信层为**手写实现，不依赖任何第三方 Modbus 库**。

> 本项目为个人学习项目，使用开源项目[ModbusTool](https://github.com/serhmarch/ModbusTools)作为数据桩，和项目内的独立断路器（使用breaker/breaker_gui.bat启动断路器）模拟断电/黑洞式连接失败场景。

**技术栈**：C# · .NET Framework 4.6 · WinForms · Modbus TCP · SQLite · MSChart · Newtonsoft.Json

## 预览
<img width="328" height="306" alt="image" src="https://github.com/user-attachments/assets/c4384525-89a9-4d37-8370-ecd2eef59203" />
<img width="1937" height="1118" alt="image" src="https://github.com/user-attachments/assets/66ea95ba-c831-4e65-9edb-2086f6821bf1" />
<img width="1937" height="1118" alt="image" src="https://github.com/user-attachments/assets/0457567b-a8fb-49c4-a6c9-d3415ac674c8" />
<img width="1937" height="1118" alt="image" src="https://github.com/user-attachments/assets/7aa62f7e-711c-423c-9b82-38db6c55ebcb" />


---

## 功能

| 模块 | 说明 |
|---|---|
| 实时监控 | 总览页 4 个注塑机组卡片，每张卡片聚合注塑机 + 模温机 + 干燥机 + 机械手，共 18 台设备，500ms 定时刷新 |
| 设备详情 | 点击机组卡片进入详情页，展示单台设备全部工艺参数与运行状态；返回时销毁控件防止内存泄漏 |
| 报警管理 | 设备报警码解析为中文描述（油温过高 / 模具报警 / 射胶异常等），按级别颜色编码（绿=正常、红=报警、橙=停机），仅在报警状态发生变化时记录日志 |
| 历史趋势 | 模具温度 1 小时滑动窗口实时曲线（MSChart），超期数据自动淘汰，500ms 刷新 |
| 参数下发 | 料筒 5 段温度批量写入（功能码 `0x10` 单次写多寄存器），带输入校验与成功/失败提示；车间温湿度设定值下发 |
| 用户登录 | PBKDF2 密码哈希（随机盐 + 10000 次迭代） |
| 运行日志 | 5 级日志（Debug/Info/Warn/Error/Fatal），文件 / 控制台 / UI 三路输出，界面内置实时日志窗，按启动时间分文件 |

---

## 通信层实现要点

- **手写 Modbus TCP 报文**：自行组包与解析 MBAP 头，覆盖 `01`（读线圈）、`02`（读离散输入）、`03`（读保持寄存器）、`04`（读输入寄存器）、`05`（写单线圈）、`06`（写单寄存器）、`0F`（写多线圈）、`10`（写多寄存器）共 8 类功能码。
- **精确读满，解决粘包/拆包**：TCP 是字节流没有消息边界，按 MBAP 头的长度字段逐段读满指定字节数
- **通道状态机 + 指数退避重连**：以 `ip:port` 为粒度维护通道状态（正常 / 超时 / 请求失败 / 连接失败），失败后按 `1s→2s→4s→8s→…→300s` 退避重试，成功后清零。
- **同通道请求串行化**：用 `SemaphoreSlim` 保证同一通道同时只有一个在途请求，避免响应串包。
- **自增事务号**：按 `ip:port` 独立自增，通过 `ConcurrentDictionary` + 细粒度锁保证 18 台设备并发采集下的线程安全。
- **地址连续性分组**：轮询前按寄存器区域与地址连续性自动分组，把逐点读取合并为批量读取 —— **125 个采集点位的单轮请求数由 422 次降至 91 次，减少 78.4%**。
- **跨寄存器 32 位数据合成**：高低字拼接组成 `int` / `uint`，配合定点小数（×10）换算，适配现场设备的数据格式。
- **可扩展的报文解析**：抽象设备基类 + 委托回调，特殊设备可自定义解析逻辑，其余走默认解析。

---

## 目录结构

```
InjectionHost/
├─ UI/                  界面层：9 个窗体与自定义控件
│   ├─ MainForm         主框架
│   ├─ LoginControl     登录页
│   ├─ Homepage         总览页（车间环境 + 4 个机组卡片 + 实时日志窗）
│   ├─ InjectMachineGroup       机组概览卡片（自定义控件）
│   ├─ InjectionGroupDetail     机组详情页
│   ├─ SetBarrelTemp            料筒温度设定
│   ├─ HistoryTemp              历史温度曲线
│   └─ StandardDoubleLabel      通用"标签 + 值"控件
├─ device/              设备层：6 类设备的属性封装与轮询逻辑
│   ├─ interfaces/      IModbusSlave 接口、ModbusSlaveTcp 抽象基类
│   └─ SlaveManager     设备实例工厂（按 ip:port:slaveId 缓存复用）
├─ modbus/              通信层：报文收发、通道状态机、事务号
├─ domain/              领域模型：点位属性、设备信息、数据历史、报警、用户
├─ repository/          数据层：SQLite 访问与泛型结果集映射
├─ service/             业务层：用户注册/登录
├─ config/              配置：XML 读取 + JSON 点位映射
├─ log/                 自研分级日志组件
├─ task/                定时任务管理（按周期聚合任务）
├─ tool/                工具：地址分组、字节序转换、流读取
├─ constants/           Modbus 功能码等常量
├─ sql/                 建表脚本
└─ config/slave/SlaveAddressMapping.json   设备与点位定义
```

---

## 运行

### 环境要求

- Windows
- .NET Framework 4.6 或更高
- Visual Studio 2019+ ，或 MSBuild（命令行构建需先还原 NuGet 包）

### 构建

Visual Studio 打开 `moju.sln` 直接生成即可（会自动还原 NuGet 包）。

### 数据桩（联调前提）

本项目自身不产生数据，运行前需要一个 **Modbus TCP 从站**提供数据。

联调时使用开源项目 [ModbusTools](https://github.com/serhmarch/ModbusTools) 的 `mbserver` 作为从站容器，加载自建数据桩 `stub.mbs`。该桩由生成脚本产出，是 18 个从站的工程配置，包含各设备的寄存器初值、随周期变化的模拟量，以及温度等参数按热惯性收敛的规则（而不是突变），用于逼近真实现场的数据特征。

启动顺序：

1. 启动 `mbserver` 并加载 `stub.mbs`
2. （可选）启动掉线模拟代理，用于验证重连逻辑（见下节）
3. 运行 `moju.exe`

### 设备与端口对应表

上位机按此表连接从站。同一类别下各实例的点位定义相同，仅 IP / 端口 / 站号不同。

| 设备 | 类别 | 端口 | 站号 |
|---|---|---|---|
| IM-01 / 02 / 03 / 04 | 注塑机 | 502 / 506 / 510 / 514 | 1 / 5 / 9 / 13 |
| MTC-01 / 02 / 03 / 04 | 模温机 | 503 / 507 / 511 / 515 | 2 / 6 / 10 / 14 |
| DRY-01 / 02 / 03 / 04 | 干燥机 | 504 / 508 / 512 / 516 | 3 / 7 / 11 / 15 |
| ROB-01 / 02 / 03 / 04 | 机械手 | 505 / 509 / 513 / 517 | 4 / 8 / 12 / 16 |
| EM-01 | 总电表 | 518 | 61 |
| ENV-01 | 车间环境 | 519 | 71 |

### 掉线模拟
使用breaker/breaker_gui.bat断路器模拟断路
工控现场网络抖动、设备断电是常态。为了验证上位机的重连与容错逻辑，另外配了一个独立的**断路器代理**（Python，仅用标准库），架在上位机与从站之间：

- 可以对**单台设备**单独制造掉线，不影响其它设备
- 两种掉线模式：
  - `refuse`（断电式）：关闭监听并掐断已有连接 → 上位机遇到连接被拒/重置，最接近设备断电
  - `silent`（黑洞式）：保留连接但把所有请求吞掉 → 上位机读超时，模拟"网线拔了但设备没死"
- 三种触发方式，可叠加：手动控制台指令、定时窗口（每 N 分钟掉线 M 秒）、随机概率
- 带 tkinter 图形前端，双击 `breaker_gui.bat` 即可启动

用它可以复现单台掉线、多台同时掉线、周期性抖动等场景，检验通道状态机、指数退避重连与界面状态刷新是否正确。

> 数据桩生成脚本与断路器代理目前不在本仓库内。

---

## 配置说明

### `config/config.xml`

| 节点 | 说明 |
|---|---|
| `Log/LogLevel` | 日志级别：`DEBUG` / `INFO` / `WARN` / `ERROR` / `FATAL` |
| `Log/outputTarget` | 输出目标，逗号分隔：`0` 文件、`1` 控制台、`2` UI、`3` 数据库 |
| `modbusProtocol/requestTimeoutMs` | 单次 Modbus 请求超时（毫秒） |
| `modbusProtocol/retryArray` | 请求失败后的退避间隔序列（毫秒） |
| `modbusProtocol/mappingJsonAddress` | 点位映射文件路径 |
| `database/fileName` | SQLite 数据库文件名 |

### `config/slave/SlaveAddressMapping.json`

定义设备类别、设备实例与采集点位：

- `mapping`：该类设备的点位定义，字段包括 `Name`（英文名）、`region`（数据区：`0` 线圈 / `1` 离散输入 / `3` 输入寄存器 / `4` 保持寄存器）、`address`（区域内地址）、`Description`（中文含义）、`dotIndex`（小数位数）、`unit`（单位）。
- `instance`：该类设备的实例列表，字段为 `ip` / `port` / `slaveId`。

**扩展设备时只需要改这个 JSON**，新增实例或点位无需修改代码。只有在需要新增一类设备（新的点位结构）时，才需要继承 `ModbusSlaveTcp` 增加一个设备类。
