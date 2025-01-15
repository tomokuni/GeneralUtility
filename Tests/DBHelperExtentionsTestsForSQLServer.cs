using ChainingAssertion;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#if NET9_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
    using System.Data.SqlClient;
#endif

using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using System.Data.SQLite;

#pragma warning disable IDE0079 // �s�v�ȗ}�����폜���܂�
#pragma warning disable IDE0028 // �R���N�V�����̏��������ȑf���ł��܂�
#pragma warning disable IDE0063 // 'using' �X�e�[�g�����g�͊ȑf���ł��܂�
#pragma warning disable IDE0290 // �v���C�}�� �R���X�g���N�^�[�̎g�p

namespace GeneralUtility
{
    // ���� UnitTest �����{����ꍇ�́A�P�̃e�X�g�v���W�F�N�g�� appsettings.json ��
    // {
    //   "Connection": {
    //     "SQLServer": "server=127.0.0.1;user id=sa;pwd=sqlpass123!;database=utestdb;"
    //   }
    // }
    // �̂悤�ɋL�q���A�e�[�u���̍쐬/�폜��ǉ�/�X�V���\��DB��p�ӂ��邱�ƁB


    [TestClass]
    public class DBHelperExtentionsTestsForSQLServer
    {
        private static readonly IConfiguration appsettings = (new ConfigurationBuilder())
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        private static readonly string _connString = appsettings.GetSection("connection")["sqlserver"] ?? "";


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLServer)")]
        public void Open()
        {
            using (var db = new SqlConnection())
            {
                // TEST || DBHelperExtentions.OpenWithRetry() || (SQLServer) ���g���C��ɐڑ��ł��邱�ƁB
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(0, 0);
                db.State.Is(ConnectionState.Open);
                db.Close();

                // TEST || DBHelperExtentions.OpenWithRetry() || (SQLServer) ���g���C��ɐڑ��ł��邱�ƁB
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(3, 50);
                db.State.Is(ConnectionState.Open);
                db.Close();

                // TEST || DBHelperExtentions.OpenWithRetry() || (SQLServer) ���g���C��ɐڑ��ł��邱�ƁB
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(3, (i) => i * 30);
                db.State.Is(ConnectionState.Open);
                db.Close();
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLServer)")]
        public void Open_RetryOut()
        {
            // TEST || DBHelperExtentions.OpenWithRetry() || (SQLServer) �ڑ��Ɏ��s�����ꍇ�A���g���C�A�E�g���ė�O�𔭐����邱�ƁB
            var connStringInvalid = "Server=localhost;User ID=sa;Pwd=y0925&t;Database=UTestDummy;Connect Timeout=1;";
            using (var db = new SqlConnection())
            {
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = connStringInvalid;
                var ex = Assert.ThrowsException<SqlException>(() =>
                {
                    db.OpenWithRetry(1, 100);
                });
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLServer)")]
        public void Transaction()
        {
            using (var db = new SqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("IF OBJECT_ID(N'Test', N'U') IS NOT NULL DROP TABLE [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");

                // TEST || DBHelperExtentions.Query() || (SQLServer) BeginTransaction���Ă��Ȃ��ꍇ�́A�����R�~�b�g����邱�ƁB
                db.Execute("INSERT INTO [Test] VALUES (1)");
                var actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (SQLServer) Rollback()�Ń��[���o�b�N����邱�ƁB
                var trx = db.BeginTransaction();
                db.Execute("INSERT INTO [Test] VALUES (2)", transaction: trx);
                trx.Rollback();
                actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (SQLServer) Commit()�ŃR�~�b�g����邱�ƁB
                var trx2 = db.BeginTransaction();
                db.Execute("INSERT INTO [Test] VALUES (3)", transaction: trx2);
                trx2.Commit();
                actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1, 3 }).IsTrue();

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLServer)")]
        public void QueryToDataTable()
        {
            using (var db = new SqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("IF OBJECT_ID(N'Test', N'U') IS NOT NULL DROP TABLE [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");
                db.Execute("INSERT INTO [Test] VALUES (3), (4)");

                // TEST || DBHelperExtentions.QueryToDataTable() || (SQLServer) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
                var actual = db.QueryToDataTable(typeof(SqlDataAdapter), "SELECT * FROM [Test] ORDER BY ID");
                actual.Rows.Count.Is(2);
                actual.Columns.Count.Is(1);
                actual.Columns[0].ColumnName.Is("ID");
                ((int)actual.Rows[0]["ID"]).Is(3);
                ((int)actual.Rows[1]["ID"]).Is(4);

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLServer)")]
        public void Query()
        {
            using (var db = new SqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("IF OBJECT_ID(N'Test', N'U') IS NOT NULL DROP TABLE [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");
                db.Execute("INSERT INTO [Test] VALUES (4), (5)");

                // TEST || DBHelperExtentions.Query() || (SQLServer) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
                var actual = db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 4, 5 }).IsTrue();

                // TEST || DBHelperExtentions.Query().ToEntity() || (SQLServer) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
                var sampleDic = new Dictionary<string, object>() { { "id", 5 }, { "name", "n5" } };
                actual = db.Query("SELECT * FROM [Test] WHERE ID=@id", sampleDic).ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 5 }).IsTrue();

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLServer)")]
        public void QueryScalar()
        {
            using (var db = new SqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("IF OBJECT_ID(N'Test', N'U') IS NOT NULL DROP TABLE [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");
                db.Execute("INSERT INTO [Test] VALUES (5), (6)");

                // TEST || DBHelperExtentions.QueryScalar() || (SQLServer) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
                var actual = db.QueryScalar<int>("SELECT TOP (1) * FROM [Test] ORDER BY ID");
                actual.Is(5);

                // TEST || DBHelperExtentions.QueryScalar() || (SQLServer) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
                var sampleDic = new Dictionary<string, object>() { { "id", 6 }, { "name", "n6" } };
                actual = db.QueryScalar<int>("SELECT * FROM [Test] WHERE ID=@id", sampleDic);
                actual.Is(6);

                db.Execute("DROP TABLE [Test]");
            }
        }


        [TestMethod, TestCategory("GeneralUtility. DBHelperExtentions (SQLServer)")]
        public void Execute()
        {
            using (var db = new SqlConnection())
            {
                db.ConnectionString = _connString;
                db.Open();

                db.Execute("IF OBJECT_ID(N'Test', N'U') IS NOT NULL DROP TABLE [Test]");
                db.Execute("CREATE TABLE [Test] (ID int)");

                // TEST || DBHelperExtentions.Execute() || (SQLServer) DB��SQL�����s����ď�Ԃ��ς�邱�ƁB
                db.Execute("INSERT INTO [Test] VALUES (5), (6)");

                // TEST || DBHelperExtentions.Execute() || (SQLServer) DB��SQL�����s����ď�Ԃ��ς�邱�ƁB
                var sampleDic = new Dictionary<string, object>() { { "id", 7 }, { "name", "n7" } };
                db.Execute("INSERT INTO test (ID) VALUES ( @id )", sampleDic);

                var actual = db.Query("SELECT * FROM [Test]").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 5, 6, 7 }).IsTrue();

                db.Execute("DROP TABLE [Test]");
            }
        }


        //[TestMethod, TestCategory("GeneralUtility/DBHelperSQLServer")]
        //public void RunScript()
        //{
        //    var script = "IF OBJECT_ID (N'[dbo].[Test]', N'U') IS NOT NULL DROP TABLE [Test] \n CREATE TABLE [dbo].[Test]([CRTDT] [datetime] NOT NULL) \n GO";
        //
        //    // TEST || GeneralUtility/DBHelperSQLServer.cs || DBHelperSQLServer.RunScript() || �X�N���v�g�����s���邱�ƁB
        //    _db.RunScript(script);
        //    _db.QueryScalar<int>("IF OBJECT_ID (N'[dbo].[Test]', N'U') IS NOT NULL SELECT 1; ELSE SELECT 0").Is(1);
        //
        //    _db.Execute("DROP TABLE [Test]");
        //}


        //[TestMethod, TestCategory("GeneralUtility/DBHelperSQLServer")]
        //public void BulkInsert()
        //{
        //    var list = new List<ID>() { new ID(1), new ID(2), new ID(3), new ID(4), new ID(5) };
        //
        //    _db.Execute("IF OBJECT_ID(N'Test', N'U') IS NOT NULL DROP TABLE [Test]");
        //    _db.Execute("CREATE TABLE [Test] (ID int)");
        //
        //    // TEST || GeneralUtility\DBHelperSQLServer.cs || DBHelperSQLServer.BulkInsert() || DB��SQL�����s����Ēl���i�[����邱�ƁB
        //    var columnMappings = new Dictionary<string, string>() { { "Id", "ID" } };
        //    _db.BulkInsert("Test", list, columnMappings);
        //    var actual = _db.Query("SELECT * FROM [Test] ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
        //    Assert.IsTrue((new List<int> { 1, 2, 3, 4, 5 }).SequenceEqual(actual));
        //
        //    _db.Execute("DROP TABLE [Test]");
        //}

        class ID
        {
            public int Id { get; set; }

            public ID(int id)
            {
                Id = id;
            }
        }
    }
}
