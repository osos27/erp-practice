using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ErpPractice.Data
{
    /// <summary>
    /// 轻量数据访问帮助类（ADO.NET + SQL Server / .NET Framework 4.x）
    ///
    /// 设计目标只有一个：<b>把"打开连接、执行、释放"这套样板代码收进一处</b>，
    /// 让业务代码里只剩下一行 SQL 和几个参数。
    ///
    /// <para>它解决的四件事：</para>
    /// <list type="number">
    ///   <item>资源释放 —— 所有连接/命令/读取器都在 using 里，异常也不会泄漏</item>
    ///   <item>参数化查询 —— 从 API 层面就不提供字符串拼接的入口，防 SQL 注入</item>
    ///   <item>事务边界 —— 一组操作要么全成功要么全回滚，不用每处手写 try/catch</item>
    ///   <item>类型转换 —— DBNull 和可空类型统一处理，业务层不用到处判 null</item>
    /// </list>
    ///
    /// <para>
    /// 使用方式：
    /// <code>
    /// var db = new SqlHelper(connStr);
    /// var rows = db.ExecuteDataTable("SELECT * FROM dbo.Material WHERE Category = @c",
    ///                                P.NVarChar("@c", "紧固件"));
    /// </code>
    /// </para>
    /// </summary>
    public sealed class SqlHelper
    {
        private readonly string _connectionString;

        public SqlHelper(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("连接字符串不能为空", nameof(connectionString));

            _connectionString = connectionString;
        }

        public string ConnectionString => _connectionString;

        /// <summary>命令超时（秒）。报表跑得久可以临时调大。</summary>
        public int CommandTimeout { get; set; } = 30;


        #region 内部：连接与命令的构造

        /// <summary>
        /// 打开一个新连接。
        /// <para>
        /// ★ 别被名字骗了：连接池默认是开启的。
        /// 这里的 Open() 并不是"建立一条新的物理 TCP 连接"，而是从池里借一条现成的；
        /// 对应的 Close()/Dispose() 也不是断开，而是<b>还回池里</b>。
        /// 所以"每次查询都 Open 一次"并不慢，反而是 ADO.NET 的标准写法 ——
        /// 真正的错误做法是把 SqlConnection 做成单例长期持有（会有并发和状态问题）。
        /// </para>
        /// </summary>
        private SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct)
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return conn;
        }

        private SqlCommand CreateCommand(
            SqlConnection conn,
            SqlTransaction tran,
            CommandType commandType,
            string sql,
            SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL 语句不能为空", nameof(sql));

            var cmd = conn.CreateCommand();
            cmd.CommandType = commandType;
            cmd.CommandText = sql;
            cmd.CommandTimeout = CommandTimeout;

            if (tran != null)
                cmd.Transaction = tran;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p != null)
                        cmd.Parameters.Add(p);
                }
            }

            return cmd;
        }

        #endregion


        #region 同步：增删改

        /// <summary>
        /// 执行 INSERT / UPDATE / DELETE，返回受影响行数。
        /// </summary>
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 在指定事务里执行 INSERT / UPDATE / DELETE。
        /// <para>连接由事务持有，所以这里<b>不能</b>using 连接，只能 using 命令。</para>
        /// </summary>
        public int ExecuteNonQuery(SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            using (var cmd = CreateCommand(transaction.Connection, transaction, CommandType.Text, sql, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        #endregion


        #region 同步：单值查询

        /// <summary>
        /// 查第一行第一列，没有结果返回 null。
        /// </summary>
        public object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            {
                return cmd.ExecuteScalar();
            }
        }

        public object ExecuteScalar(SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            using (var cmd = CreateCommand(transaction.Connection, transaction, CommandType.Text, sql, parameters))
            {
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// 泛型单值查询，自动处理 DBNull 与可空类型。
        /// <code>
        /// int total = db.ExecuteScalar&lt;int&gt;("SELECT COUNT(*) FROM dbo.Material");
        /// decimal? avg = db.ExecuteScalar&lt;decimal?&gt;("SELECT AVG(Amount) FROM dbo.BillDetail");
        /// </code>
        /// </summary>
        public T ExecuteScalar<T>(string sql, params SqlParameter[] parameters)
        {
            return ConvertValue<T>(ExecuteScalar(sql, parameters));
        }

        public T ExecuteScalar<T>(SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            return ConvertValue<T>(ExecuteScalar(transaction, sql, parameters));
        }

        #endregion


        #region 同步：表格查询

        /// <summary>
        /// 查询到 DataTable（一次性把结果读进内存）。
        /// 适合中小结果集、需要给 GridView/报表控件绑定的场景。
        /// </summary>
        public DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
        {
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        /// <summary>查询到 DataSet（多条 SELECT 语句的结果集）。</summary>
        public DataSet ExecuteDataSet(string sql, params SqlParameter[] parameters)
        {
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        #endregion


        #region 同步：映射成对象（推荐用法）

        /// <summary>
        /// 用 DataReader 逐行读取并映射成对象。
        /// <para>
        /// ★ 和大结果集一起用时，这比 ExecuteDataTable 省内存 ——
        /// DataReader 是流式的，读一行处理一行，不会把整张表塞进内存。
        /// </para>
        /// <para>
        /// ★ mapper 里<b>不能</b>保存 reader 的引用（reader 在方法返回时就被释放了）。
        /// </para>
        /// </summary>
        public List<T> ExecuteList<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            var list = new List<T>();
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    list.Add(mapper(reader));
            }
            return list;
        }

        public List<T> ExecuteList<T>(SqlTransaction transaction, string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            var list = new List<T>();
            using (var cmd = CreateCommand(transaction.Connection, transaction, CommandType.Text, sql, parameters))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    list.Add(mapper(reader));
            }
            return list;
        }

        /// <summary>
        /// 只取第一行做映射；没有数据时返回 default(T)（引用类型为 null）。
        /// </summary>
        public T ExecuteSingle<T>(string sql, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            using (var reader = cmd.ExecuteReader())
            {
                return reader.Read() ? mapper(reader) : default(T);
            }
        }

        #endregion


        #region 存储过程

        /// <summary>执行存储过程，返回 DataTable。</summary>
        public DataTable ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
        {
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.StoredProcedure, procedureName, parameters))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        /// <summary>执行存储过程（不返回结果集），返回受影响行数。</summary>
        public int ExecuteStoredProcedureNonQuery(string procedureName, params SqlParameter[] parameters)
        {
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.StoredProcedure, procedureName, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>执行存储过程并返回单值。</summary>
        public T ExecuteStoredProcedureScalar<T>(string procedureName, params SqlParameter[] parameters)
        {
            using (var conn = OpenConnection())
            using (var cmd = CreateCommand(conn, null, CommandType.StoredProcedure, procedureName, parameters))
            {
                return ConvertValue<T>(cmd.ExecuteScalar());
            }
        }

        #endregion


        #region 事务

        /// <summary>
        /// 在一个事务里执行一组操作，返回业务结果。
        /// <para>
        /// work 内部抛任何异常（包括 CHECK 约束冲突、主键重复）都会自动回滚。
        /// 正常返回则提交。
        /// </para>
        /// <code>
        /// var billId = db.ExecuteInTransaction(tran =>
        /// {
        ///     db.ExecuteNonQuery(tran, "INSERT INTO ...", P.NVarChar("@No", no));
        ///     var id = db.ExecuteScalar&lt;int&gt;(tran, "SELECT CAST(SCOPE_IDENTITY() AS INT);");
        ///     db.ExecuteNonQuery(tran, "INSERT INTO ... ", P.Int("@BillId", id));
        ///     return id;
        /// });
        /// </code>
        /// </summary>
        public T ExecuteInTransaction<T>(Func<SqlTransaction, T> work)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));

            using (var conn = OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    var result = work(tran);
                    tran.Commit();
                    return result;
                }
                catch
                {
                    try
                    {
                        tran.Rollback();
                    }
                    catch
                    {
                        // 连接已断开时回滚本身也会抛，这时原始异常更重要，
                        // 不能让它被回滚失败覆盖掉。
                    }
                    throw;
                }
            }
        }

        /// <summary>无返回值版本。</summary>
        public void ExecuteInTransaction(Action<SqlTransaction> work)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));

            ExecuteInTransaction<int>(tran =>
            {
                work(tran);
                return 0;
            });
        }

        #endregion


        #region 异步版本

        /*
         * 说明：桌面程序和 Web 里，同步方法会占住一个线程干等 IO。
         * 异步版本在等待数据库时把线程还回去，并发量高的时候差别很明显。
         * 但注意：await 之后的代码不保证还在原来的线程上 ——
         * 如果随后要更新 UI，记得切回 UI 线程（WinForm 用 Invoke/BeginInvoke，
         * WPF 用 Dispatcher，ASP.NET 里则一般不需要）。
         */

        public async Task<int> ExecuteNonQueryAsync(string sql, SqlParameter[] parameters = null, CancellationToken ct = default(CancellationToken))
        {
            using (var conn = await OpenConnectionAsync(ct).ConfigureAwait(false))
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            {
                return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        public async Task<object> ExecuteScalarAsync(string sql, SqlParameter[] parameters = null, CancellationToken ct = default(CancellationToken))
        {
            using (var conn = await OpenConnectionAsync(ct).ConfigureAwait(false))
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            {
                return await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, SqlParameter[] parameters = null, CancellationToken ct = default(CancellationToken))
        {
            var value = await ExecuteScalarAsync(sql, parameters, ct).ConfigureAwait(false);
            return ConvertValue<T>(value);
        }

        public async Task<List<T>> ExecuteListAsync<T>(string sql, Func<SqlDataReader, T> mapper, SqlParameter[] parameters = null, CancellationToken ct = default(CancellationToken))
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            var list = new List<T>();
            using (var conn = await OpenConnectionAsync(ct).ConfigureAwait(false))
            using (var cmd = CreateCommand(conn, null, CommandType.Text, sql, parameters))
            using (var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    list.Add(mapper(reader));
            }
            return list;
        }

        #endregion


        #region 类型转换

        /// <summary>
        /// 把数据库返回的 object 安全地转成 T。
        /// <para>
        /// 数据库的 NULL 在 ADO.NET 里是 <see cref="DBNull.Value"/> 而不是 C# 的 null，
        /// 直接 cast 会抛 InvalidCastException。这里统一处理掉，
        /// 同时支持可空类型（int? / decimal? / DateTime?）。
        /// </para>
        /// </summary>
        private static T ConvertValue<T>(object value)
        {
            if (value == null || value == DBNull.Value)
                return default(T);

            var target = typeof(T);
            var underlying = Nullable.GetUnderlyingType(target);
            if (underlying != null)
                target = underlying;

            if (target.IsInstanceOfType(value))
                return (T)value;

            return (T)Convert.ChangeType(value, target);
        }

        #endregion
    }
}
