using System;
using System.Data;
using System.Data.SqlClient;

namespace ErpPractice.Data
{
    /// <summary>
    /// 参数构造助手。名字取短是因为它会在每一行 SQL 调用里出现。
    ///
    /// <para>
    /// ★ 为什么不用 <c>cmd.Parameters.AddWithValue("@x", value)</c>？
    /// </para>
    /// <para>
    /// AddWithValue 会<b>根据 .NET 值去猜</b> SQL 类型。这带来两个真实问题：
    /// </para>
    /// <list type="number">
    ///   <item>
    ///     <b>参数类型和列类型不一致，导致索引失效。</b>
    ///     比如 String 默认按 NVARCHAR 传，如果数据库列是 VARCHAR，
    ///     SQL Server 会做隐式转换，转换发生在"列"上，索引就用不上了。
    ///   </item>
    ///   <item>
    ///     <b>执行计划缓存被污染。</b>
    ///     同样的 SQL 第一次传入不同的长度/精度，后续可能复用一个不合适的计划。
    ///   </item>
    /// </list>
    /// <para>
    /// 所以这里全部<b>显式指定 SqlDbType 和长度</b>。多敲几个字，换来的
    /// 是确定的执行计划和正确的类型。
    /// </para>
    ///
    /// <para>示例：</para>
    /// <code>
    /// db.ExecuteDataTable(sql,
    ///     P.NVarChar("@Keyword", "轴", 50),
    ///     P.Int("@PageIndex", 1),
    ///     P.Dec("@Amount", 12.5m),
    ///     P.Date("@BillDate", DateTime.Today));
    /// </code>
    /// </summary>
    public static class P
    {
        /// <summary>NVARCHAR，对应 C# string。size 要和列的 NVARCHAR(n) 一致。</summary>
        public static SqlParameter NVarChar(string name, string value, int size = 100)
        {
            return new SqlParameter(name, SqlDbType.NVarChar, size)
            {
                Value = (object)value ?? DBNull.Value
            };
        }

        /// <summary>VARCHAR，用于确认列是 VARCHAR 而非 NVARCHAR 的场景。</summary>
        public static SqlParameter VarChar(string name, string value, int size = 100)
        {
            return new SqlParameter(name, SqlDbType.VarChar, size)
            {
                Value = (object)value ?? DBNull.Value
            };
        }

        /// <summary>INT，支持 int?（传 null 会自动变成 DBNull）。</summary>
        public static SqlParameter Int(string name, int? value)
        {
            return new SqlParameter(name, SqlDbType.Int)
            {
                Value = (object)value ?? DBNull.Value
            };
        }

        /// <summary>DECIMAL(18,4)，金额和数量统一用它。</summary>
        public static SqlParameter Dec(string name, decimal? value)
        {
            return new SqlParameter(name, SqlDbType.Decimal)
            {
                Value     = (object)value ?? DBNull.Value,
                Precision = 18,
                Scale     = 4
            };
        }

        /// <summary>BIT，对应 C# bool。</summary>
        public static SqlParameter Bit(string name, bool? value)
        {
            return new SqlParameter(name, SqlDbType.Bit)
            {
                Value = (object)value ?? DBNull.Value
            };
        }

        /// <summary>DATE（只有日期）。需要精确到时间用 DateTime。</summary>
        public static SqlParameter Date(string name, DateTime? value)
        {
            return new SqlParameter(name, SqlDbType.Date)
            {
                Value = (object)value ?? DBNull.Value
            };
        }

        /// <summary>DATETIME2（精确到秒/毫秒）。</summary>
        public static SqlParameter DateTime2(string name, DateTime? value)
        {
            return new SqlParameter(name, SqlDbType.DateTime2)
            {
                Value = (object)value ?? DBNull.Value
            };
        }

        /// <summary>
        /// 输出参数。调用后读 <see cref="SqlParameter.Value"/> 拿结果。
        /// <code>
        /// var total = P.Output("@TotalCount", SqlDbType.Int);
        /// db.ExecuteStoredProcedureNonQuery("dbo.usp_GetBillList",
        ///     P.NVarChar("@BillType", "PI", 10), total);
        /// Console.WriteLine(total.Value);   // 注意：不能用 total.Value 直接当 int 用
        /// </code>
        /// </summary>
        public static SqlParameter Output(string name, SqlDbType dbType, int size = 0)
        {
            var p = size > 0
                ? new SqlParameter(name, dbType, size)
                : new SqlParameter(name, dbType);

            p.Direction = ParameterDirection.Output;
            return p;
        }

        /// <summary>输入输出参数。</summary>
        public static SqlParameter InputOutput(string name, SqlDbType dbType, object value, int size = 0)
        {
            var p = size > 0
                ? new SqlParameter(name, dbType, size)
                : new SqlParameter(name, dbType);

            p.Direction = ParameterDirection.InputOutput;
            p.Value = value ?? DBNull.Value;
            return p;
        }

        /// <summary>返回值参数（存储过程的 RETURN 值）。</summary>
        public static SqlParameter ReturnValue(string name = "@ReturnValue")
        {
            return new SqlParameter(name, SqlDbType.Int)
            {
                Direction = ParameterDirection.ReturnValue
            };
        }
    }
}
