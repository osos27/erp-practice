/* ============================================================================
   04-stored-procedures.sql —— 存储过程
   ----------------------------------------------------------------------------
   为什么这个岗位一定会问存储过程：
   金蝶二次开发里，报表和复杂查询基本都是存储过程 + 视图，很少在 C# 里拼长 SQL。

   两个过程演示了实际工作中 90% 的场景：
     usp_GetBillList    —— 多条件 + 分页 + 输出总条数
     usp_GetStockLedger —— 指定物料的库存流水

   执行：sqlcmd -S localhost -E -f 65001 -i 04-stored-procedures.sql
   ============================================================================ */

USE ERP_Practice;
GO

IF OBJECT_ID(N'dbo.usp_GetBillList', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetBillList;
GO

/* ============================================================================
   usp_GetBillList —— 单据分页查询
   ----------------------------------------------------------------------------
   参数全部给了默认值，调用方只传自己关心的条件。
   @TotalCount 是 OUTPUT 参数，用来告诉前端"一共多少条"，否则前端没法做分页控件。
   ============================================================================ */
CREATE PROCEDURE dbo.usp_GetBillList
    @BillType   NVARCHAR(10) = NULL,      -- 不传 = 全部类型
    @DateFrom   DATE         = NULL,      -- 不传 = 一年前
    @DateTo     DATE         = NULL,      -- 不传 = 今天
    @PageIndex  INT          = 1,
    @PageSize   INT          = 20,
    @TotalCount INT          = NULL OUTPUT
AS
BEGIN
    /* ★ SET NOCOUNT ON 几乎是存储过程的第一条铁律。
       如果不加，每次 INSERT/UPDATE 都会往客户端多推一条 "n 行受影响" 的消息，
       行数多的时候会明显拖慢速度，还会让 DataSet 里多出空表。 */
    SET NOCOUNT ON;

    /* ---- 参数清洗：防御性编程，别相信调用方 ---- */
    IF @PageIndex IS NULL OR @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize  IS NULL OR @PageSize  < 1 SET @PageSize  = 20;
    IF @PageSize  > 200 SET @PageSize = 200;          -- 防止前端传 999999 把内存打爆
    SET @DateFrom = ISNULL(@DateFrom, DATEADD(DAY, -365, CAST(SYSDATETIME() AS DATE)));
    SET @DateTo   = ISNULL(@DateTo,   CAST(SYSDATETIME() AS DATE));
    IF @DateFrom > @DateTo SET @DateTo = @DateFrom;   -- 起止反了就纠正，别抛错

    /* ---- 先查总数 ---- */
    SELECT @TotalCount = COUNT(*)
    FROM   dbo.BillHead b
    WHERE  b.Status = 2
      AND (@BillType IS NULL OR b.BillType = @BillType)
      AND  b.BillDate BETWEEN @DateFrom AND @DateTo;

    /* ---- 再查当前页 ----
       注意：这里两次查询用了同一套 WHERE 条件。
       严格来说应该把条件抽成一个临时表，避免"两次扫描"和"参数嗅探"问题；
       但在这个数据量下可读性更重要。知道有这回事就行。 */
    SELECT  b.BillId,
            b.BillNo,
            b.BillType,
            CASE b.BillType WHEN N'PI' THEN N'采购入库'
                            WHEN N'SO' THEN N'销售出库'
                            WHEN N'RT' THEN N'采购退货'
                            ELSE            N'销售退货' END      AS BillTypeName,
            b.BillDate,
            w.WarehouseName,
            ISNULL(s.SupplierName, c.CustomerName)               AS PartnerName,
            (SELECT COUNT(*)       FROM dbo.BillDetail d WHERE d.BillId = b.BillId) AS LineCount,
            (SELECT ISNULL(SUM(d.Amount), 0) FROM dbo.BillDetail d WHERE d.BillId = b.BillId) AS TotalAmount
    FROM    dbo.BillHead   b
    JOIN    dbo.Warehouse  w ON w.WarehouseId = b.WarehouseId
    LEFT JOIN dbo.Supplier s ON s.SupplierId  = b.SupplierId
    LEFT JOIN dbo.Customer c ON c.CustomerId  = b.CustomerId
    WHERE   b.Status = 2
      AND  (@BillType IS NULL OR b.BillType = @BillType)
      AND   b.BillDate BETWEEN @DateFrom AND @DateTo
    ORDER BY b.BillDate DESC, b.BillNo DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO


IF OBJECT_ID(N'dbo.usp_GetStockLedger', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetStockLedger;
GO

/* ============================================================================
   usp_GetStockLedger —— 指定物料的库存流水（带逐笔累计余额）
   ============================================================================ */
CREATE PROCEDURE dbo.usp_GetStockLedger
    @MaterialId INT,
    @DateFrom   DATE = NULL,
    @DateTo     DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @DateFrom = ISNULL(@DateFrom, DATEADD(DAY, -365, CAST(SYSDATETIME() AS DATE)));
    SET @DateTo   = ISNULL(@DateTo,   CAST(SYSDATETIME() AS DATE));

    SELECT  b.BillDate,
            b.BillNo,
            b.BillType,
            w.WarehouseName,
            ISNULL(s.SupplierName, c.CustomerName) AS PartnerName,
            CASE WHEN b.BillType IN (N'PI', N'ST') THEN d.Quantity ELSE 0 END AS InQty,
            CASE WHEN b.BillType IN (N'SO', N'RT') THEN d.Quantity ELSE 0 END AS OutQty,
            SUM(CASE WHEN b.BillType IN (N'PI', N'ST') THEN  d.Quantity
                     ELSE -d.Quantity END)
                OVER (ORDER BY b.BillDate, b.BillId
                      ROWS UNBOUNDED PRECEDING)                            AS RunningBalance
    FROM    dbo.BillDetail d
    JOIN    dbo.BillHead   b ON b.BillId      = d.BillId
    JOIN    dbo.Warehouse  w ON w.WarehouseId = b.WarehouseId
    LEFT JOIN dbo.Supplier s ON s.SupplierId  = b.SupplierId
    LEFT JOIN dbo.Customer c ON c.CustomerId  = b.CustomerId
    WHERE   d.MaterialId = @MaterialId
      AND   b.Status     = 2
      AND   b.BillDate BETWEEN @DateFrom AND @DateTo
    ORDER BY b.BillDate, b.BillId;
END
GO


/* ============================================================================
   自测：在 T-SQL 里直接调用
   ============================================================================ */
DECLARE @Total INT;

EXEC dbo.usp_GetBillList
     @BillType   = N'PI',
     @PageIndex  = 1,
     @PageSize   = 10,
     @TotalCount = @Total OUTPUT;

SELECT @Total AS TotalPurchaseBills;
GO

EXEC dbo.usp_GetStockLedger @MaterialId = 5;
GO

PRINT N'04-stored-procedures.sql 执行完成：2 个存储过程';
GO
