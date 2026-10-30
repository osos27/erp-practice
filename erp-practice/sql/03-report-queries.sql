/* ============================================================================
   03-report-queries.sql —— 10 个报表查询
   ----------------------------------------------------------------------------
   这是整个练习里真正要动脑的部分。每个查询都标了【考点】，
   面试问"你这个项目写过什么复杂查询"时，就照着这些讲。

   执行：sqlcmd -S localhost -E -f 65001 -i 03-report-queries.sql
   每个查询之间用 GO 隔开，所以变量作用域互不影响，可以单独选中执行。
   ============================================================================ */

USE ERP_Practice;
GO


/* ============================================================================
   Q1  物料分页查询（关键字 + 分类筛选）
   【考点】OFFSET/FETCH 分页；LIKE '%关键字%' 会让索引直接失效
   ============================================================================ */
DECLARE @Keyword    NVARCHAR(50) = N'轴';
DECLARE @Category   NVARCHAR(30) = NULL;       -- 传 NULL 表示不按分类过滤
DECLARE @PageIndex  INT = 1;
DECLARE @PageSize   INT = 10;

SELECT  MaterialId, MaterialCode, MaterialName, Spec, Unit, Category, SafetyStock
FROM    dbo.Material
WHERE   IsActive = 1
  AND  (@Category IS NULL OR Category = @Category)
  AND  (@Keyword  IS NULL OR MaterialName LIKE N'%' + @Keyword + N'%'
                          OR MaterialCode LIKE N'%' + @Keyword + N'%')
ORDER BY MaterialId
OFFSET (@PageIndex - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;

/* ★ 这里有个重要的坑，也是面试常问：
   `(@Category IS NULL OR Category = @Category)` 这种"万能条件"写法，
   在参数为 NULL 时可以让优化器走索引，但参数有值时可能生成次优执行计划
   （这叫参数嗅探 Parameter Sniffing）。

   更稳的写法之一是动态 SQL（仅在确实需要时拼接条件），
   或者给查询加 OPTION (RECOMPILE)。先知道有这回事，别一上来就用。

   另外 LIKE N'%轴%' 前面带 % 是必然全表扫描的 ——
   前缀不确定就无法用 B 树索引定位。真实项目里要么换成全文索引，
   要么把搜索框改成"以...开头"。这个回答能让面试官知道你懂索引原理。 */
GO


/* ============================================================================
   Q2  库存余额 + 安全库存预警
   【考点】视图的用法；CASE 表达式；NULL 与比较
   ============================================================================ */
SELECT  WarehouseCode, WarehouseName, MaterialCode, MaterialName, Unit,
        InQty, OutQty, BalanceQty, SafetyStock,
        CASE WHEN BalanceQty <= 0                    THEN N'缺货'
             WHEN BalanceQty <  SafetyStock          THEN N'低于安全库存'
             ELSE                                         N'正常'
        END AS StockStatus
FROM    dbo.vw_StockBalance
ORDER BY CASE WHEN BalanceQty < SafetyStock THEN 0 ELSE 1 END,   -- 预警的排前面
         WarehouseCode, BalanceQty;
GO


/* ============================================================================
   Q3  近 30 天各类型单据汇总
   【考点】GROUP BY + 聚合；COUNT(DISTINCT ...) 去重计数
   ============================================================================ */
SELECT  b.BillType,
        CASE b.BillType WHEN N'PI' THEN N'采购入库'
                        WHEN N'SO' THEN N'销售出库'
                        WHEN N'RT' THEN N'采购退货'
                        ELSE            N'销售退货' END        AS BillTypeName,
        COUNT(DISTINCT b.BillId)                              AS BillCount,
        COUNT(*)                                              AS LineCount,
        SUM(d.Quantity)                                       AS TotalQty,
        SUM(d.Amount)                                         AS TotalAmount
FROM    dbo.BillHead   b
JOIN    dbo.BillDetail d ON d.BillId = b.BillId
WHERE   b.Status    = 2
  AND   b.BillDate >= DATEADD(DAY, -30, CAST(SYSDATETIME() AS DATE))
GROUP BY b.BillType
ORDER BY b.BillType;

/* ★ COUNT(*) 和 COUNT(DISTINCT b.BillId) 的区别：
   一张单有 3 行明细，JOIN 之后就是 3 条记录。
   COUNT(*) 数的是"明细行数"，COUNT(DISTINCT BillId) 数的是"单据数"。
   混淆这两者，报表里的单数就会虚高 —— 这是做报表最容易犯的错。 */
GO


/* ============================================================================
   Q4  本月每个物料的出入库与净变动
   【考点】CASE WHEN 做条件聚合（行转列的雏形）
   ============================================================================ */
DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(SYSDATETIME()), MONTH(SYSDATETIME()), 1);
DECLARE @MonthEnd   DATE = DATEADD(DAY, -1, DATEADD(MONTH, 1, @MonthStart));

SELECT  m.MaterialCode, m.MaterialName, m.Unit, m.Category,
        SUM(CASE WHEN b.BillType IN (N'PI', N'ST') THEN d.Quantity ELSE 0 END) AS InQty,
        SUM(CASE WHEN b.BillType IN (N'SO', N'RT') THEN d.Quantity ELSE 0 END) AS OutQty,
        SUM(CASE WHEN b.BillType IN (N'PI', N'ST') THEN d.Quantity
                 ELSE -d.Quantity END)                                          AS NetQty,
        SUM(d.Amount)                                                           AS TotalAmount
FROM    dbo.BillDetail d
JOIN    dbo.BillHead   b ON b.BillId     = d.BillId
JOIN    dbo.Material   m ON m.MaterialId = d.MaterialId
WHERE   b.Status = 2
  AND   b.BillDate BETWEEN @MonthStart AND @MonthEnd
GROUP BY m.MaterialCode, m.MaterialName, m.Unit, m.Category
HAVING  SUM(d.Quantity) > 0
ORDER BY NetQty DESC;
GO


/* ============================================================================
   Q5  呆滞物料：90 天没有任何出入库
   【考点】LEFT JOIN 找"不存在"；HAVING 里对聚合结果再用日期函数；NULL 的坑
   ============================================================================ */
DECLARE @StaleDays INT = 90;

SELECT  m.MaterialCode, m.MaterialName, m.Category,
        MAX(b.BillDate)                                              AS LastMovementDate,
        DATEDIFF(DAY, MAX(b.BillDate), CAST(SYSDATETIME() AS DATE))  AS DaysSinceLastMovement
FROM    dbo.Material   m
LEFT JOIN dbo.BillDetail d ON d.MaterialId = m.MaterialId
LEFT JOIN dbo.BillHead   b ON b.BillId = d.BillId AND b.Status = 2
GROUP BY m.MaterialId, m.MaterialCode, m.MaterialName, m.Category
HAVING  MAX(b.BillDate) IS NULL                                   -- 从来没动过
   OR   MAX(b.BillDate) < DATEADD(DAY, -@StaleDays, CAST(SYSDATETIME() AS DATE))
ORDER BY DaysSinceLastMovement DESC;

/* ★ 为什么 HAVING 要写 `MAX(b.BillDate) IS NULL`？
   如果只写 `MAX(b.BillDate) < 某日期`，那么"从来没有出入库过的物料"
   （LEFT JOIN 之后 BillDate 全是 NULL）会被比较结果 UNKNOWN 过滤掉，
   而它恰恰是最呆滞的那批。这是 NULL 三值逻辑最经典的坑。 */
GO


/* ============================================================================
   Q6  客户销售额 Top 5
   【考点】ROW_NUMBER() 窗口函数；RANK / DENSE_RANK 的区别
   ============================================================================ */
WITH SalesByCustomer AS
(
    SELECT  c.CustomerId,
            c.CustomerName,
            SUM(d.Amount)                AS TotalAmount,
            COUNT(DISTINCT b.BillId)     AS BillCount
    FROM    dbo.BillHead   b
    JOIN    dbo.BillDetail d ON d.BillId     = b.BillId
    JOIN    dbo.Customer   c ON c.CustomerId = b.CustomerId
    WHERE   b.BillType = N'SO'
      AND   b.Status   = 2
    GROUP BY c.CustomerId, c.CustomerName
)
SELECT  ROW_NUMBER() OVER (ORDER BY TotalAmount DESC) AS Rn,
        CustomerName, BillCount, TotalAmount
FROM    SalesByCustomer
ORDER BY TotalAmount DESC;

/* ★ ROW_NUMBER / RANK / DENSE_RANK 的区别（面试高频）：
     金额 100 100 90，三种函数的序号分别是：
       ROW_NUMBER : 1, 2, 3   （永远不重复、不跳号）
       RANK       : 1, 1, 3   （并列占位，会跳号）
       DENSE_RANK : 1, 1, 2   （并列不跳号）
   取 Top N 时到底该用哪个，取决于业务要不要"并列也算进去"。
   如果 Top 5 里有两个并列第 3，用 RANK 会返回 6 条，用 DENSE_RANK 返回 5 条。 */
GO


/* ============================================================================
   Q7  ★ 库存结存流水（逐笔累计余额）—— 本文件最难的查询
   【考点】SUM() OVER (PARTITION BY ... ORDER BY ... ROWS UNBOUNDED PRECEDING)
   ============================================================================ */
DECLARE @MaterialId INT = 5;

SELECT  b.BillDate,
        b.BillNo,
        b.BillType,
        CASE WHEN b.BillType IN (N'PI', N'ST') THEN d.Quantity ELSE 0 END AS InQty,
        CASE WHEN b.BillType IN (N'SO', N'RT') THEN d.Quantity ELSE 0 END AS OutQty,
        SUM(CASE WHEN b.BillType IN (N'PI', N'ST') THEN  d.Quantity
                 ELSE -d.Quantity END)
            OVER (PARTITION BY d.MaterialId
                  ORDER BY b.BillDate, b.BillId
                  ROWS UNBOUNDED PRECEDING)                               AS RunningBalance
FROM    dbo.BillDetail d
JOIN    dbo.BillHead   b ON b.BillId = d.BillId
WHERE   d.MaterialId = @MaterialId
  AND   b.Status     = 2
ORDER BY b.BillDate, b.BillId;

/* ★ 这是 ERP 里最经典的报表："每一笔出入库之后，库存还剩多少"。
   老写法是自连接：每条记录去关联所有日期 <= 自己的记录再 SUM，
   N 条记录就是 N² 次运算。窗口函数是一次排序扫描就出来了。
   面试被问"你会哪些窗口函数"，答这一条比背 ROW_NUMBER 有用得多。

   ★ 注意 ORDER BY 里必须带上 BillId 作为第二排序键。
   同一天有多笔单据时，如果只按 BillDate 排序，
   累计值的顺序是不确定的 —— 这叫"不确定排序"，是报表数据对不上的经典原因。 */
GO


/* ============================================================================
   Q8  月度采购金额与环比
   【考点】LAG() 取上一行；NULLIF 防止除零
   ============================================================================ */
WITH MonthlyPurchase AS
(
    SELECT  CONVERT(CHAR(7), b.BillDate, 120) AS MonthKey,   -- 'yyyy-mm'
            SUM(d.Amount)                      AS Amount
    FROM    dbo.BillHead   b
    JOIN    dbo.BillDetail d ON d.BillId = b.BillId
    WHERE   b.BillType = N'PI'
      AND   b.Status   = 2
    GROUP BY CONVERT(CHAR(7), b.BillDate, 120)
),
WithPrev AS
(
    SELECT  MonthKey, Amount,
            LAG(Amount) OVER (ORDER BY MonthKey) AS PrevAmount
    FROM    MonthlyPurchase
)
SELECT  MonthKey,
        Amount,
        PrevAmount,
        CAST((Amount - PrevAmount) * 100.0 / NULLIF(PrevAmount, 0) AS DECIMAL(10,2)) AS GrowthPercent
FROM    WithPrev
ORDER BY MonthKey;

/* ★ 为什么用 NULLIF(PrevAmount, 0) 而不是直接除？
   第一个月没有上个月，PrevAmount 是 NULL，除以 NULL 结果是 NULL（还好）。
   但如果某个月金额恰好是 0，直接除会抛"除数为零"的运行时错误。
   NULLIF(x, 0) 把 0 变成 NULL，整个表达式就安全地返回 NULL。

   ★ 为什么不用 FORMAT(b.BillDate, 'yyyy-MM')？
   FORMAT 比 CONVERT(..., 120) 慢一个数量级（它内部走 .NET 格式化），
   在几百万行的报表里这是实打实的性能差距。 */
GO


/* ============================================================================
   Q9  行转列（PIVOT）：每个仓库的四种单据金额各占一列
   【考点】PIVOT 的基本写法；为什么动态列必须用动态 SQL
   ============================================================================ */
WITH SourceData AS
(
    SELECT  w.WarehouseCode,
            w.WarehouseName,
            b.BillType,
            d.Amount
    FROM    dbo.BillHead   b
    JOIN    dbo.BillDetail d ON d.BillId     = b.BillId
    JOIN    dbo.Warehouse  w ON w.WarehouseId = b.WarehouseId
    WHERE   b.Status = 2
),
Pivoted AS
(
    SELECT  *
    FROM    SourceData
    PIVOT (SUM(Amount) FOR BillType IN ([PI], [SO], [RT], [ST])) AS p
)
SELECT  WarehouseCode,
        WarehouseName,
        ISNULL([PI], 0) AS PurchaseIn,
        ISNULL([SO], 0) AS SalesOut,
        ISNULL([RT], 0) AS PurchaseReturn,
        ISNULL([ST], 0) AS SalesReturn,
        ISNULL([PI], 0) + ISNULL([SO], 0) + ISNULL([RT], 0) + ISNULL([ST], 0) AS TotalAmount
FROM    Pivoted
ORDER BY WarehouseCode;

/* ★ PIVOT 的硬伤：列名（[PI] / [SO] ...）必须写死。
   如果列是"月份"这种动态来源，就必须拼动态 SQL（sp_executesql）才能实现。
   面试问"你怎么做动态行列转换"时，答这一点就够了。 */
GO


/* ============================================================================
   Q10  索引失效对照实验（同一张表，三种写法）
   【考点】能看懂 STATISTICS IO 和实际执行计划
   ============================================================================ */
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

/* 10-1 走索引：直接用列的值去等值查询 */
SELECT MaterialId, MaterialCode, MaterialName
FROM   dbo.Material
WHERE  MaterialCode = N'M0010';

/* 10-2 索引失效：在索引列上套了函数（LEFT 包住了 MaterialCode） */
SELECT MaterialId, MaterialCode, MaterialName
FROM   dbo.Material
WHERE  LEFT(MaterialCode, 5) = N'M0010';

/* 10-3 索引失效：隐式类型转换（列是 NVARCHAR，传的是 VARCHAR 字面量，
   在排序规则/类型不一致时会导致转换发生在列上）。
   记住结论：查询参数的 SQL 类型必须和列的类型一致。 */
SELECT MaterialId, MaterialCode, MaterialName
FROM   dbo.Material
WHERE  MaterialCode = 'M0010';

SET STATISTICS TIME OFF;
SET STATISTICS IO   OFF;
GO
