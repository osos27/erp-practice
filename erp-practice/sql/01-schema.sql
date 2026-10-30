/* ============================================================================
   01-schema.sql —— ERP 练习库：建库 + 建表 + 约束 + 索引 + 视图
   ----------------------------------------------------------------------------
   目标环境：SQL Server 2012+（Express / LocalDB 均可）
   执行方式：sqlcmd -S localhost -E -f 65001 -i 01-schema.sql
             或者用 SSMS 打开直接 F5
   本脚本可重复执行：每次都会先 DROP 再 CREATE
   ----------------------------------------------------------------------------
   设计约定（面试可以直接讲）：
   1. 主键用 IDENTITY 自增 INT，业务编号（MaterialCode / BillNo）另加唯一约束
   2. 表名用单数、字段用 PascalCase，外键命名为 FK_子表_主表
   3. 金额/数量统一 DECIMAL(18,4)，绝不用 FLOAT（浮点误差在财务里是事故）
   4. 业务规则尽量写成 CHECK 约束，而不是散落在代码里的 if
   5. 库存不建表，它是单据的汇总结果（详见 vw_StockBalance 注释）
   ============================================================================ */

IF DB_ID(N'ERP_Practice') IS NULL
BEGIN
    CREATE DATABASE ERP_Practice;
END
GO

USE ERP_Practice;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   清理：按外键依赖倒序删除，保证脚本可以反复执行
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.vw_StockBalance',   N'V') IS NOT NULL DROP VIEW      dbo.vw_StockBalance;
IF OBJECT_ID(N'dbo.usp_GetBillList',   N'P') IS NOT NULL DROP PROCEDURE dbo.usp_GetBillList;
IF OBJECT_ID(N'dbo.usp_GetStockLedger',N'P') IS NOT NULL DROP PROCEDURE dbo.usp_GetStockLedger;
IF OBJECT_ID(N'dbo.BillDetail', N'U') IS NOT NULL DROP TABLE dbo.BillDetail;
IF OBJECT_ID(N'dbo.BillHead',   N'U') IS NOT NULL DROP TABLE dbo.BillHead;
IF OBJECT_ID(N'dbo.Material',   N'U') IS NOT NULL DROP TABLE dbo.Material;
IF OBJECT_ID(N'dbo.Warehouse',  N'U') IS NOT NULL DROP TABLE dbo.Warehouse;
IF OBJECT_ID(N'dbo.Supplier',   N'U') IS NOT NULL DROP TABLE dbo.Supplier;
IF OBJECT_ID(N'dbo.Customer',   N'U') IS NOT NULL DROP TABLE dbo.Customer;
GO

/* ===========================================================================
   1. 物料主数据
   =========================================================================== */
CREATE TABLE dbo.Material
(
    MaterialId   INT IDENTITY(1,1) NOT NULL,
    MaterialCode NVARCHAR(30)   NOT NULL,          -- 业务编号，如 M0010
    MaterialName NVARCHAR(100)  NOT NULL,
    Spec         NVARCHAR(100)  NULL,              -- 规格型号
    Unit         NVARCHAR(10)   NOT NULL CONSTRAINT DF_Material_Unit     DEFAULT (N'个'),
    Category     NVARCHAR(30)   NOT NULL CONSTRAINT DF_Material_Category DEFAULT (N'其他'),
    SafetyStock  DECIMAL(18,4)  NOT NULL CONSTRAINT DF_Material_Safety   DEFAULT (0),
    IsActive     BIT            NOT NULL CONSTRAINT DF_Material_IsActive DEFAULT (1),
    CreatedAt    DATETIME2(0)   NOT NULL CONSTRAINT DF_Material_Created  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Material PRIMARY KEY CLUSTERED (MaterialId),
    CONSTRAINT UQ_Material_Code UNIQUE (MaterialCode),
    CONSTRAINT CK_Material_SafetyStock CHECK (SafetyStock >= 0)
);
GO

/* 索引不是越多越好：每一个索引都会拖慢 INSERT/UPDATE。
   这里只建两个真正会被 WHERE 用到的（Name 用于模糊查询、Category 用于分类筛选）。 */
CREATE INDEX IX_Material_Name     ON dbo.Material (MaterialName);
CREATE INDEX IX_Material_Category ON dbo.Material (Category, IsActive);
GO


/* ===========================================================================
   2. 仓库
   =========================================================================== */
CREATE TABLE dbo.Warehouse
(
    WarehouseId   INT IDENTITY(1,1) NOT NULL,
    WarehouseCode NVARCHAR(20)  NOT NULL,
    WarehouseName NVARCHAR(50)  NOT NULL,
    Location      NVARCHAR(100) NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_Warehouse_IsActive DEFAULT (1),

    CONSTRAINT PK_Warehouse PRIMARY KEY CLUSTERED (WarehouseId),
    CONSTRAINT UQ_Warehouse_Code UNIQUE (WarehouseCode)
);
GO


/* ===========================================================================
   3. 供应商 / 4. 客户
   =========================================================================== */
CREATE TABLE dbo.Supplier
(
    SupplierId   INT IDENTITY(1,1) NOT NULL,
    SupplierCode NVARCHAR(20)  NOT NULL,
    SupplierName NVARCHAR(100) NOT NULL,
    Contact      NVARCHAR(30)  NULL,
    Phone        NVARCHAR(30)  NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_Supplier_IsActive DEFAULT (1),

    CONSTRAINT PK_Supplier PRIMARY KEY CLUSTERED (SupplierId),
    CONSTRAINT UQ_Supplier_Code UNIQUE (SupplierCode)
);
GO

CREATE TABLE dbo.Customer
(
    CustomerId   INT IDENTITY(1,1) NOT NULL,
    CustomerCode NVARCHAR(20)  NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    Contact      NVARCHAR(30)  NULL,
    Phone        NVARCHAR(30)  NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_Customer_IsActive DEFAULT (1),

    CONSTRAINT PK_Customer PRIMARY KEY CLUSTERED (CustomerId),
    CONSTRAINT UQ_Customer_Code UNIQUE (CustomerCode)
);
GO


/* ===========================================================================
   5. 单据主表（一个"单头"）
   ---------------------------------------------------------------------------
   单据类型：
     PI = 采购入库    SO = 销售出库
     RT = 采购退货    ST = 销售退货
   方向上：PI、ST 是入库；SO、RT 是出库
   =========================================================================== */
CREATE TABLE dbo.BillHead
(
    BillId      INT IDENTITY(1,1) NOT NULL,
    BillNo      NVARCHAR(30)  NOT NULL,
    BillType    NVARCHAR(10)  NOT NULL,
    BillDate    DATE          NOT NULL,
    WarehouseId INT           NOT NULL,
    SupplierId  INT           NULL,                -- 采购类单据填这个
    CustomerId  INT           NULL,                -- 销售类单据填这个
    Status      TINYINT       NOT NULL CONSTRAINT DF_BillHead_Status   DEFAULT (2), -- 1草稿 2已审核 3作废
    Remark      NVARCHAR(200) NULL,
    CreatedAt   DATETIME2(0)  NOT NULL CONSTRAINT DF_BillHead_Created  DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_BillHead PRIMARY KEY CLUSTERED (BillId),
    CONSTRAINT UQ_BillHead_BillNo UNIQUE (BillNo),

    CONSTRAINT FK_BillHead_Warehouse FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouse (WarehouseId),
    CONSTRAINT FK_BillHead_Supplier  FOREIGN KEY (SupplierId)  REFERENCES dbo.Supplier  (SupplierId),
    CONSTRAINT FK_BillHead_Customer  FOREIGN KEY (CustomerId)  REFERENCES dbo.Customer  (CustomerId),

    CONSTRAINT CK_BillHead_Status CHECK (Status IN (1, 2, 3)),

    /* ★ 重要示范：把业务规则写进数据库，而不是只靠代码里的 if。
       "采购单据必须有供应商、销售单据必须有客户" —— 这条规则写在代码里，
       只要有一个人绕过程序直接改库，数据就脏了。写在这里，谁也绕不过去。 */
    CONSTRAINT CK_BillHead_Partner CHECK
    (
        (BillType IN (N'PI', N'RT') AND SupplierId IS NOT NULL AND CustomerId IS NULL) OR
        (BillType IN (N'SO', N'ST') AND CustomerId IS NOT NULL AND SupplierId IS NULL)
    )
);
GO

/* 覆盖索引（INCLUDE）：查询只需要这几列时，可以直接从索引里取，
   不用回表。这是 ERP 报表优化的常用手段，面试加分点。 */
CREATE INDEX IX_BillHead_BillDate ON dbo.BillHead (BillDate) INCLUDE (BillType, WarehouseId, Status);
CREATE INDEX IX_BillHead_TypeDate ON dbo.BillHead (BillType, BillDate);
GO


/* ===========================================================================
   6. 单据明细（一张单可以有多行物料）
   =========================================================================== */
CREATE TABLE dbo.BillDetail
(
    DetailId   INT IDENTITY(1,1) NOT NULL,
    BillId     INT           NOT NULL,
    MaterialId INT           NOT NULL,
    Quantity   DECIMAL(18,4) NOT NULL,
    UnitPrice  DECIMAL(18,4) NOT NULL CONSTRAINT DF_BillDetail_Price DEFAULT (0),

    /* ★ 计算列：金额永远由 数量 × 单价 推导出来，不允许单独写入。
       如果让 Amount 变成一个可以随便写的普通字段，迟早会出现
       "数量 10、单价 5、金额 60" 这种三方打架的数据。 */
    Amount AS (CAST(Quantity * UnitPrice AS DECIMAL(18,4))) PERSISTED,

    CONSTRAINT PK_BillDetail PRIMARY KEY CLUSTERED (DetailId),
    CONSTRAINT FK_BillDetail_Bill     FOREIGN KEY (BillId)     REFERENCES dbo.BillHead (BillId),
    CONSTRAINT FK_BillDetail_Material FOREIGN KEY (MaterialId) REFERENCES dbo.Material (MaterialId),
    CONSTRAINT CK_BillDetail_Qty   CHECK (Quantity  >  0),
    CONSTRAINT CK_BillDetail_Price CHECK (UnitPrice >= 0),

    /* 同一张单里同一个物料只能出现一行 —— 业务上叫"明细去重" */
    CONSTRAINT UQ_BillDetail_Bill_Material UNIQUE (BillId, MaterialId)
);
GO

CREATE INDEX IX_BillDetail_BillId     ON dbo.BillDetail (BillId);
CREATE INDEX IX_BillDetail_MaterialId ON dbo.BillDetail (MaterialId) INCLUDE (Quantity, Amount);
GO


/* ===========================================================================
   7. 库存视图
   ---------------------------------------------------------------------------
   ★ 这里是整个练习库里最值钱的一个设计决定：故意不建"库存表"。

   新手最常见的做法是建一张 Inventory 表存当前库存，然后每次出入库去 UPDATE 它。
   问题在于：只要有任何一条路径漏了更新（补单、反审、批量导入、直接改库），
   库存数字就会和单据永远对不上，而且你永远不知道是哪一步错的。

   正确做法是"库存 = 流水的汇总"，需要实时余额就用视图/物化视图算出来。
   真实系统里通常会有一张 StockMovement 库存流水表来承载，
   这里为了练习 join + 聚合，直接从单据算。
   =========================================================================== */
CREATE VIEW dbo.vw_StockBalance
AS
SELECT  w.WarehouseId,
        w.WarehouseCode,
        w.WarehouseName,
        m.MaterialId,
        m.MaterialCode,
        m.MaterialName,
        m.Unit,
        m.SafetyStock,
        SUM(CASE WHEN b.BillType IN (N'PI', N'ST') THEN d.Quantity ELSE 0 END) AS InQty,
        SUM(CASE WHEN b.BillType IN (N'SO', N'RT') THEN d.Quantity ELSE 0 END) AS OutQty,
        SUM(CASE WHEN b.BillType IN (N'PI', N'ST') THEN d.Quantity
                 ELSE -d.Quantity END)                                          AS BalanceQty
FROM    dbo.BillDetail d
JOIN    dbo.BillHead   b ON b.BillId     = d.BillId
JOIN    dbo.Material   m ON m.MaterialId = d.MaterialId
JOIN    dbo.Warehouse  w ON w.WarehouseId = b.WarehouseId
WHERE   b.Status = 2                      -- 只统计"已审核"单据：草稿和作废不算
GROUP BY w.WarehouseId, w.WarehouseCode, w.WarehouseName,
         m.MaterialId, m.MaterialCode, m.MaterialName, m.Unit, m.SafetyStock;
GO

PRINT N'01-schema.sql 执行完成：6 张表 + 1 个视图 + 6 个显式索引 + 6 个唯一约束 + 业务 CHECK 约束';
GO
