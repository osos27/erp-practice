/* ============================================================================
   02-seed.sql —— 造练习数据
   ----------------------------------------------------------------------------
   主数据手写，单据用 T-SQL 循环生成（真实项目里种子数据也都这么干，
   手敲 1000 张单不现实）。
   执行：sqlcmd -S localhost -E -f 65001 -i 02-seed.sql
   可重复执行。
   ============================================================================ */

USE ERP_Practice;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ---------------------------------------------------------------------------
   清空数据（保留表结构）
   --------------------------------------------------------------------------- */
DELETE FROM dbo.BillDetail;
DELETE FROM dbo.BillHead;
DELETE FROM dbo.Material;
DELETE FROM dbo.Warehouse;
DELETE FROM dbo.Supplier;
DELETE FROM dbo.Customer;

/* ★ 这里故意不写 DBCC CHECKIDENT(..., RESEED, 0) 把自增重置回 1。
   原因有两个：
     1. 重置不重置本来就无所谓 —— 下面取随机 ID 时用的是"实际的最小/最大值"，
        而不是假设 ID 从 1 连续到 N；
     2. RESEED 本身是个坑：对一张"从没插过数据"的表执行 RESEED 0，
       下一条记录的 ID 会是 0 而不是 1。第一版脚本就是因为假设了
        物料 ID 是 1~24，结果实际是 0~23，插明细时撞了外键。
   结论：业务代码永远不要假设自增主键从 1 开始、也不要假设连续（删除会留空洞）。 */
GO

/* ---------------------------------------------------------------------------
   仓库
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Warehouse (WarehouseCode, WarehouseName, Location) VALUES
    (N'WH01', N'一号原料库', N'A 区 1 号库房'),
    (N'WH02', N'二号成品库', N'B 区 2 号库房'),
    (N'WH03', N'三号备件库', N'C 区 3 号库房');
GO

/* ---------------------------------------------------------------------------
   物料（24 条，覆盖 5 个分类，含安全库存）
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Material (MaterialCode, MaterialName, Spec, Unit, Category, SafetyStock) VALUES
    (N'M0010', N'六角螺栓',     N'M8x30',        N'个', N'紧固件',  200),
    (N'M0020', N'六角螺母',     N'M8',           N'个', N'紧固件',  200),
    (N'M0030', N'平垫圈',       N'M8',           N'个', N'紧固件',  300),
    (N'M0040', N'内六角螺栓',   N'M10x40',       N'个', N'紧固件',  150),
    (N'M0050', N'不锈轴',       N'φ20x300',      N'根', N'机加工件', 30),
    (N'M0060', N'铝合金板',     N'5052 2mm',     N'张', N'型材',    50),
    (N'M0070', N'冷轧钢板',     N'SPCC 1.5mm',   N'张', N'型材',    40),
    (N'M0080', N'铜排',         N'40x4',         N'米', N'型材',    80),
    (N'M0090', N'聚氨酯密封圈', N'φ50',          N'个', N'密封件',  100),
    (N'M0100', N'O 型圈',       N'20x2.4',       N'个', N'密封件',  300),
    (N'M0110', N'轴承',         N'6204-2RS',     N'个', N'标准件',  60),
    (N'M0120', N'轴承',         N'6308',         N'个', N'标准件',  40),
    (N'M0130', N'同步带',       N'HTD-5M-800',   N'根', N'传动件',  25),
    (N'M0140', N'联轴器',       N'φ25',          N'个', N'传动件',  20),
    (N'M0150', N'伺服电机',     N'750W',         N'台', N'电气件',  10),
    (N'M0160', N'接近开关',     N'M18 PNP',      N'个', N'电气件',  40),
    (N'M0170', N'光电编码器',   N'1024P',        N'个', N'电气件',  15),
    (N'M0180', N'电缆',         N'RVV 4x1.5',    N'米', N'电气件',  200),
    (N'M0190', N'交流接触器',   N'CJX2-2510',    N'个', N'电气件',  30),
    (N'M0200', N'微型断路器',   N'DZ47-63 C16',  N'个', N'电气件',  30),
    (N'M0210', N'PLC 模块',     N'CPU1214C',     N'台', N'电气件',  5),
    (N'M0220', N'触摸屏',       N'7 寸',         N'台', N'电气件',  5),
    (N'M0230', N'润滑油',       N'32#',          N'升', N'辅料',    100),
    (N'M0240', N'砂纸',         N'400 目',       N'张', N'辅料',    200);
GO

/* ---------------------------------------------------------------------------
   供应商 / 客户
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Supplier (SupplierCode, SupplierName, Contact, Phone) VALUES
    (N'S001', N'上海恒力紧固件有限公司', N'张伟', N'021-12340001'),
    (N'S002', N'苏州精密机械制造厂',     N'李强', N'0512-1234002'),
    (N'S003', N'东莞华兴电子元件公司',   N'王芳', N'0769-1234003'),
    (N'S004', N'宁波金属材料交易所',     N'陈刚', N'0574-1234004'),
    (N'S005', N'无锡传动设备有限公司',   N'刘洋', N'0510-1234005'),
    (N'S006', N'深圳自动化元器件商行',   N'赵敏', N'0755-1234006');
GO

INSERT INTO dbo.Customer (CustomerCode, CustomerName, Contact, Phone) VALUES
    (N'C001', N'杭州德力机械有限公司',   N'孙鹏', N'0571-2234001'),
    (N'C002', N'南京中科智能装备公司',   N'周静', N'025-2234002'),
    (N'C003', N'合肥鑫源汽车零部件厂',   N'吴斌', N'0551-2234003'),
    (N'C004', N'青岛海联重工有限公司',   N'郑凯', N'0532-2234004'),
    (N'C005', N'成都西南精密制造公司',   N'冯雪', N'028-2234005'),
    (N'C006', N'武汉光谷自动化设备厂',   N'蒋涛', N'027-2234006'),
    (N'C007', N'天津北方机械集团',       N'韩磊', N'022-2234007'),
    (N'C008', N'西安秦川装备制造公司',   N'杨帆', N'029-2234008');
GO


/* ===========================================================================
   生成单据：1000 张单头，每张 1~4 行明细，日期散布在最近 180 天
   ---------------------------------------------------------------------------
   ★ 技巧说明（面试可以聊）：
   ABS(CHECKSUM(NEWID())) % N + 1   是 T-SQL 里生成随机整数的标准写法。
   为什么不直接用 RAND()？
     - RAND() 在同一个批处理里对每一行返回相同的值（除非用 RAND(CHECKSUM(NEWID()))）
     - NEWID() 每行都不同，CHECKSUM 把它转成整数，再取模就是随机数
   为什么要 CAST ... AS BIGINT？
     - CHECKSUM 可能返回 INT 最小值，ABS(int 最小值) 会溢出报错
   =========================================================================== */
DECLARE @i          INT = 1,
        @billCount  INT = 1000,
        @billId     INT,
        @billType   NVARCHAR(10),
        @billDate   DATE,
        @whId       INT,
        @supId      INT,
        @cusId      INT,
        @status     TINYINT,
        @lineCount  INT,
        @lineNo     INT,
        @matId      INT,
        @qty        DECIMAL(18,4),
        @price      DECIMAL(18,4),
        @matMin     INT, @matMax INT, @staleFrom INT,
        @whMin      INT, @whMax  INT,
        @supMin     INT, @supMax INT,
        @cusMin     INT, @cusMax INT;

/* 先取各主数据表的实际 ID 区间（而不是假设 1..N），
   这样脚本无论执行多少次、自增涨到多少，都能取到真实存在的 ID。 */
SELECT @matMin = MIN(MaterialId), @matMax = MAX(MaterialId) FROM dbo.Material;
SELECT @whMin  = MIN(WarehouseId), @whMax = MAX(WarehouseId) FROM dbo.Warehouse;
SELECT @supMin = MIN(SupplierId), @supMax = MAX(SupplierId) FROM dbo.Supplier;
SELECT @cusMin = MIN(CustomerId), @cusMax = MAX(CustomerId) FROM dbo.Customer;

IF @matMin IS NULL OR @matMax IS NULL
BEGIN
    RAISERROR (N'主数据为空，请先执行本脚本前面的 INSERT 部分。', 16, 1);
    RETURN;
END

/* 把最后 4 个物料当成"已停用"：它们只出现在 90 天前的单据里，
   这样 Q4「呆滞物料」才有结果可看。真实业务里那就是一批没人再用的料。 */
SET @staleFrom = @matMax - 3;

BEGIN TRANSACTION;

WHILE @i <= @billCount
BEGIN
    /* 随机单据类型：20% 为草稿（Status=1），用来验证"只统计已审核"的逻辑。

       这里用 CASE 而不是 CHOOSE(索引, 值1, 值2...)。
       CHOOSE 看着更短，但它对索引参数的类型很挑：索引表达式是 BIGINT 时
       会直接返回 NULL（不报错），然后一路把 BillNo 拼成 NULL，插库时才炸。
       这是个典型的"跑到一半才暴露"的坑 ——
       在 SQL 里遇到莫名其妙插入 NULL，第一反应就该去查 CHOOSE / NULLIF
       这类会静默返回 NULL 的函数。 */
    SET @billType = CASE ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % 4
                         WHEN 0 THEN N'PI'
                         WHEN 1 THEN N'SO'
                         WHEN 2 THEN N'RT'
                         ELSE        N'ST'
                    END;
    SET @billDate = DATEADD(DAY, -ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % 180,
                            CAST(SYSDATETIME() AS DATE));
    SET @whId     = @whMin  + ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % (@whMax  - @whMin  + 1);
    SET @supId    = @supMin + ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % (@supMax - @supMin + 1);
    SET @cusId    = @cusMin + ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % (@cusMax - @cusMin + 1);
    SET @status   = CASE WHEN ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % 20 = 0
                         THEN 1 ELSE 2 END;

    INSERT INTO dbo.BillHead (BillNo, BillType, BillDate, WarehouseId,
                              SupplierId, CustomerId, Status)
    VALUES (
        @billType + N'-' + CONVERT(NVARCHAR(8), @billDate, 112)
                  + N'-' + RIGHT(N'0000' + CAST(@i AS NVARCHAR(10)), 4),
        @billType,
        @billDate,
        @whId,
        CASE WHEN @billType IN (N'PI', N'RT') THEN @supId    ELSE NULL END,
        CASE WHEN @billType IN (N'SO', N'ST') THEN @cusId    ELSE NULL END,
        @status
    );

    SET @billId = SCOPE_IDENTITY();

    -- 每张单 1~4 行明细
    SET @lineCount = ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % 4 + 1;
    SET @lineNo    = 1;

    WHILE @lineNo <= @lineCount
    BEGIN
        /* 近 90 天的单据只用"在用物料"（@matMin .. @staleFrom-1），
           90 天之前才可能用到全部物料 —— 这样就有了真实的呆滞物料。 */
        IF @billDate >= DATEADD(DAY, -90, CAST(SYSDATETIME() AS DATE))
            SET @matId = @matMin + ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % (@staleFrom - @matMin);
        ELSE
            SET @matId = @matMin + ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % (@matMax - @matMin + 1);
        SET @qty   = CAST(ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % 90 + 10 AS DECIMAL(18,4));
        SET @price = CAST((ABS(CAST(CHECKSUM(NEWID()) AS BIGINT)) % 900 + 100) / 10.0 AS DECIMAL(18,4));

        -- 唯一约束 UQ_BillDetail_Bill_Material 保证同一张单不会重复同一物料，
        -- 这里先判断再插，避免撞约束报错
        IF NOT EXISTS (SELECT 1 FROM dbo.BillDetail
                       WHERE BillId = @billId AND MaterialId = @matId)
        BEGIN
            INSERT INTO dbo.BillDetail (BillId, MaterialId, Quantity, UnitPrice)
            VALUES (@billId, @matId, @qty, @price);
        END

        SET @lineNo = @lineNo + 1;
    END

    SET @i = @i + 1;
END

COMMIT TRANSACTION;
GO

/* ---------------------------------------------------------------------------
   造完之后核对一下数据量
   ★ 别名用了方括号：ROWCOUNT 是 T-SQL 保留字（SET ROWCOUNT），
     不加 [] 会直接报"关键字 'RowCount' 附近有语法错误"。
     遇到这种莫名其妙的语法错误，先怀疑别名撞了保留字。
   --------------------------------------------------------------------------- */
SELECT N'Material'   AS TableName, COUNT(*) AS [RowCnt] FROM dbo.Material
UNION ALL SELECT N'Warehouse',  COUNT(*) FROM dbo.Warehouse
UNION ALL SELECT N'Supplier',   COUNT(*) FROM dbo.Supplier
UNION ALL SELECT N'Customer',   COUNT(*) FROM dbo.Customer
UNION ALL SELECT N'BillHead',   COUNT(*) FROM dbo.BillHead
UNION ALL SELECT N'BillDetail', COUNT(*) FROM dbo.BillDetail;
GO
