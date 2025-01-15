using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Transactions;
using System.Windows.Input;

#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable IDE0028 // コレクションの初期化を簡素化できます
#pragma warning disable IDE0063 // 'using' ステートメントは簡素化できます
#pragma warning disable IDE0090 // 'new' 式を簡素化できます


namespace GeneralUtility
{
    /// <summary>
    /// DB操作用の拡張メソッドを定義した静的クラス
    /// </summary>
    public static class DBHelperExtentions
    {
        /// <summary>
        /// DBに接続する（接続リトライ付き）
        /// </summary>
        /// <param name="connString">DB接続情報</param>
        /// <param name="retryNum">リトライ回数</param>
        /// <param name="getRetryIntervalMSec">リトライx回目を元にリトライ間隔(ミリ秒)を返す関数</param>
#if NET5_0_OR_GREATER
        public static void OpenWithRetry(this DbConnection connection,
            int retryNum = 3,
            Func<int, int>? getRetryIntervalMSec = null,
            Func<Exception, bool>? doRetry = null)
#else
        public static void OpenWithRetry(this DbConnection connection,
            int retryNum = 0,
            Func<int, int> getRetryIntervalMSec = null,
            Func<Exception, bool> doRetry = null)
#endif
        {
            int i = 0;
            for (; i <= retryNum; i++)
            {
                try
                {
                    connection.Open();
                    break;
                }
                catch (Exception ex)
                {
                    if (i < retryNum)
                    {
                        if (doRetry != null && doRetry(ex) == false)
                        {
                            break;
                        }

                        if (getRetryIntervalMSec != null)
                        {
                            Thread.Sleep(getRetryIntervalMSec(i + 1));
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// DBに接続する（接続リトライ付き）
        /// </summary>
        /// <param name="connString">DB接続情報</param>
        /// <param name="retryNum">リトライ回数</param>
        /// <param name="getRetryIntervalMSec">リトライx回目を元にリトライ間隔(ミリ秒)を返す関数</param>
        public static void OpenWithRetry(this DbConnection connection,
            int retryNum = 0,
            int retryIntervalMSec = 0)
        {
            OpenWithRetry(connection, retryNum, (_) => retryIntervalMSec);
        }


        /// <summary>
        /// DbCommandオブジェクトを取得する
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        /// <param name="params">SQLに渡すパラメータ</param>
        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        public static DbCommand GetDbCommand(this DbConnection connection,
            string sql,
            Dictionary<string, object>? parameters = null,
            DbTransaction? transaction = null,
            int? timeoutSec = null)
#else
        public static DbCommand GetDbCommand(this DbConnection connection,
            string sql,
            Dictionary<string, object> parameters = null,
            DbTransaction transaction = null,
            int? timeoutSec = null)
#endif
        {
            var dbCommand = connection.CreateCommand();
            dbCommand.CommandText = sql;

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> item in parameters)
                {
                    var dbParameter = dbCommand.CreateParameter();
                    dbParameter.ParameterName = item.Key;
                    dbParameter.Value = item.Value;
                    dbCommand.Parameters.Add(dbParameter);
                }
            }

            if (transaction != null)
            {
                dbCommand.Transaction = transaction;
            }

            if (timeoutSec != null)
            {
                dbCommand.CommandTimeout = timeoutSec.Value;
            }

            return dbCommand;
        }


        /// <summary>
        /// 参照SQLの実行して DataTable として取得する
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        /// <param name="params">SQLに渡すパラメータ</param>
        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        /// <returns>取得した DataTable</returns>
#if NET5_0_OR_GREATER
        public static DataTable QueryToDataTable(this DbConnection connection,
            Type classTypeDbDataAdapter, // typeof(DbDataAdapter);
            string sql,
            Dictionary<string, object>? parameters = null,
            DbTransaction? transaction = null,
            int? timeoutSec = null)
#else
        public static DataTable QueryToDataTable(this DbConnection connection,
            Type classTypeDbDataAdapter, // typeof(DbDataAdapter);
            string sql,
            Dictionary<string, object> @parameters = null,
            DbTransaction transaction = null,
            int? timeoutSec = null)
#endif
        {
            using (var dbCommand = connection.GetDbCommand(
                sql, parameters: parameters, transaction: transaction, timeoutSec: timeoutSec))
            {
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
                using (var adapter = (DbDataAdapter)Activator.CreateInstance(classTypeDbDataAdapter, dbCommand)
                    ?? throw new InvalidOperationException($"Cannot create object of class {classTypeDbDataAdapter.Name}"))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
            }
        }


        /// <summary>
        /// 参照SQLの実行して IEnumerable&lt;IDataRecord&gt; として取得する
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        /// <param name="params">SQLに渡すパラメータ</param>
        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        /// <returns>取得した IEnumerable&lt;IDataRecord&gt;</returns>
        /// <remarks>戻り値の IDataReader は呼び元で Dispose すること。</remarks>
#if NET5_0_OR_GREATER
        public static IDataReader Query(this DbConnection connection,
            string sql,
            Dictionary<string, object>? parameters = null,
            DbTransaction? transaction = null,
            int? timeoutSec = null)
#else
        public static IDataReader Query(this DbConnection connection,
            string sql,
            Dictionary<string, object> parameters = null,
            DbTransaction transaction = null,
            int? timeoutSec = null)
#endif
        {
            using (var command = GetDbCommand(connection, sql, parameters, transaction, timeoutSec))
            {
                return command.ExecuteReader();
            }
        }


        /// <summary>
        /// 参照SQLの実行して単一値取得する
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        /// <param name="params">SQLに渡すパラメータ</param>
        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        /// <returns>取得した値</returns>
#if NET5_0_OR_GREATER
        public static T? QueryScalar<T>(this DbConnection connection,
            string sql,
            Dictionary<string, object>? parameters = null,
            DbTransaction? transaction = null,
            int? timeoutSec = null)
#else
        public static T QueryScalar<T>(this DbConnection connection,
            string sql,
            Dictionary<string, object> parameters = null,
            DbTransaction transaction = null,
            int? timeoutSec = null)
#endif
        {
            using (var dbCommand = GetDbCommand(connection, sql, parameters, transaction, timeoutSec))
            {
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
                return (T)dbCommand.ExecuteScalar();
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
            }
        }


        /// <summary>
        /// 非参照系SQLの実行する
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        /// <param name="params">SQLに渡すパラメータ</param>
        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        /// <returns>実行された結果、影響を受けたレコード数</returns>
#if NET5_0_OR_GREATER
        public static int Execute(this DbConnection connection,
            string sql,
            Dictionary<string, object>? parameters = null,
            DbTransaction? transaction = null,
            int? timeoutSec = null)
#else
        public static int Execute(this DbConnection connection,
            string sql,
            Dictionary<string, object> parameters = null,
            DbTransaction transaction = null,
            int? timeoutSec = null)
#endif
        {
            using (var dbCommand = GetDbCommand(connection, sql, parameters, transaction, timeoutSec))
            {
                return dbCommand.ExecuteNonQuery();
            }
        }


        //        /// <summary>
        //        /// SQLスクリプトを実行する
        //        /// </summary>
        //        /// <param name="script">実行するSQLスクリプト</param>
        //        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        //        public virtual void RunScript(string script, int timeoutSec = 0)
        //        {
        //            var errScript = script.Length > 100 ? script.Substring(0, 100) + " ...\n..." : script;
        //            var msg = string.Format("Not Implemented.\nScript: \n{0}", errScript);
        //            throw new NotImplementedException(msg);
        //        }


        //        /// <summary>
        //        /// SQLスクリプトファイルを読み取り実行する
        //        /// </summary>
        //        /// <param name="fileName">SQLスクリプトファイル名</param>
        //        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        //        public virtual void RunScriptFile(string fileName, int timeoutSec = 0)
        //        {
        //            var script = File.ReadAllText(fileName);
        //            RunScript(script, timeoutSec);
        //        }


        //        /// <summary>
        //        /// 一括挿入を行う
        //        /// </summary>
        //        /// <typeparam name="T">Entityクラス</typeparam>
        //        /// <param name="tableName">挿入先のテーブル名</param>
        //        /// <param name="source">挿入元のデータコレクション</param>
        //        /// <param name="columnMappings">Entityクラスのフィールドをテーブルのカラムに対応させる辞書</param>
        //        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        //        /// <exception cref="NotImplementedException"></exception>
        //        public virtual void BulkInsert<T>(
        //            string tableName,
        //            IEnumerable<T> source,
        //            IDictionary<string, string> columnMappings,
        //            int timeoutSec = 0)
        //        {
        //            throw new NotImplementedException();
        //        }


        //        /// <summary>
        //        /// 一括挿入を行う
        //        /// </summary>
        //        /// <typeparam name="T">Entityクラス</typeparam>
        //        /// <param name="tableName">挿入先のテーブル名</param>
        //        /// <param name="source">挿入元のデータコレクション</param>
        //        /// <param name="timeoutSec">実行タイムアウト(秒)</param>
        //        /// <remarks>Entityクラスのフィールド名とテーブルのカラム名は一致している必要がある</remarks>
        //        public virtual void BulkInsert<T>(string tableName, IEnumerable<T> source, int timeoutSec = 0)
        //        {
        //            var columnMappings = typeof(T).GetProperties().ToDictionary(s => s.Name, s => s.Name);
        //            BulkInsert(tableName, source, columnMappings, timeoutSec);
        //        }
        //    }


        /// <summary>
        /// IDataRecordから、dynamic に変換する。
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// <remarks>dynamic (ExpandoObject) の生成は、IDataRecord を引数とする。</remarks>
        public static dynamic ToExpando(this IDataRecord dr)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            for (var i = 0; i < dr.FieldCount; i++)
            {
                expandoObject.Add(dr.GetName(i), dr[i]);
            }
            return expandoObject;
        }


        /// <summary>
        /// IDataReader を IEnumerable&lt;dynamic&gt; に変換する。
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isDispose"></param>
        /// <returns></returns>
        /// <remarks>dynamic (ExpandoObject) の生成は、IDataRecord を引数とする。</remarks>
        public static IEnumerable<dynamic> ToExpando(this IDataReader dr, bool isDispose = true)
        {
            while (dr.Read())
            {
                yield return ((IDataRecord)dr).ToExpando();
            }

            if (isDispose)
            {
                dr.Dispose();
            }
        }


        /// <summary>
        /// IDataReader を IEnumerable&lt;Dictionary&lt;string, object&gt;&gt; に変換する。
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isDispose"></param>
        /// <returns></returns>
        public static IEnumerable<IDictionary<string, object>> ToDictionary(this IDataReader dr, bool isDispose = true)
        {
            return dr.ToExpando(isDispose).Cast<IDictionary<string, object>>();
        }


        /// <summary>
        /// IDataReader を IEnumerable&lt;T&gt; に変換する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="createEntiry"></param>
        /// <returns></returns>
        /// <remarks>T の生成は、IDataRecord を IDictionary に変換したオブジェクトを引数とする。</remarks>
        public static IEnumerable<T> ToEntity<T>(this IDataReader dr, Func<IDictionary<string, object>, T> createEntiry)
        {
            return dr.ToDictionary().Select(s => createEntiry(s));
        }


        /// <summary>
        /// IDataRecord を Entity に変換する
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// <remarks>
        /// IDataRecord の列名と一致する T のプロパティにマッピングする。(小文字にして比較)
        /// <br/>一致するプロパティ名がない場合はなにもしない。
        /// </remarks>
        public static T ToEntity<T>(this IDataRecord dr) where T : new()
        {
            var obj = new T();
            var theType = obj.GetType();
            var p = theType.GetProperties().ToDictionary(s => s.Name.ToLower(), s => s);
            foreach (var i in Enumerable.Range(0, dr.FieldCount))
            {
                var fieldName = dr.GetName(i);
                if (p.ContainsKey(fieldName.ToLower()))
                {
                    if (dr[fieldName] != DBNull.Value)
                    {
                        p[fieldName].SetValue(obj, dr[fieldName], null);
                    }
                    else
                    {
                        p[fieldName].SetValue(obj, null, null);
                    }
                }
            }
            return obj;
        }


        /// <summary>
        /// IDataReader でラップした IEnumerable を返す (BulkCopyで使用可能)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDataReader AsDataReader<T>(this IEnumerable<T> source)
        {
            return new EnumerableDataReader<T>(source);
        }

    }


#pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
#pragma warning disable CS8767 // パラメーターの型における参照型の NULL 値の許容が、暗黙的に実装されるメンバーと一致しません。おそらく、NULL 値の許容の属性が原因です。

    /// <summary>
    /// IDataReader でラップした IEnumerable を返す (BulkCopyで使用可能)
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    class EnumerableDataReader<TSource> : IDataReader, IDataRecord
    {
        protected readonly IEnumerator<TSource> _iterator;
        protected readonly Dictionary<string, int> _dictNameToIndex;
        protected readonly List<PropertyInfo> _propList = new List<PropertyInfo>();

        public EnumerableDataReader(IEnumerable<TSource> source)
        {
            _iterator = source.GetEnumerator();
            var props = typeof(TSource).GetProperties();
            _dictNameToIndex = props.Select((p, i) => new { p, i }).ToDictionary(x => x.p.Name, x => x.i);
            _propList.AddRange(typeof(TSource).GetProperties());
        }

        public virtual bool Read() { return _iterator.MoveNext(); }
        public virtual object this[string name] { get { return GetValue(GetOrdinal(name)); } }
        public virtual object this[int i] { get { return GetValue(i); } }
        public virtual string GetName(int i) { return _propList[i].Name; }
        public virtual object GetValue(int i) { return _propList[i].GetValue(_iterator.Current); }
        public virtual int FieldCount { get { return _propList.Count; } }
        public virtual int GetOrdinal(string name) { return _dictNameToIndex[name]; }
        public virtual void Close() { _iterator.Dispose(); }
        public virtual void Dispose() { Close(); GC.SuppressFinalize(this); }

        public virtual int Depth { get { throw new NotImplementedException(); } }
        public virtual bool IsClosed { get { throw new NotImplementedException(); } }
        public virtual int RecordsAffected { get { throw new NotImplementedException(); } }
        public virtual DataTable GetSchemaTable() { throw new NotImplementedException(); }
        public virtual bool NextResult() { throw new NotImplementedException(); }
        public virtual string GetDataTypeName(int i) { throw new NotImplementedException(); }
        public virtual bool GetBoolean(int i) { throw new NotImplementedException(); }
        public virtual byte GetByte(int i) { throw new NotImplementedException(); }
        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }
        public virtual char GetChar(int i) { throw new NotImplementedException(); }
        public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }
        public virtual IDataReader GetData(int i) { throw new NotImplementedException(); }
        public virtual DateTime GetDateTime(int i) { throw new NotImplementedException(); }
        public virtual decimal GetDecimal(int i) { throw new NotImplementedException(); }
        public virtual double GetDouble(int i) { throw new NotImplementedException(); }
        public virtual Type GetFieldType(int i) { throw new NotImplementedException(); }
        public virtual float GetFloat(int i) { throw new NotImplementedException(); }
        public virtual Guid GetGuid(int i) { throw new NotImplementedException(); }
        public virtual short GetInt16(int i) { throw new NotImplementedException(); }
        public virtual int GetInt32(int i) { throw new NotImplementedException(); }
        public virtual long GetInt64(int i) { throw new NotImplementedException(); }
        public virtual string GetString(int i) { throw new NotImplementedException(); }
        public virtual int GetValues(object[] values) { throw new NotImplementedException(); }
        public virtual bool IsDBNull(int i) { throw new NotImplementedException(); }
    }

#pragma warning restore CS8603 // Null 参照戻り値である可能性があります。
#pragma warning restore CS8767 // パラメーターの型における参照型の NULL 値の許容が、暗黙的に実装されるメンバーと一致しません。おそらく、NULL 値の許容の属性が原因です。


    ///// <summary>
    ///// BulkCopyに使用できるIDataReaderを作成する
    ///// </summary>
    //public class DictionaryDataReader : IDataReader, IDataRecord
    //{
    //    protected readonly IEnumerator<IDictionary<string, object>> _iterator;
    //    protected readonly IDictionary<string, int> _dictNameToIndex;
    //    protected readonly IDictionary<int, string> _dictIndexToName;
    //
    //    public DictionaryDataReader(IEnumerable<IDictionary<string, object>> source)
    //    {
    //        _iterator = source.GetEnumerator();
    //        _dictNameToIndex = source.First().Keys.Select((k, i) => new { k, i }).ToDictionary(x => x.k, x => x.i);
    //        _dictIndexToName = _dictNameToIndex.ToDictionary(kv => kv.Value, kv => kv.Key);
    //    }
    //
    //    public bool Read() { return _iterator.MoveNext(); }
    //    public virtual object this[string name] { get { return GetValue(GetOrdinal(name)); } }
    //    public virtual object this[int i] { get { return GetValue(i); } }
    //    public virtual string GetName(int i) { return _dictIndexToName[i]; }
    //    public virtual object GetValue(int i) { return _iterator.Current.TryGetValue(_dictIndexToName[i], out object val) ? val : new object(); }
    //    public virtual int FieldCount { get { return _dictIndexToName.Count; } }
    //    public virtual int GetOrdinal(string name) { return _dictNameToIndex[name]; }
    //    public virtual void Close() { _iterator.Dispose(); }
    //    public virtual void Dispose() { Close(); GC.SuppressFinalize(this); }
    //
    //    public virtual int Depth { get { throw new NotImplementedException(); } }
    //    public virtual bool IsClosed { get { throw new NotImplementedException(); } }
    //    public virtual int RecordsAffected { get { throw new NotImplementedException(); } }
    //    public virtual DataTable GetSchemaTable() { throw new NotImplementedException(); }
    //    public virtual bool NextResult() { throw new NotImplementedException(); }
    //    public virtual string GetDataTypeName(int i) { throw new NotImplementedException(); }
    //    public virtual bool GetBoolean(int i) { throw new NotImplementedException(); }
    //    public virtual byte GetByte(int i) { throw new NotImplementedException(); }
    //    public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }
    //    public virtual char GetChar(int i) { throw new NotImplementedException(); }
    //    public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }
    //    public virtual IDataReader GetData(int i) { throw new NotImplementedException(); }
    //    public virtual DateTime GetDateTime(int i) { throw new NotImplementedException(); }
    //    public virtual decimal GetDecimal(int i) { throw new NotImplementedException(); }
    //    public virtual double GetDouble(int i) { throw new NotImplementedException(); }
    //    public virtual Type GetFieldType(int i) { throw new NotImplementedException(); }
    //    public virtual float GetFloat(int i) { throw new NotImplementedException(); }
    //    public virtual Guid GetGuid(int i) { throw new NotImplementedException(); }
    //    public virtual short GetInt16(int i) { throw new NotImplementedException(); }
    //    public virtual int GetInt32(int i) { throw new NotImplementedException(); }
    //    public virtual long GetInt64(int i) { throw new NotImplementedException(); }
    //    public virtual string GetString(int i) { throw new NotImplementedException(); }
    //    public virtual int GetValues(object[] values) { throw new NotImplementedException(); }
    //    public virtual bool IsDBNull(int i) { throw new NotImplementedException(); }
    //}
}