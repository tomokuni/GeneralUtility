using ChainingAssertion;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using System.Data.SQLite;

using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using MySql.Data.MySqlClient;

#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable IDE0028 // コレクションの初期化を簡略化します
#pragma warning disable IDE0063 // 単純な 'using' ステートメントを使用する
#pragma warning disable IDE0290 // プライマリ コンストラクターの使用


namespace GeneralUtility
{
    // この UnitTest を実施する場合は、単体テストプロジェクトの appsettings.json に
    // {
    //   "Connection": {
    //     "SQLServer": "data source=:memory:;"
    //   }
    // }
    // のように記述し、テーブルの作成/削除や追加/更新が可能なDBを用意すること。


    [TestClass]
    public class DBHelperExtentionsTestsForSQLite
    {
        private static readonly IConfiguration appsettings = (new ConfigurationBuilder())
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        private static readonly string _connString = appsettings.GetSection("connection")["sqlite"] ?? "";


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLite)")]
        public void Open()
        {
            using (var db = new SQLiteConnection())
            {
                // TEST || DBHelperExtentions.OpenWithRetry() || (SQLite) リトライ後に接続できること。
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(0, 0);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);

                // TEST || DBHelperExtentions.OpenWithRetry() || (SQLite) リトライ後に接続できること。
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(3, 50);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);

                // TEST || DBHelperExtentions.OpenWithRetry() || (SQLite) リトライ後に接続できること。
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(3, (i) => i * 30);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLite)")]
        public void Open_RetryOut()
        {
            // TEST || DBHelperExtentions.OpenWithRetry() || (SQLite) 接続に失敗した場合、リトライアウトして例外を発生すること。
            var connStringInvalid = "Data Source=:memoryInvalid:;";
            using (var db = new SQLiteConnection())
            {
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = connStringInvalid;
#if NET5_0_OR_GREATER
                var ex = Assert.ThrowsException<SQLiteException>(() =>
#else
                var ex = Assert.ThrowsException<NotSupportedException>(() =>
#endif
                {
                    db.OpenWithRetry(1, 100);
                });
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLite)")]
        public void Transaction()
        {
            using (var db = new SQLiteConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");

                // TEST || DBHelperExtentions.Query() || (SQLite) BeginTransaction()していない場合は、自動コミットされること。
                db.Execute("INSERT INTO [Test] VALUES (1)");
                var actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (SQLite) Rollback(Transaction)でロールバックされること。
                var trx = db.BeginTransaction();
                db.Execute("INSERT INTO [Test] VALUES (2)", transaction: trx);
                trx.Rollback();
                actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (SQLite) Commit(Transaction)でコミットされること。
                var trx2 = db.BeginTransaction();
                db.Execute("INSERT INTO [Test] VALUES (3)", transaction: trx2);
                trx2.Commit();
                actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1, 3 }).IsTrue();

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLite)")]
        public void QueryToDataTable()
        {
            using (var db = new SQLiteConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");
                db.Execute("INSERT INTO [Test] VALUES (3), (4)");

                // TEST || DBHelperExtentions.QueryToDataTable() || (SQLite) DBにSQLが発行されて値が取得できること。
                var actual = db.QueryToDataTable(typeof(SQLiteDataAdapter), "SELECT * FROM [Test] ORDER BY ID");
                actual.Rows.Count.Is(2);
                actual.Columns.Count.Is(1);
                actual.Columns[0].ColumnName.Is("ID");
                ((int)actual.Rows[0]["ID"]).Is(3);
                ((int)actual.Rows[1]["ID"]).Is(4);

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLite)")]
        public void Query()
        {
            using (var db = new SQLiteConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");
                db.Execute("INSERT INTO [Test] VALUES (4), (5)");

                // TEST || DBHelperExtentions.Query() || (SQLite) DBにSQLが発行されて値が取得できること。
                var actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 4, 5 }).IsTrue();

                // TEST || DBHelperExtentions.Query().ToEntity() || (SQLServer) DBにSQLが発行されて値が取得できること。
                var sampleDic = new Dictionary<string, object>() { { "id", 5 }, { "name", "n5" } };
                actual = db.Query("SELECT * FROM [Test] WHERE ID=@id", sampleDic).ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 5 }).IsTrue();

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLite)")]
        public void QueryScalar()
        {
            using (var db = new SQLiteConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");
                db.Execute("INSERT INTO [Test] VALUES (5), (6)");

                // TEST || DBHelperExtentions.QueryScalar() || (SQLite) DBにSQLが発行されて値が取得できること。
                var actual = db.QueryScalar<int>("SELECT * FROM [Test] ORDER BY ID LIMIT 1");
                actual.Is(5);

                // TEST || DBHelperExtentions.QueryScalar() || (SQLite) DBにSQLが発行されて値が取得できること。
                var sampleDic = new Dictionary<string, object>() { { "id", 6 }, { "name", "n6" } };
                actual = db.QueryScalar<int>("SELECT * FROM [Test] WHERE ID=@id", sampleDic);
                actual.Is(6);

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLite)")]
        public void Execute()
        {
            using (var db = new SQLiteConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("DROP TABLE IF EXISTS [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");

                // TEST || DBHelperExtentions.Execute() || (SQLite) DBにSQLが発行されて状態が変わること。
                db.Execute("INSERT INTO [Test] VALUES (5), (6)");

                // TEST || DBHelperExtentions.Execute() || (SQLite) DBにSQLが発行されて状態が変わること。
                var sampleDic = new Dictionary<string, object>() { { "id", 7 }, { "name", "n7" } };
                db.Execute("INSERT INTO test (ID) VALUES ( @id )", sampleDic);

                var actual = db.Query("SELECT * FROM [Test]").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 5, 6, 7 }).IsTrue();

                db.Execute("DROP TABLE [Test]");
            }
        }


        //[TestMethod, TestCategory("GeneralUtility/DBHelperSQLite")]
        //public void RunScript()
        //{
        //    var script = "DROP TABLE IF EXISTS [Test]; CREATE TABLE [Test] (ID int); INSERT INTO [Test] VALUES (5), (6)";
        //
        //    // TEST || GeneralUtility/DBHelperSQLite.cs || DBHelperSQLite.RunSqlScript() || スクリプトを実行すること。
        //    _db.RunScript(script);
        //    var actual = _db.QueryScalar<int>("SELECT * FROM [Test] ORDER BY ID LIMIT 1");
        //    actual.Is(5);
        //
        //    _db.Execute("DROP TABLE [Test]");
        //}


        //[TestMethod, TestCategory("GeneralUtility/DBHelperSQLite")]
        //public void BulkInsert()
        //{
        //    // TEST || GeneralUtility/DBHelperSQLite.cs || DBHelperSQLite.BulkInsert() || 未実装例外が発生すること。(マッピング指定あり)
        //    Assert.ThrowsException<NotImplementedException>(() =>
        //    {
        //        _db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } }, new Dictionary<string, string> { { "ID", "ID" }, { "NAME", "NAME" } });
        //    });
        //
        //    // TEST || GeneralUtility/DBHelperSQLite.cs || DBHelperSQLite.BulkInsert() || 未実装例外が発生すること。(マッピング指定省略)
        //    Assert.ThrowsException<NotImplementedException>(() =>
        //    {
        //        _db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } });
        //    });
        //}

        class ID
        {
            public int Id { get; set; }
            public ID(int id)
            {
                this.Id = id;
            }
        }
    }
}
