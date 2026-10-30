using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ErpPractice.Data;
using ErpPractice.Models;

namespace ErpPractice
{
    /// <summary>
    /// 演示程序：把 SqlHelper 的每一种用法都跑一遍。
    /// 先执行 sql/01-schema.sql 和 sql/02-seed.sql，再运行本程序。
    /// </summary>
    internal static class Program
    {
        private static SqlHelper _db;

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var connectionString = ConfigurationManager.ConnectionStrings["Erp"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("App.config 里没有找到名为 Erp 的连接字符串。");
                Pause();
                return;
            }

            _db = new SqlHelper(connectionString);

            try
            {
                var server = _db.ExecuteScalar<string>("SELECT @@SERVERNAME");
                var database = _db.ExecuteScalar<string>("SELECT DB_NAME()");
                var billCount = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.BillHead");
                Console.WriteLine($"已连接：{server} / {database}，单据 {billCount} 张");
            }
            catch (Exception ex)
            {
                Console.WriteLine("数据库连接失败：" + ex.Message);
                Console.WriteLine();
                Console.WriteLine("请检查：");
                Console.WriteLine("  1. sql/01-schema.sql 和 sql/02-seed.sql 是否已执行");
                Console.WriteLine("  2. App.config 里的连接字符串是否指向正确的实例");
                Pause();
                return;
            }

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("──────────────────────────────────────────────");
                Console.WriteLine("  1. 物料分页查询（参数化 + OFFSET/FETCH）");
                Console.WriteLine("  2. 库存预警（ExecuteList 映射成对象）");
                Console.WriteLine("  3. 近 30 天单据汇总（GROUP BY + 条件聚合）");
                Console.WriteLine("  4. 呆滞物料（LEFT JOIN + HAVING + NULL 陷阱）");
                Console.WriteLine("  5. 客户销售额 Top 5（ROW_NUMBER 窗口函数）");
                Console.WriteLine("  6. 调用存储过程（分页 + OUTPUT 参数）");
                Console.WriteLine("  7. 事务：新增一张采购入库单（提交）");
                Console.WriteLine("  8. 事务：故意失败并回滚（证明原子性）");
                Console.WriteLine("  9. 库存流水（SUM() OVER 累计余额）");
                Console.WriteLine("  0. 退出");
                Console.WriteLine("──────────────────────────────────────────────");
                Console.Write("请选择：");

                var input = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (input)
                    {
                        case "1": Query1_MaterialPaging(); break;
                        case "2": Query2_StockWarning();   break;
                        case "3": Query3_BillSummary();    break;
                        case "4": Query4_StaleMaterials(); break;
                        case "5": Query5_TopCustomers();   break;
                        case "6": Query6_StoredProcedure(); break;
                        case "7": Query7_TransactionCommit(); break;
                        case "8": Query8_TransactionRollback(); break;
                        case "9": Query9_StockLedger();    break;
                        case "0": return;
                        default:  Console.WriteLine("没有这个选项。"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("执行出错：" + ex.Message);
                }
            }
        }


        /* ====================================================================
           1. 物料分页 —— 演示参数化查询 + 强类型映射
           ==================================================================== */
        private static void Query1_MaterialPaging()
        {
            const string sql = @"
SELECT  MaterialId, MaterialCode, MaterialName, Spec, Unit, Category, SafetyStock
FROM    dbo.Material
WHERE   IsActive = 1
  AND  (@Category IS NULL OR Category = @Category)
  AND  (@Keyword  IS NULL OR MaterialName LIKE N'%' + @Keyword + N'%'
                          OR MaterialCode LIKE N'%' + @Keyword + N'%')
ORDER BY MaterialId
OFFSET (@PageIndex - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;";

            var list = _db.ExecuteList(sql, r => new MaterialRow
            {
                MaterialId   = r.GetInt32(r.GetOrdinal("MaterialId")),
                MaterialCode = r.GetString(r.GetOrdinal("MaterialCode")),
                MaterialName = r.GetString(r.GetOrdinal("MaterialName")),
                Spec         = r.IsDBNull(r.GetOrdinal("Spec")) ? null : r.GetString(r.GetOrdinal("Spec")),
                Unit         = r.GetString(r.GetOrdinal("Unit")),
                Category     = r.GetString(r.GetOrdinal("Category")),
                SafetyStock  = r.GetDecimal(r.GetOrdinal("SafetyStock"))
            },
            P.NVarChar("@Keyword",   "轴", 50),
            P.NVarChar("@Category",  null, 30),
            P.Int("@PageIndex",      1),
            P.Int("@PageSize",       10));

            Console.WriteLine($"命中 {list.Count} 行：");
            foreach (var m in list)
                Console.WriteLine($"  {m.MaterialCode}  {m.MaterialName}  {m.Spec}  安全库存 {m.SafetyStock}{m.Unit}");
        }


        /* ====================================================================
           2. 库存预警 —— 演示 ExecuteList + 对象映射，顺便体现"不要传 DataTable"
           ==================================================================== */
        private static void Query2_StockWarning()
        {
            const string sql = @"
SELECT  WarehouseCode, WarehouseName, MaterialId, MaterialCode, MaterialName, Unit,
        InQty, OutQty, BalanceQty, SafetyStock
FROM    dbo.vw_StockBalance
WHERE   BalanceQty < SafetyStock
ORDER BY WarehouseCode, BalanceQty;";

            var rows = _db.ExecuteList(sql, r => new StockRow
            {
                WarehouseCode = r.GetString(r.GetOrdinal("WarehouseCode")),
                WarehouseName = r.GetString(r.GetOrdinal("WarehouseName")),
                MaterialId    = r.GetInt32(r.GetOrdinal("MaterialId")),
                MaterialCode  = r.GetString(r.GetOrdinal("MaterialCode")),
                MaterialName  = r.GetString(r.GetOrdinal("MaterialName")),
                Unit          = r.GetString(r.GetOrdinal("Unit")),
                InQty         = r.GetDecimal(r.GetOrdinal("InQty")),
                OutQty        = r.GetDecimal(r.GetOrdinal("OutQty")),
                BalanceQty    = r.GetDecimal(r.GetOrdinal("BalanceQty")),
                SafetyStock   = r.GetDecimal(r.GetOrdinal("SafetyStock"))
            });

            Console.WriteLine($"共 {rows.Count} 条低于安全库存：");
            foreach (var row in rows)
                Console.WriteLine("  " + row);
        }


        /* ====================================================================
           3. 近 30 天单据汇总 —— 演示 ExecuteDataTable + 打印表格
           ==================================================================== */
        private static void Query3_BillSummary()
        {
            const string sql = @"
SELECT  b.BillType,
        CASE b.BillType WHEN N'PI' THEN N'采购入库'
                        WHEN N'SO' THEN N'销售出库'
                        WHEN N'RT' THEN N'采购退货'
                        ELSE             N'销售退货' END AS BillTypeName,
        COUNT(DISTINCT b.BillId) AS BillCount,
        COUNT(*)                 AS LineCount,
        SUM(d.Quantity)          AS TotalQty,
        SUM(d.Amount)            AS TotalAmount
FROM    dbo.BillHead   b
JOIN    dbo.BillDetail d ON d.BillId = b.BillId
WHERE   b.Status    = 2
  AND   b.BillDate >= DATEADD(DAY, -30, CAST(SYSDATETIME() AS DATE))
GROUP BY b.BillType
ORDER BY b.BillType;";

            PrintTable(_db.ExecuteDataTable(sql));
        }


        /* ====================================================================
           4. 呆滞物料
           ==================================================================== */
        private static void Query4_StaleMaterials()
        {
            const string sql = @"
SELECT  m.MaterialCode, m.MaterialName, m.Category,
        MAX(b.BillDate) AS LastMovementDate,
        DATEDIFF(DAY, MAX(b.BillDate), CAST(SYSDATETIME() AS DATE)) AS DaysSinceLastMovement
FROM    dbo.Material   m
LEFT JOIN dbo.BillDetail d ON d.MaterialId = m.MaterialId
LEFT JOIN dbo.BillHead   b ON b.BillId = d.BillId AND b.Status = 2
GROUP BY m.MaterialId, m.MaterialCode, m.MaterialName, m.Category
HAVING  MAX(b.BillDate) IS NULL
   OR   MAX(b.BillDate) < DATEADD(DAY, -90, CAST(SYSDATETIME() AS DATE))
ORDER BY DaysSinceLastMovement DESC;";

            PrintTable(_db.ExecuteDataTable(sql));
        }


        /* ====================================================================
           5. 客户销售额 Top 5 —— 演示 ExecuteList + 强制类型转换
           ==================================================================== */
        private static void Query5_TopCustomers()
        {
            const string sql = @"
WITH SalesByCustomer AS
(
    SELECT  c.CustomerName,
            SUM(d.Amount)            AS TotalAmount,
            COUNT(DISTINCT b.BillId) AS BillCount
    FROM    dbo.BillHead   b
    JOIN    dbo.BillDetail d ON d.BillId     = b.BillId
    JOIN    dbo.Customer   c ON c.CustomerId = b.CustomerId
    WHERE   b.BillType = N'SO' AND b.Status = 2
    GROUP BY c.CustomerId, c.CustomerName
)
SELECT  ROW_NUMBER() OVER (ORDER BY TotalAmount DESC) AS Rn,
        CustomerName, BillCount, TotalAmount
FROM    SalesByCustomer
ORDER BY TotalAmount DESC;";

            var rows = _db.ExecuteList(sql, r => new
            {
                Rn          = r.GetInt64(0),
                Customer    = r.GetString(1),
                BillCount   = r.GetInt32(2),
                TotalAmount = r.GetDecimal(3)
            }).Take(5);

            foreach (var row in rows)
                Console.WriteLine($"  第 {row.Rn} 名  {row.Customer}  单数 {row.BillCount}  金额 {row.TotalAmount:N2}");
        }


        /* ====================================================================
           6. 存储过程 + OUTPUT 参数 —— 实际工作里最常见的一种
           ==================================================================== */
        private static void Query6_StoredProcedure()
        {
            var totalCount = P.Output("@TotalCount", SqlDbType.Int);

            var dt = _db.ExecuteStoredProcedure(
                "dbo.usp_GetBillList",
                P.NVarChar("@BillType", "PI", 10),
                P.Int("@PageIndex", 1),
                P.Int("@PageSize", 5),
                totalCount);

            Console.WriteLine($"采购入库单总数（由 OUTPUT 参数返回）：{totalCount.Value}");
            Console.WriteLine("当前页：");
            PrintTable(dt);

            /* ★ OUTPUT 参数的值类型是 object 而且是 DBNull 的可能来源，
               所以不要写 (int)totalCount.Value，用 Convert.ToInt32 更安全。 */
        }


        /* ====================================================================
           7. 事务提交 —— 单头 + 明细必须同时成功
           ==================================================================== */
        private static void Query7_TransactionCommit()
        {
            var billNo = "PI-DEMO-" + DateTime.Now.ToString("HHmmssff");
            var before = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.BillHead");

            var billId = _db.ExecuteInTransaction(tran =>
            {
                /* ★ 关键点：INSERT 和"取刚插入的 ID"必须在同一条命令里。

                   常见错误写法是分两步：
                       INSERT INTO dbo.BillHead ...           -- 第一条命令
                       SELECT SCOPE_IDENTITY()                -- 第二条命令 → 返回 NULL！

                   因为 SCOPE_IDENTITY() 的作用域是"当前批次/存储过程"。
                   换一条 SqlCommand 就是换了一个作用域，它拿不到上一条命令的 ID，
                   返回 NULL，CAST 成 int 就变成 0 —— 接着插明细就会撞外键。

                   正确做法是让 INSERT 直接 OUTPUT 出 ID，
                   一次往返同时完成插入和取值，而且不受触发器影响。 */
                var id = _db.ExecuteScalar<int>(tran, @"
INSERT INTO dbo.BillHead (BillNo, BillType, BillDate, WarehouseId, SupplierId, Status, Remark)
OUTPUT INSERTED.BillId
VALUES (@BillNo, N'PI', @BillDate, 1, 1, 2, N'由演示程序创建');",
                    P.NVarChar("@BillNo", billNo, 30),
                    P.Date("@BillDate", DateTime.Today));

                _db.ExecuteNonQuery(tran, @"
INSERT INTO dbo.BillDetail (BillId, MaterialId, Quantity, UnitPrice)
VALUES (@BillId, @MaterialId, @Quantity, @UnitPrice);",
                    P.Int("@BillId", id),
                    P.Int("@MaterialId", 1),
                    P.Dec("@Quantity", 10m),
                    P.Dec("@UnitPrice", 2.5m));

                _db.ExecuteNonQuery(tran, @"
INSERT INTO dbo.BillDetail (BillId, MaterialId, Quantity, UnitPrice)
VALUES (@BillId, @MaterialId, @Quantity, @UnitPrice);",
                    P.Int("@BillId", id),
                    P.Int("@MaterialId", 2),
                    P.Dec("@Quantity", 20m),
                    P.Dec("@UnitPrice", 1.2m));

                return id;
            });

            var after = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.BillHead");

            Console.WriteLine($"事务已提交：新单据 BillId={billId}，单号 {billNo}");
            Console.WriteLine($"单头数量 {before} → {after}");
            Console.WriteLine("（注意：明细金额 Amount 是计算列，插入时不需要也不能写它）");
        }


        /* ====================================================================
           8. 事务回滚 —— 故意让第二行明细违反 CHECK 约束
           ==================================================================== */
        private static void Query8_TransactionRollback()
        {
            var billNo = "PI-BAD-" + DateTime.Now.ToString("HHmmssff");
            var before = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.BillHead");
            Console.WriteLine($"事务开始前，单据数 = {before}");

            try
            {
                _db.ExecuteInTransaction(tran =>
                {
                    var id = _db.ExecuteScalar<int>(tran, @"
INSERT INTO dbo.BillHead (BillNo, BillType, BillDate, WarehouseId, SupplierId, Status)
OUTPUT INSERTED.BillId
VALUES (@BillNo, N'PI', @BillDate, 1, 1, 2);",
                        P.NVarChar("@BillNo", billNo, 30),
                        P.Date("@BillDate", DateTime.Today));

                    _db.ExecuteNonQuery(tran, @"
INSERT INTO dbo.BillDetail (BillId, MaterialId, Quantity, UnitPrice)
VALUES (@BillId, @MaterialId, @Quantity, @UnitPrice);",
                        P.Int("@BillId", id),
                        P.Int("@MaterialId", 3),
                        P.Dec("@Quantity", 5m),
                        P.Dec("@UnitPrice", 9.9m));

                    // ↓ 数量为负数，违反 CK_BillDetail_Qty (Quantity > 0)
                    _db.ExecuteNonQuery(tran, @"
INSERT INTO dbo.BillDetail (BillId, MaterialId, Quantity, UnitPrice)
VALUES (@BillId, @MaterialId, @Quantity, @UnitPrice);",
                        P.Int("@BillId", id),
                        P.Int("@MaterialId", 4),
                        P.Dec("@Quantity", -1m),
                        P.Dec("@UnitPrice", 3m));

                    return id;
                });

                Console.WriteLine("不应该走到这里 —— 约束没生效？");
            }
            catch (SqlException ex)
            {
                Console.WriteLine("捕获到异常（预期内）：" + ex.Message);
            }

            var after = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.BillHead");
            Console.WriteLine($"事务回滚后，单据数 = {after}（应等于 {before}）");
            Console.WriteLine($"验证单号 {billNo} 是否残留：" +
                (_db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.BillHead WHERE BillNo = @n",
                    P.NVarChar("@n", billNo, 30)) == 0 ? "没有残留 ✔" : "残留了 ✘"));
        }


        /* ====================================================================
           9. 库存流水：SUM() OVER 累计余额
           ==================================================================== */
        private static void Query9_StockLedger()
        {
            var materialId = _db.ExecuteScalar<int>(
                "SELECT TOP 1 MaterialId FROM dbo.BillDetail GROUP BY MaterialId ORDER BY COUNT(*) DESC;");

            var material = _db.ExecuteSingle(
                "SELECT MaterialCode + N' ' + MaterialName FROM dbo.Material WHERE MaterialId = @id",
                r => r.GetString(0),
                P.Int("@id", materialId));

            Console.WriteLine($"物料：{material}（MaterialId={materialId}）");
            Console.WriteLine("最后一列 RunningBalance 是逐笔累计结存：");
            PrintTable(_db.ExecuteStoredProcedure("dbo.usp_GetStockLedger",
                P.Int("@MaterialId", materialId)));
        }


        /* ====================================================================
           工具方法
           ==================================================================== */

        private static void PrintTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                Console.WriteLine("(无数据)");
                return;
            }

            var headers = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            var rows = new List<string[]>();
            foreach (DataRow row in dt.Rows)
            {
                rows.Add(headers.Select((h,i) =>
                {
                    var v = row[i];
                    return v == null || v == DBNull.Value ? "" : Convert.ToString(v);
                }).ToArray());
            }

            var widths = new int[headers.Length];
            for (var i = 0; i < headers.Length; i++)
            {
                widths[i] = DisplayWidth(headers[i]);
                foreach (var r in rows)
                    widths[i] = Math.Max(widths[i], DisplayWidth(r[i]));
            }

            Console.WriteLine(string.Join(" │ ", headers.Select((h, i) => Pad(h, widths[i]))));
            Console.WriteLine(string.Join("─┼─", widths.Select(w => new string('─', w))));

            foreach (var r in rows)
                Console.WriteLine(string.Join(" │ ", r.Select((c, i) => Pad(c, widths[i]))));

            Console.WriteLine($"({rows.Count} 行)");
        }

        /// <summary>中文算两个显示宽度，否则对齐会歪。</summary>
        private static int DisplayWidth(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            var w = 0;
            foreach (var ch in s)
                w += ch > 0x7F ? 2 : 1;
            return w;
        }

        private static string Pad(string s, int width)
        {
            s = s ?? "";
            var pad = width - DisplayWidth(s);
            return pad > 0 ? s + new string(' ', pad) : s;
        }

        private static void Pause()
        {
            Console.WriteLine("按任意键退出…");
            Console.ReadKey(true);
            Environment.Exit(0);
        }
    }
}
