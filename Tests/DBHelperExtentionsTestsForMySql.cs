using ChainingAssertion;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using MySql.Data.MySqlClient;

using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable IDE0028 // コレクションの初期化を簡略化します
#pragma warning disable IDE0063 // 単純な 'using' ステートメントを使用する


namespace GeneralUtility
{
    // この UnitTest を実施する場合は、単体テストプロジェクトの appsettings.json に
    // {
    //   "Connection": {
    //     "SQLServer": "server=127.0.0.1;port=3306;user=root;pwd=rootpass;database=utestdb;"
    //   }
    // }
    // のように記述し、テーブルの作成/削除や追加/更新が可能なDBを用意すること。


    [TestClass]
    public class DBHelperExtentionsTestsForMySql
    {
        private static readonly IConfiguration appsettings = (new ConfigurationBuilder())
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        private static readonly string _connString = appsettings.GetSection("connection")["mysql"] ?? "";


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (MySQL)")]
        [DoNotParallelize()]
        public void Open()
        {
            using (var db = new MySqlConnection())
            {
                // TEST || DBHelperExtentions.OpenWithRetry() || (MySQL) リトライ後に接続できること。
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(0, 0);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);

                // TEST || DBHelperExtentions.OpenWithRetry() || (MySQL) リトライ後に接続できること。
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(3, 50);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);

                // TEST || DBHelperExtentions.OpenWithRetry() || (MySQL) リトライ後に接続できること。
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(3, (i) => i * 30);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (MySQL)")]
        [DoNotParallelize()]
        public void Open_RetryOut()
        {
            // TEST || DBHelperExtentions.Open() || (MySQL) 接続に失敗した場合、リトライアウトして例外を発生すること。
            var connStringInvalid = "server=127.0.0.1;port=3306;user id=root;database=utestdummy;Connect Timeout=1;";
            using (var db = new MySqlConnection())
            {
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = connStringInvalid;
                Assert.ThrowsException<MySqlException>(() =>
                {
                    db.OpenWithRetry(1, 100);
                });
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (MySQL)")]
        [DoNotParallelize()]
        public void Transaction()
        {
            using (var db = new MySqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS test; CREATE TABLE IF NOT EXISTS test (ID int)");

                // TEST || DBHelperExtentions.Query() || (MySQL) BeginTransaction()していない場合は、自動コミットされること。
                db.Execute("INSERT INTO test VALUES (1)");
                var actual = db.Query("SELECT * FROM test ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (MySQL) Rollback()でロールバックされること。
                var trx = db.BeginTransaction();
                db.Execute("INSERT INTO test VALUES (2)", transaction: trx);
                trx.Rollback();
                actual = db.Query("SELECT * FROM test ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (MySQL) Commit()でコミットされること。
                var trx2 = db.BeginTransaction();
                db.Execute("INSERT INTO test VALUES (3)", transaction: trx2);
                trx2.Commit();
                actual = db.Query("SELECT * FROM test ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1, 3 }).IsTrue();

                db.Execute("DROP TABLE test");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (MySQL)")]
        [DoNotParallelize()]
        public void QueryToDataTable()
        {
            using (var db = new MySqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS test; CREATE TABLE IF NOT EXISTS test (ID int)");
                db.Execute("INSERT INTO test VALUES (3), (4)");

                // TEST || DBHelperExtentions.QueryToDataTable() || (MySQL) DBにSQLが発行されて値が取得できること。
                var actual = db.QueryToDataTable(typeof(MySqlDataAdapter), "SELECT * FROM test ORDER BY ID");
                actual.Rows.Count.Is(2);
                actual.Columns.Count.Is(1);
                actual.Columns[0].ColumnName.Is("ID");
                ((int)actual.Rows[0]["ID"]).Is(3);
                ((int)actual.Rows[1]["ID"]).Is(4);

                db.Execute("DROP TABLE test");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (MySQL)")]
        [DoNotParallelize()]
        public void Query()
        {
            using (var db = new MySqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS test; CREATE TABLE IF NOT EXISTS test (ID int)");
                db.Execute("INSERT INTO test VALUES (4), (5)");

                // TEST || DBHelperExtentions.Query() || (MySQL) DBにSQLが発行されて値が取得できること。
                var actual = db.Query("SELECT * FROM test ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 4, 5 }).IsTrue();

                // TEST || DBHelperExtentions.Query().ToEntity() || (SQLServer) DBにSQLが発行されて値が取得できること。
                var sampleDic = new Dictionary<string, object>() { { "id", 5 }, { "name", "n5" } };
                actual = db.Query("SELECT * FROM test WHERE ID=@id", sampleDic).ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 5 }).IsTrue();

                db.Execute("DROP TABLE test");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (MySQL)")]
        [DoNotParallelize()]
        public void QueryScalar()
        {
            using (var db = new MySqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS test; CREATE TABLE IF NOT EXISTS test (ID int)");
                db.Execute("INSERT INTO test VALUES (5), (6)");

                // TEST || DBHelperExtentions.QueryScalar() || (MySQL) DBにSQLが発行されて値が取得できること。
                var actual = db.QueryScalar<int>("SELECT * FROM test ORDER BY ID LIMIT 1");
                actual.Is(5);

                // TEST || DBHelperExtentions.QueryScalar() || (MySQL) DBにSQLが発行されて値が取得できること。
                var sampleDic = new Dictionary<string, object>() { { "id", 6 }, { "name", "n6" } };
                actual = db.QueryScalar<int>("SELECT * FROM test WHERE ID=@id", sampleDic);
                actual.Is(6);

                db.Execute("DROP TABLE test");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (MySQL)")]
        [DoNotParallelize()]
        public void Execute()
        {
            using (var db = new MySqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS test; CREATE TABLE IF NOT EXISTS test (ID int)");

                // TEST || DBHelperExtentions.Execute() || (MySQL) DBにSQLが発行されて状態が変わること。
                db.Execute("INSERT INTO test VALUES (5), (6)");

                // TEST || DBHelperExtentions.Execute() || (MySQL) DBにSQLが発行されて状態が変わること。
                var sampleDic = new Dictionary<string, object>() { { "id", 7 }, { "name", "n7" } };
                db.Execute("INSERT INTO test (ID) VALUES ( @id )", sampleDic);

                var actual = db.Query("SELECT * FROM test").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 5, 6, 7 }).IsTrue();

                db.Execute("DROP TABLE test");
            }
        }


        //[TestMethod, TestCategory("GeneralUtility/DBHelperMySql")]
        //[DoNotParallelize()]
        //public void RunScript()
        //{
        //    using (var db = new MySqlConnection())
        //    {
        //        var script = "DROP TABLE IF EXISTS test; CREATE TABLE IF NOT EXISTS test (ID int); INSERT INTO test VALUES (5), (6)";
        //
        //        // TEST || GeneralUtility/DBHelperMySql.cs || DBHelperMySql.RunScript() || スクリプトを実行すること。
        //        db.RunScript(script);
        //        var actual = db.QueryScalar<int>("SELECT * FROM test ORDER BY ID LIMIT 1");
        //        actual.Is(5);
        //
        //        db.Execute("DROP TABLE test");
        //    }
        //}


        //[TestMethod, TestCategory("GeneralUtility/DBHelperMySql")]
        //[DoNotParallelize()]
        //public void BulkInsert()
        //{
        //    using (var db = new MySqlConnection())
        //    {
        //        // TEST || GeneralUtility/DBHelperMySql.cs || DBHelperMySql.BulkInsert() || 未実装例外が発生すること。(マッピング指定あり)
        //        Assert.ThrowsException<NotImplementedException>(() =>
        //        {
        //            db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } }, new Dictionary<string, string> { { "ID", "ID" }, { "NAME", "NAME" } });
        //        });
        //
        //        // TEST || GeneralUtility/DBHelperMySql.cs || DBHelperMySql.BulkInsert() || 未実装例外が発生すること。(マッピング指定省略)
        //        Assert.ThrowsException<NotImplementedException>(() =>
        //        {
        //            db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } });
        //        });
        //    }
        //}

    }
}