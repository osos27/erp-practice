# C# / SQL Server 练习仓库

个人学习与练手记录，方向：**C# / .NET Framework + SQL Server 的企业应用与 ERP 二次开发**。

仓库里是几个独立的小项目，从数据库设计、数据访问层封装，到 WinForms 上位机与自定义控件，逐个补工程能力。

---

## 内容一览

| 目录 | 内容 | 涉及技术 |
|---|---|---|
| **[`erp-practice/`](erp-practice/)** | **主项目**：ERP 风格数据库 + ADO.NET 数据访问层 | SQL Server、T-SQL、窗口函数、存储过程、ADO.NET |
| [`Controls/`](Controls/) | 自定义 WinForms 控件库 | UserControl、自定义绘制 |
| [`tempWork/`](tempWork/) | WinForms 上位机练习 | 串口通信、手写 Modbus RTU 协议栈 |

---

## 主项目：erp-practice

一套可运行的 ERP 练习环境：6 张业务表 + 库存视图 + 1000 张模拟单据，配套 10 个报表查询、2 个存储过程和一个 `SqlHelper` 数据访问层，外加一个控制台演示程序把每样都跑一遍。

**数据库部分**

- 物料 / 仓库 / 供应商 / 客户 / 单据主表 / 单据明细 六张表，含外键、唯一约束与业务 CHECK 约束
- 库存**不建表**，由单据汇总成视图 —— 避免"库存字段与单据对不上"这类经典问题
- 单据金额使用 `PERSISTED` 计算列，杜绝数量、单价、金额三方打架

**报表部分**

| 查询 | 技术点 |
|---|---|
| 物料分页 | `OFFSET / FETCH` |
| 库存预警 | 视图 + `CASE` 表达式 |
| 单据汇总 | 条件聚合、`COUNT(DISTINCT)` |
| 呆滞物料 | `LEFT JOIN` + `HAVING` + NULL 三值逻辑 |
| 客户销售额 Top N | `ROW_NUMBER()` 窗口函数 |
| **逐笔库存结存** | `SUM() OVER (... ROWS UNBOUNDED PRECEDING)` |
| 月度采购环比 | `LAG()` + `NULLIF` 防除零 |
| 仓库 × 单据类型 | `PIVOT` 行列转换 |
| 索引失效对照 | `SET STATISTICS IO` 实验 |

**数据访问层**

`SqlHelper` 覆盖参数化查询、事务封装、`DataReader` 流式映射、异步方法与存储过程调用；参数统一用显式 `SqlDbType` 构造，规避 `AddWithValue` 的类型推断问题。

---

## 快速开始

### 1. 建库造数

需要 SQL Server（Express / Developer / LocalDB 均可）。

```powershell
cd erp-practice
.\run-sql.cmd
```

脚本会依次执行 `sql/` 下的建表、造数、报表、存储过程四个文件，约 6 秒完成。

### 2. 配置连接字符串

打开 `erp-practice/csharp/App.config`，按需修改 `Erp` 连接字符串（默认 `Server=localhost` + Windows 集成认证）。

### 3. 运行演示

用 Visual Studio 打开 `erp-practice/csharp/ErpPractice.csproj`，F5。控制台会给出 9 个菜单项，逐项对应一个查询或一种 `SqlHelper` 用法。

> 详细说明、七天学习计划与踩坑记录见 [`erp-practice/README.md`](erp-practice/README.md)。

---

## 目录结构

```
.
├── erp-practice/            ERP 练习库（主项目）
│   ├── sql/                 建表、造数、报表查询、存储过程
│   ├── csharp/              控制台演示 + SqlHelper 数据访问层
│   ├── run-sql.cmd          一键建库造数（Windows）
│   ├── run-sql.ps1
│   └── README.md            详细文档
├── Controls/                自定义 WinForms 控件（温湿度监控面板）
├── tempWork/                WinForms 上位机练习 + 手写 Modbus RTU 协议栈
└── Controls.slnx            解决方案文件
```

---

## 环境要求

- Windows + Visual Studio 2022 或更高版本
- .NET Framework 4.8
- SQL Server 2016 及以上（本项目在 SQL Server 2022 上验证）
- SQL Server Management Studio（可选，用于手工查询）

---

## 关于本仓库

这几个项目是**练习性质**的，不是生产代码。仓库的提交记录即为学习进度。
