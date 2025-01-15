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

#pragma warning disable IDE0079 // �s�v�ȗ}�����폜���܂�
#pragma warning disable IDE0028 // �R���N�V�����̏��������ȗ������܂�
#pragma warning disable IDE0063 // �P���� 'using' �X�e�[�g�����g���g�p����


namespace GeneralUtility
{
    // ���� UnitTest �����{����ꍇ�́A�P�̃e�X�g�v���W�F�N�g�� appsettings.json ��
    // {
    //   "Connection": {
    //     "SQLServer": "server=127.0.0.1;port=3306;user=root;pwd=rootpass;database=utestdb;"
    //   }
    // }
    // �̂悤�ɋL�q���A�e�[�u���̍쐬/�폜��ǉ�/�X�V���\��DB��p�ӂ��邱�ƁB


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
                // TEST || DBHelperExtentions.OpenWithRetry() || (MySQL) ���g���C��ɐڑ��ł��邱�ƁB
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(0, 0);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);

                // TEST || DBHelperExtentions.OpenWithRetry() || (MySQL) ���g���C��ɐڑ��ł��邱�ƁB
                db.State.Is(ConnectionState.Closed);
                db.ConnectionString = _connString;
                db.OpenWithRetry(3, 50);
                db.State.Is(ConnectionState.Open);
                db.Close();
                db.State.Is(ConnectionState.Closed);

                // TEST || DBHelperExtentions.OpenWithRetry() || (MySQL) ���g���C��ɐڑ��ł��邱�ƁB
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
            // TEST || DBHelperExtentions.Open() || (MySQL) �ڑ��Ɏ��s�����ꍇ�A���g���C�A�E�g���ė�O�𔭐����邱�ƁB
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

                // TEST || DBHelperExtentions.Query() || (MySQL) BeginTransaction()���Ă��Ȃ��ꍇ�́A�����R�~�b�g����邱�ƁB
                db.Execute("INSERT INTO test VALUES (1)");
                var actual = db.Query("SELECT * FROM test ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (MySQL) Rollback()�Ń��[���o�b�N����邱�ƁB
                var trx = db.BeginTransaction();
                db.Execute("INSERT INTO test VALUES (2)", transaction: trx);
                trx.Rollback();
                actual = db.Query("SELECT * FROM test ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 1 }).IsTrue();

                // TEST || DBHelperExtentions.Query() || (MySQL) Commit()�ŃR�~�b�g����邱�ƁB
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

                // TEST || DBHelperExtentions.QueryToDataTable() || (MySQL) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
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

                // TEST || DBHelperExtentions.Query() || (MySQL) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
                var actual = db.Query("SELECT * FROM test ORDER BY ID").ToEntity(r => (int)r["ID"]).ToList();
                actual.SequenceEqual(new List<int> { 4, 5 }).IsTrue();

                // TEST || DBHelperExtentions.Query().ToEntity() || (SQLServer) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
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

                // TEST || DBHelperExtentions.QueryScalar() || (MySQL) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
                var actual = db.QueryScalar<int>("SELECT * FROM test ORDER BY ID LIMIT 1");
                actual.Is(5);

                // TEST || DBHelperExtentions.QueryScalar() || (MySQL) DB��SQL�����s����Ēl���擾�ł��邱�ƁB
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

                // TEST || DBHelperExtentions.Execute() || (MySQL) DB��SQL�����s����ď�Ԃ��ς�邱�ƁB
                db.Execute("INSERT INTO test VALUES (5), (6)");

                // TEST || DBHelperExtentions.Execute() || (MySQL) DB��SQL�����s����ď�Ԃ��ς�邱�ƁB
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
        //        // TEST || GeneralUtility/DBHelperMySql.cs || DBHelperMySql.RunScript() || �X�N���v�g�����s���邱�ƁB
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
        //        // TEST || GeneralUtility/DBHelperMySql.cs || DBHelperMySql.BulkInsert() || ��������O���������邱�ƁB(�}�b�s���O�w�肠��)
        //        Assert.ThrowsException<NotImplementedException>(() =>
        //        {
        //            db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } }, new Dictionary<string, string> { { "ID", "ID" }, { "NAME", "NAME" } });
        //        });
        //
        //        // TEST || GeneralUtility/DBHelperMySql.cs || DBHelperMySql.BulkInsert() || ��������O���������邱�ƁB(�}�b�s���O�w��ȗ�)
        //        Assert.ThrowsException<NotImplementedException>(() =>
        //        {
        //            db.BulkInsert("TestTable", new[] { new { ID = 5, NAME = "TEST" } });
        //        });
        //    }
        //}

    }
}