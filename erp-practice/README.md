# ERP 练习库（SQL Server + C# ADO.NET）

这不是一份教程，是一套**已经在 SQL Server 2022 上实际跑通的练习环境**：

- 6 张 ERP 风格的表 + 1 个库存视图 + 6 个显式索引 + 6 个唯一约束
- 1000 张单据 / 约 2400 行明细的模拟数据（随机、可重复生成）
- 10 个真实业务报表查询（从分页到窗口函数到 PIVOT）
- 2 个存储过程（分页 + OUTPUT 参数）
- 一个 459 行的 `SqlHelper`（参数化、事务、对象映射、异步）
- 一个能直接 F5 的控制台演示程序，把上面每一样都跑一遍

针对的岗位：**C# / .NET Framework + SQL Server 的企业应用与 ERP 二次开发**。

---

## 1. 先看难度，再决定投多少时间

这是你最关心的部分，所以放最前面。**别被 2000 行代码吓到 —— 里面一多半是注释和数据，真正要你动脑的大约 300 行。**

| 文件 | 行数 | 难度 | 你要做什么 | 预计耗时 |
|---|---:|---|---|---|
| `sql/01-schema.sql` | 238 | ★★☆☆☆ | 不用改，但要**读懂**为什么库存不建表、Amount 为什么是计算列 | 1.5 h |
| `sql/02-seed.sql` | 233 | ★★☆☆☆ | 不用改。看懂循环造数和那几手随机数技巧即可 | 1 h |
| `sql/03-report-queries.sql` | 305 | ★★★★☆ | **真正的难点在这**。10 个查询，其中 Q5/Q7/Q9 需要想 | 5–6 h |
| `sql/04-stored-procedures.sql` | 146 | ★★★☆☆ | 读懂分页 + OUTPUT 参数，然后自己写一个 | 2 h |
| `csharp/Data/SqlHelper.cs` | 459 | ★★☆☆☆ | 结构很固定，读一遍；重点是能**默写** `ExecuteNonQuery` 和事务版本 | 3 h |
| `csharp/Data/DbParams.cs` | 148 | ★☆☆☆☆ | 纯工具类，10 分钟看完 | 0.2 h |
| `csharp/Program.cs` | 467 | ★★☆☆☆ | 调用演示，看懂 9 个菜单怎么用 SqlHelper 即可 | 1.5 h |
| `csharp/Models/StockRow.cs` | 67 | ★☆☆☆☆ | 映射示例 | 0.2 h |

**总账：跟着抄一遍 1 天，自己独立默写出来 5–7 天。**

一句话判断：**如果你能在不看答案的情况下写出 Q7（库存结存流水）和 `SqlHelper.ExecuteInTransaction`，这部分就过关了。**

---

## 2. 三步跑起来

### 第 1 步：建库造数

在 `erp-practice` 目录下执行（推荐第一种，不用管执行策略）：

```powershell
.\run-sql.cmd
```

也可以直接调 PowerShell：

```powershell
powershell -ExecutionPolicy Bypass -File .\run-sql.ps1
```

> ⚠️ **最容易错的一步**：上面这行开头的 `powershell` 不能漏。
> 漏了就会报 `无法将"-ExecutionPolicy"项识别为 cmdlet`——
> 因为 `-ExecutionPolicy` 是 `powershell.exe` 的参数，不是 PowerShell 里的命令。
>
> 如果你已经在 PowerShell 提示符里，直接 `.\run-sql.ps1` 也能跑，
> 前提是执行策略允许；被拦的话先执行
> `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`（只影响当前窗口）。

脚本会自动执行 `sql/` 下 4 个文件，约 6 秒，最后打印数据量。
如果用的是命名实例：`.\run-sql.cmd -Server ".\SQLEXPRESS"`。

> 也可以用 SSMS 按 01 → 04 的顺序逐个打开执行。
> 注意：SQL 文件是 **UTF-8 with BOM**，用 sqlcmd 执行时记得 `-f 65001`，
> 否则中文会变成乱码。

### 第 2 步：改连接串

打开 `csharp/ErpPractice.csproj`，改 `App.config` 里的 `Erp` 连接字符串
（默认是 `Server=localhost;Database=ERP_Practice;` + Windows 集成认证）。

### 第 3 步：F5

Visual Studio 打开 csproj，直接运行。你会看到一个 9 项的菜单，每项对应一个查询或一个 `SqlHelper` 用法。

---

## 3. 数据库结构

```
Material（物料）──┐
Warehouse（仓库）─┼── BillHead（单据主表）── BillDetail（明细）
Supplier（供应商）┘                              │
Customer（客户）──┘                              ↓
                                    vw_StockBalance（库存视图）
```

四个设计决定，每个都能当面试谈资：

1. **库存不建表。** 库存 = 出入库的汇总结果。建表存库存，一旦有人绕过程序改数据，账面和实际就永远对不上。
2. **金额是计算列。** `Amount = Quantity × UnitPrice`，物理存储在表里（`PERSISTED`），但永远不能单独写入，杜绝"数量 10、单价 5、金额 60"。
3. **业务规则写进 CHECK 约束。** "采购单必须有供应商、销售单必须有客户"这条规则在数据库层，谁也绕不过去。
4. **明细去重靠唯一约束。** 同一张单同一物料只能一行，用 `UNIQUE (BillId, MaterialId)` 保证。

---

## 4. 七天计划

每天 2–3 小时。**顺序很重要，别跳。**

| Day | 任务 | 验收标准 |
|---|---|---|
| **1** | 跑通环境 + 读 `01-schema.sql` | 能回答：库存为什么不建表？`PERSISTED` 计算列有什么用？哪里用了覆盖索引？ |
| **2** | 手写 Q1–Q3（不看答案） | 分页能写对；能解释 `COUNT(*)` 和 `COUNT(DISTINCT)` 的差别 |
| **3** | 手写 Q4–Q5 | 能说出 Q5 里 `HAVING ... IS NULL` 为什么不能省 |
| **4** | **Q6 + Q7（最难的一天）** | 能讲清 `ROW_NUMBER/RANK/DENSE_RANK` 区别；能把 Q7 用自连接写一遍再换窗口函数，说出性能差异 |
| **5** | Q8 Q9 + 两个存储过程 | 能写一个带 OUTPUT 参数的分页过程 |
| **6** | 通读 `SqlHelper` | **合上书默写** `ExecuteNonQuery`、`ExecuteList`、`ExecuteInTransaction` 三个方法 |
| **7** | 自己加一个报表 | 例：按「月份 × 仓库」统计出入库金额，写成存储过程 + 控制台调用 |

---

## 5. 我在实测中踩到的 4 个坑（都已写进代码注释）

这 4 个坑是**故意留的**，因为它们是真实开发里最常见的"跑起来才炸"的类型，而且非常适合当面试谈资：

| # | 现象 | 原因 | 正确做法 |
|---|---|---|---|
| 1 | 插入时莫名插了 NULL | `CHOOSE(索引, ...)` 的索引是 **BIGINT** 时静默返回 NULL | 改用 `CASE`，或把索引显式 `CAST` 成 INT |
| 2 | 明细插入撞外键 | 对空表执行 `DBCC CHECKIDENT(..., RESEED, 0)`，**第一条记录 ID 是 0 不是 1** | 永远不要假设自增从 1 开始、也不要假设连续 |
| 3 | `关键字 'RowCount' 附近有语法错误` | `ROWCOUNT` 是保留字（`SET ROWCOUNT`） | 列别名加 `[]`，或换个名字 |
| 4 | `SCOPE_IDENTITY()` 返回 0 → 撞外键 | 作用域是**当前批次**，换一条 `SqlCommand` 就拿不到 | `INSERT ... OUTPUT INSERTED.BillId` 一条命令搞定 |

第 4 个尤其值得记：它是"为什么插入主从表总是报外键错误"的头号原因。

---

## 6. 面试能直接讲的 7 个点

1. **库存为什么不建表** → 数据一致性，流水 vs 快照
2. **计算列 vs 普通字段** → 消除三方数据打架
3. **CHECK 约束承载业务规则** → 数据库层兜底，不只靠代码 if
4. **索引失效的三种情况** → 列上套函数、隐式类型转换、违反最左前缀（`03-report-queries.sql` 的 Q10 有对照实验）
5. **窗口函数 vs 自连接** → 累计余额从 O(N²) 降到一次排序扫描
6. **`AddWithValue` 的坑** → 类型推断导致索引失效和计划污染，要显式指定 `SqlDbType`
7. **连接池不是"每次 Open 很慢"** → `Open()` 是借、`Close()` 是还，所以每次查询 Open 一次是标准写法

---

## 7. 已知的不完美（都是故意的，可当延伸练习）

- **库存会出现负数**：随机数据没做"出库不能超过可用库存"的校验。真实系统必须拦。→ *练习：加一个触发器或存储过程校验。*
- **PIVOT 的列名写死**：换成"按月份"就必须动态 SQL。→ *练习：用 `sp_executesql` 写动态 PIVOT。*
- **存储过程里两次查询用了同一套 WHERE**：可读但会扫两遍。→ *练习：改成临时表 + 参数嗅探讨论。*
- **没有单元测试**：`SqlHelper` 可以用一个本地库做集成测试。→ *练习：用 xUnit/NUnit 给 10 个查询写断言。*
- **没有索引覆盖特殊场景**：可以自己 `SET STATISTICS IO ON` 观察不同写法的逻辑读差异。

---

## 8. 和简历怎么接

面试时不要只说"我做过练习"，要说具体的东西：

> 我自己搭了一个 ERP 风格的练习库：物料/仓库/供应商/客户/单据主表/明细六张表，
> 1000 张单据数据，写了库存结存、呆滞物料、客户销售额 Top N、月度环比这些报表，
> 核心是窗口函数算逐笔累计结存。数据访问层用 ADO.NET 封装了 SqlHelper，
> 包含参数化、事务和对象映射。

这段话里每个词都能被追问，而这份代码保证你都答得上来。
