using NUnit.Framework;
using GeneralUtility;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace GeneralUtility.Tests
{
    public class ConnectionConfigHandlerTests
    {
        [SetUp]
        public void Setup()
        {
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void DefaultSection_Attribute()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.GetSection() || デフォルトのセクションの属性を取得する
            var section = ConnectionConfigSection.GetSection();
            Assert.AreEqual("9Fix4L4HB4PKeKWY", section.CryptKey);
            Assert.AreEqual("pf69DL6GrWFyZcMK", section.CryptIV);
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void DefaultSection_ElementCollection()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.GetAll() || デフォルトのセクションの全要素のキーの一致を確認する
            var section = ConnectionConfigSection.GetSection();
            Assert.IsNotNull(section);

            var list = section.GetAll();
            CollectionAssert.AreEqual(new[] { "Key", "Key2", "Key3" }, list.Select(s => s.Key));
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void DefaultSection_ElementValue1()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || デフォルトのセクションから特定キーの全属性を取得する
            var section = ConnectionConfigSection.GetSection();
            var config = section.Get("Key");
            EqualKeyHelper(config, key: "Key");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void DefaultSection_ElementValue2()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || デフォルトのセクションの全属性を取得する (キー以外は未設定)
            var section = ConnectionConfigSection.GetSection();
            var config = section.Get("Key2");
            EqualKeyHelper(config, key: "Key2", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void DefaultSection_ElementValue3()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || デフォルトのセクションの要素のデフォルトキーで暗号化された属性を取得する
            var section = ConnectionConfigSection.GetSection();
            var key = section.Get("Key3");
            Assert.AreEqual("Key3", key.Key);
            Assert.AreEqual("conn", key.Crypt);
            Assert.AreEqual("ConnString", key.ConnString);
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Attribute()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || カスタムセクション名の属性を取得する
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            Assert.AreEqual("pf69DL6GrWFyZcMK", section.CryptKey);
            Assert.AreEqual("9Fix4L4HB4PKeKWY", section.CryptIV);
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_NotExist()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (キーが存在しない)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("notexist");
            Assert.AreEqual(null, config);
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_All()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (全暗号化(ALL指定))
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("All");
            EqualKeyHelper(config, key: "All", crypt: "all");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_ALL2()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (全暗号化(個別指定))
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("ALL2");
            EqualKeyHelper(config, key: "ALL2", crypt: "Type,Path,Src,Dst,Url,C,U,V");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Non()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (全未暗号化)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Non");
            EqualKeyHelper(config, key: "Non", crypt: "Non");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Type()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Type)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Type");
            EqualKeyHelper(config, key: "Type", crypt: "type");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Path()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Path)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Path");
            EqualKeyHelper(config, key: "Path", crypt: "PATH");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Src()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Src)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Src");
            EqualKeyHelper(config, key: "Src", crypt: "src");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Dst()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Dst)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Dst");
            EqualKeyHelper(config, key: "Dst", crypt: "dst");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Url()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Url)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Url");
            EqualKeyHelper(config, key: "Url", crypt: "url");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_C()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:C)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("C");
            EqualKeyHelper(config, key: "C", crypt: "C");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Conn()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Conn)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Conn");
            EqualKeyHelper(config, key: "Conn", crypt: "Conn");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Cs()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Cs)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Cs");
            EqualKeyHelper(config, key: "Cs", crypt: "Cs");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_ConnString()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:ConnString)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("ConnString");
            EqualKeyHelper(config, key: "ConnString", crypt: "ConnString");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Cd()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Cd)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Cd");
            EqualKeyHelper(config, key: "Cd", crypt: "Cd");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_ConnDest()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:ConnDest)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("ConnDest");
            EqualKeyHelper(config, key: "ConnDest", crypt: "ConnDest");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Cp()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Cp)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Cp");
            EqualKeyHelper(config, key: "Cp", crypt: "Cp");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_ConnPort()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:ConnPort)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("ConnPort");
            EqualKeyHelper(config, key: "ConnPort", crypt: "ConnPort");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_U()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:U)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("U");
            EqualKeyHelper(config, key: "U", crypt: "U");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_User()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:User)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("User");
            EqualKeyHelper(config, key: "User", crypt: "User");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Ud()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Ud)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Ud");
            EqualKeyHelper(config, key: "Ud", crypt: "Ud");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_UserDom()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:UserDom)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("UserDom");
            EqualKeyHelper(config, key: "UserDom", crypt: "UserDom");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Un()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Un)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Un");
            EqualKeyHelper(config, key: "Un", crypt: "Un");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_UserName()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:UserName)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("UserName");
            EqualKeyHelper(config, key: "UserName", crypt: "UserName");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Up()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Up)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Up");
            EqualKeyHelper(config, key: "Up", crypt: "Up");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_UserPW()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:UserPW)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("UserPW");
            EqualKeyHelper(config, key: "UserPW", crypt: "UserPW");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V");
            EqualKeyHelper(config, key: "V", crypt: "V");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value");
            EqualKeyHelper(config, key: "Value", crypt: "Value");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V1()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V1)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V1");
            EqualKeyHelper(config, key: "V1", crypt: "V1");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value1()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value1)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value1");
            EqualKeyHelper(config, key: "Value1", crypt: "Value1");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V2()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V2)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V2");
            EqualKeyHelper(config, key: "V2", crypt: "V2");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value2()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value2)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value2");
            EqualKeyHelper(config, key: "Value2", crypt: "Value2");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V3()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V3)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V3");
            EqualKeyHelper(config, key: "V3", crypt: "V3");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value3()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value3)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value3");
            EqualKeyHelper(config, key: "Value3", crypt: "Value3");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V4()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V4)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V4");
            EqualKeyHelper(config, key: "V4", crypt: "V4");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value4()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value4)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value4");
            EqualKeyHelper(config, key: "Value4", crypt: "Value4");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V5()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V5)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V5");
            EqualKeyHelper(config, key: "V5", crypt: "V5");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value5()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value5)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value5");
            EqualKeyHelper(config, key: "Value5", crypt: "Value5");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V6()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V6)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V6");
            EqualKeyHelper(config, key: "V6", crypt: "V6");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value6()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value6)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value6");
            EqualKeyHelper(config, key: "Value6", crypt: "Value6");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V7()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V7)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V7");
            EqualKeyHelper(config, key: "V7", crypt: "V7");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value7()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value7)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value7");
            EqualKeyHelper(config, key: "Value7", crypt: "Value7");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V8()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V8)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V8");
            EqualKeyHelper(config, key: "V8", crypt: "V8");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value8()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value8)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value8");
            EqualKeyHelper(config, key: "Value8", crypt: "Value8");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_V9()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:V9)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("V9");
            EqualKeyHelper(config, key: "V9", crypt: "V9");
        }


        [Test, Category("GeneralUtility/ConnectionConfigHandler")]
        public void EncryptSection_Crypt_Value9()
        {
            // TEST || GeneralUtility\ConnectionConfigHandler.cs || ConnectionConfigSection.Get() || 暗号化指定のセクション名から特定キーの全属性を取得する (暗号化対象:Value9)
            var section = ConnectionConfigSection.GetSection("ConnectionsEncrypt");
            var config = section.Get("Value9");
            EqualKeyHelper(config, key: "Value9", crypt: "Value9");
        }



        private static void EqualKeyHelper(ConnectionConfig config, 
            string key = "Key", string crypt = "Crypt", string type = "Type", 
            string path = "Path", string src = "Src", string dst = "Dst", string url = "Url", 
            string connString = "ConnString", string connDest = "ConnDest", string connPort = "ConnPort",
            string userDom = "UserDom", string userName = "UserName", string userPW = "UserPW", 
            string value1 = "Value1", string value2 = "Value2", string value3 = "Value3", 
            string value4 = "Value4", string value5 = "Value5", string value6 = "Value6", 
            string value7 = "Value7", string value8 = "Value8", string value9 = "Value9",
            string comment = "Comment")
        {
            Assert.AreEqual(key, config.Key);
            Assert.AreEqual(crypt, config.Crypt);
            Assert.AreEqual(type, config.Type);
            Assert.AreEqual(path, config.Path);
            Assert.AreEqual(src, config.Src);
            Assert.AreEqual(dst, config.Dst);
            Assert.AreEqual(url, config.Url);
            Assert.AreEqual(connString, config.ConnString);
            Assert.AreEqual(connDest, config.ConnDest);
            Assert.AreEqual(connPort, config.ConnPort);
            Assert.AreEqual(userDom, config.UserDom);
            Assert.AreEqual(userName, config.UserName);
            Assert.AreEqual(userPW, config.UserPW);
            Assert.AreEqual(value1, config.Value1);
            Assert.AreEqual(value2, config.Value2);
            Assert.AreEqual(value3, config.Value3);
            Assert.AreEqual(value4, config.Value4);
            Assert.AreEqual(value5, config.Value5);
            Assert.AreEqual(value6, config.Value6);
            Assert.AreEqual(value7, config.Value7);
            Assert.AreEqual(value8, config.Value8);
            Assert.AreEqual(value9, config.Value9);
            Assert.AreEqual(comment, config.Comment);
        }

    }
}