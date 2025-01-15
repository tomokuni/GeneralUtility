using ChainingAssertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Org.BouncyCastle.Asn1.BC;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;

#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable IDE0090 // 'new(...)' を使用する
#pragma warning disable IDE0290 // プライマリ コンストラクターの使用
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。


namespace GeneralUtility
{
    /// <summary>
    /// テスト用クラス
    /// </summary>
    public class DBHelperDummy
    {
        /// <summary>デフォルトの接続リトライ回数</summary>
        protected const int DEFAULT_RETRY_NUM = 3;

        /// <summary>デフォルトの接続リトライ間隔</summary>
        protected const int DEFAULT_RETRY_INTERVAL_MSEC = 100;

        /// <summary>SQLコネクション・オブジェクト</summary>
#if NET5_0_OR_GREATER
        protected DbConnection? _conn;
#else
        protected DbConnection _conn;
#endif


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DBHelperDummy(DbConnection connection)
        {
            _conn = connection;
        }


        /// <summary>
        /// DBに接続する
        /// </summary>
        /// <param name="connString">DB接続情報</param>
        /// <param name="retryNum">(省略可) リトライ回数</param>
        /// <param name="retryIntervalMSec">(省略可) リトライ間隔(ミリ秒)</param>
        public virtual void Open(
            string connString,
            int retryNum = DEFAULT_RETRY_NUM,
            int retryIntervalMSec = DEFAULT_RETRY_INTERVAL_MSEC)
        {
            _conn.ConnectionString = connString;
            _conn.OpenWithRetry(retryNum, retryIntervalMSec);
        }


        /// <summary>
        /// DBに接続する
        /// </summary>
        /// <param name="connString">DB接続情報</param>
        /// <param name="retryNum">リトライ回数</param>
        /// <param name="getRetryIntervalMSec">リトライx回目を元にリトライ間隔(ミリ秒)を返す関数</param>
        public virtual void Open(
            string connString,
            int retryNum,
            Func<int, int> getRetryIntervalMSec)
        {
            _conn.ConnectionString = connString;
            _conn.OpenWithRetry(retryNum, getRetryIntervalMSec);
        }

    }


    [TestClass]
    public class DBHelperExtentionsTests
    {
        private readonly Mock<DbConnection> _mockConn = new Mock<DbConnection>();
        private readonly Mock<Func<DbCommand, DbDataAdapter>> _mockDataAdapterFactory = new Mock<Func<DbCommand, DbDataAdapter>>();
        private readonly Mock<Func<KeyValuePair<string, object>, DbParameter>> _mockParameterFactory = new Mock<Func<KeyValuePair<string, object>, DbParameter>>();

        private readonly DBHelperDummy _db;

        public DBHelperExtentionsTests()
        {
            _db = new DBHelperDummy(_mockConn.Object);
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (Mock)")]
        public void Open()
        {
            // TEST || DBHelperExtentions.OpenWithRetry() || (Mock) リトライ後に接続できること。
            _mockConn.SetupSequence(m => m.Open()).Throws<Exception>().Throws<Exception>().Throws<Exception>().Pass();
            _db.Open("ConnString", 3, 10);

            // TEST || DBHelperExtentions.OpenWithRetry() || (Mock) リトライアウトで例外発生すること。
            _mockConn.SetupSequence(m => m.Open()).Throws<Exception>().Throws<Exception>().Throws<Exception>().Pass();
            Assert.ThrowsException<Exception>(() =>
            {
                _db.Open("ConnString", 2, 10);
            });

            // TEST || DBHelperExtentions.OpenWithRetry() || (Mock) リトライインターバルを決める引数が正しく渡されること
            _mockConn.SetupSequence(m => m.Open()).Throws<Exception>().Throws<Exception>().Throws<Exception>().Pass();
            var expected = 0;
            _db.Open("ConnString", 3, (t) =>
            {
                Assert.AreEqual(++expected, t);
                Assert.IsTrue(3 >= t);
                return t * 10;
            });
        }


        //const string script1 = "a234567890b234567890c234567890d234567890e234567890\nf234567890g234567890h234567890h234567890i23456789";
        //const string script2 = "a234567890b234567890c234567890d234567890e234567890\nf234567890g234567890h234567890h234567890i234567890";
        //private static IEnumerable<object[]> RunSqlScript_Exception_TestCase => new List<object[]>()
        //{
        //    new object[] { "name1", script1, string.Format("Not Implemented.\nScript: \n{0}", script1)},
        //    new object[] { "name2", script2, string.Format("Not Implemented.\nScript: \n{0}", script2.Substring(0, 100) + " ...\n...")},
        //};

        //[DataTestMethod, TestCategory("GeneralUtility/DBHelperBase")]
        //[DynamicData(nameof(RunSqlScript_Exception_TestCase))]
        //public void RunScript_NotImplementedException(string name, string script, string expected)
        //{
        //    // TEST || GeneralUtility/DBHelper.cs || DBHelper.RunScript() || 未実装例外が発生すること。(スクリプトが短い場合は全文記載)
        //    // TEST || GeneralUtility/DBHelper.cs || DBHelper.RunScript() || 未実装例外が発生すること。(スクリプトが長い場合は後続省略)
        //    Assert.ThrowsException<NotImplementedException>(() =>
        //    {
        //        _db.RunScript(script);
        //    }, expected);
        //}


        //[DataTestMethod, TestCategory("GeneralUtility/DBHelperBase")]
        //[DynamicData(nameof(RunSqlScript_Exception_TestCase))]
        //public void RunScriptFile_NotImplementedException(string name, string script, string expected)
        //{
        //    var pathName = $@".\temp";
        //    var fileName = $@"{pathName}\{name}";
        //    if (Directory.Exists(pathName) == false)
        //        Directory.CreateDirectory(pathName);
        //    File.WriteAllText(fileName, script);
        //
        //    // TEST || GeneralUtility/DBHelper.cs || DBHelper.RunScriptFile() || 未実装例外が発生すること。
        //    Assert.ThrowsException<NotImplementedException>(() =>
        //    {
        //        _db.RunScriptFile(fileName);
        //    }, expected);
        //}


        //[TestMethod, TestCategory("GeneralUtility/DBHelperBase")]
        //public void BulkInsert_NotImplementedException()
        //{
        //    // TEST || GeneralUtility/DBHelper.cs || DBHelper.BulkInsert() || 未実装例外が発生すること。(マッピング指定あり)
        //    Assert.ThrowsException<NotImplementedException>(() =>
        //    {
        //        _db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } }, new Dictionary<string, string> { { "ID", "ID" }, { "NAME", "NAME" } });
        //    });
        //
        //    // TEST || GeneralUtility/DBHelper.cs || DBHelper.BulkInsert() || 未実装例外が発生すること。(マッピング指定省略)
        //    Assert.ThrowsException<NotImplementedException>(() =>
        //    {
        //        _db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } });
        //    });
        //}


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions")]
        public void ToExpando()
        {
            var inData = new[]
            {
                new { Id = 1, Name = "TEST1" },
                new { Id = 2, Name = "TEST2" },
            };

            // TEST || DBHelperExtentions.ToExpando() || IDataRederオブジェクトからIEnumerable<dynamic>へのラッパーが動作すること。
            var actual = inData.AsDataReader().ToExpando().ToList();
            actual.Count.Is(2);
            ((int)actual[0].Id).Is(1);
            ((string)actual[0].Name).Is("TEST1");
            ((int)actual[1].Id).Is(2);
            ((string)actual[1].Name).Is("TEST2");
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions")]
        public void ToDictionary()
        {
            var expect = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>(){{ "ID", 1},  {"NAME", "TEST1" } },
                new Dictionary<string, object>(){{ "ID", 2},  {"NAME", "TEST2" } },
            };

            var inData = new[]
            {
                new { ID = 1, NAME = "TEST1" },
                new { ID = 2, NAME = "TEST2" },
            };

            // TEST || DBHelperExtentions.ToDictionary() || IDataRederオブジェクトからIEnumerable<IDictionary>へのラッパーが動作すること。
            var actual = inData.AsDataReader().ToDictionary().ToList();
            actual.Count.Is(expect.Count);
            actual[0].Is(expect[0]);
            actual[1].Is(expect[1]);
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions")]
        public void ToEntity()
        {
            var expect = new List<ID>
            {
                new ID(1, "TEST1"),
                new ID(2, "TEST2"),
            };

            var inData = new[]
            {
                new { Id = 1, Name = "TEST1" },
                new { Id = 2, Name = "TEST2" },
            };

            // TEST || DBHelperExtentions.ToEntity() || IDataRederオブジェクトからIEnumerable<T>へのラッパーが動作すること。
            var actual = inData.AsDataReader().ToEntity(s => new ID((int)s["Id"], (string)s["Name"])).ToList();
            actual.Count.Is(expect.Count);
            actual[0].GetType().Is(typeof(ID));
            actual[0].IsStructuralEqual(expect[0]);
            actual[1].IsStructuralEqual(expect[1]);
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions")]
        public void AsDataReader()
        {
            var expect = new[] {
                new { ID = 1, NAME = "TEST1" },
                new { ID = 2, NAME = "TEST2" },
            };

            // TEST || DBHelperExtentions.AsDataReader() || IEnumerableオブジェクトからIDataRederへのラッパーが動作すること。
            var dr = expect.AsDataReader();
            //
            dr.Read().IsTrue();
            dr.FieldCount.Is(2);
            dr.GetOrdinal("ID").Is(0);
            dr.GetValue(0).Is(1);
            dr.GetOrdinal("NAME").Is(1);
            dr.GetValue(1).Is("TEST1");
            // 
            dr.Read().IsTrue();
            dr.FieldCount.Is(2);
            dr.GetOrdinal("ID").Is(0);
            dr.GetValue(0).Is(2);
            dr.GetOrdinal("NAME").Is(1);
            dr.GetValue(1).Is("TEST2");
            //
            dr.Read().IsFalse();

            dr = expect.AsDataReader();
            dr.Read().IsTrue();
            var record = (IDataRecord)dr;
            ((int)record["ID"]).Is(1);
            ((int)record[0]).Is(1);
            ((string)record["NAME"]).Is("TEST1");
            ((string)record[1]).Is("TEST1");
            record.GetName(0).Is("ID");
            record.GetName(1).Is("NAME");
        }


        private class ID
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ID(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

    }
}
