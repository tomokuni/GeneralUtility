using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GeneralUtility
{
    /// <summary>
    /// 接続先情報を格納するクラス
    /// </summary>
    public class ConnectionConfig
    {
        /// <summary>Key値 [必須]：識別名</summary>
        public string Key { get; protected set; }
        /// <summary>Crypt値 [任意]</summary>
        public string Crypt { get; protected set; }
        /// <summary>種別 [任意]</summary>
        public string Type { get; protected set; }
        /// <summary>パス [任意]</summary>
        public string Path { get; protected set; }
        /// <summary>Src [任意]</summary>
        public string Src { get; protected set; }
        /// <summary>Dst [任意]</summary>
        public string Dst { get; protected set; }
        /// <summary>URL [任意]</summary>
        public string Url { get; protected set; }
        /// <summary>接続文字列 [任意]</summary>
        public string ConnString { get; protected set; }
        /// <summary>接続先 [任意]</summary>
        public string ConnDest { get; protected set; }
        /// <summary>ポート番号 [任意]</summary>
        public string ConnPort { get; protected set; }
        /// <summary>接続ユーザのドメイン名 [任意]</summary>
        public string UserDom { get; protected set; }
        /// <summary>接続ユーザ名 [任意]</summary>
        public string UserName { get; protected set; }
        /// <summary>接続ユーザのパスワード [任意]</summary>
        public string UserPW { get; protected set; }
        /// <summary>Value1値 [任意]</summary>
        public string Value1 { get; protected set; }
        /// <summary>Value2値 [任意]</summary>
        public string Value2 { get; protected set; }
        /// <summary>Value3値 [任意]</summary>
        public string Value3 { get; protected set; }
        /// <summary>Value4値 [任意]</summary>
        public string Value4 { get; protected set; }
        /// <summary>Value5値 [任意]</summary>
        public string Value5 { get; protected set; }
        /// <summary>Value6値 [任意]</summary>
        public string Value6 { get; protected set; }
        /// <summary>Value7値 [任意]</summary>
        public string Value7 { get; protected set; }
        /// <summary>Value8値 [任意]</summary>
        public string Value8 { get; protected set; }
        /// <summary>Value9値 [任意]</summary>
        public string Value9 { get; protected set; }
        /// <summary>Comment値 [任意]</summary>
        public string Comment { get; protected set; }

        /// <summary>
        /// クラス内容を表すサマリ文字列を取得する
        /// </summary>
        public override string ToString()
        {
            var t = string.IsNullOrWhiteSpace(Type) ? "" : $", Type=\"{Type}\"";
            var p = string.IsNullOrWhiteSpace(Path) ? "" : $", Path=\"{Path}\"";
            var s = string.IsNullOrWhiteSpace(Src) ? "" : $", Src=\"{Src}\"";
            var d = string.IsNullOrWhiteSpace(Dst) ? "" : $", Dst=\"{Dst}\"";
            var u = string.IsNullOrWhiteSpace(Url) ? "" : $", Url=\"{Url}\"";
            var cs = string.IsNullOrWhiteSpace(ConnString) ? "" : $", ConnString=\"{ConnString}\"";
            var cd = string.IsNullOrWhiteSpace(ConnDest) ? "" : $", ConnDest=\"{ConnDest}\"";
            var cp = string.IsNullOrWhiteSpace(ConnPort) ? "" : $", ConnPort=\"{ConnPort}\"";
            var ud = string.IsNullOrWhiteSpace(UserDom) ? "" : $", UserDom=\"{UserDom}\"";
            var un = string.IsNullOrWhiteSpace(UserName) ? "" : $", UserName=\"{UserName}\"";
            var up = string.IsNullOrWhiteSpace(UserPW) ? "" : $", UserPW=\"{UserPW}\"";
            var v1 = string.IsNullOrWhiteSpace(Value1) ? "" : $", Value1=\"{Value1}\"";
            var v2 = string.IsNullOrWhiteSpace(Value2) ? "" : $", Value2=\"{Value2}\"";
            var v3 = string.IsNullOrWhiteSpace(Value3) ? "" : $", Value3=\"{Value3}\"";
            var v4 = string.IsNullOrWhiteSpace(Value4) ? "" : $", Value4=\"{Value4}\"";
            var v5 = string.IsNullOrWhiteSpace(Value5) ? "" : $", Value5=\"{Value5}\"";
            var v6 = string.IsNullOrWhiteSpace(Value6) ? "" : $", Value6=\"{Value6}\"";
            var v7 = string.IsNullOrWhiteSpace(Value7) ? "" : $", Value7=\"{Value7}\"";
            var v8 = string.IsNullOrWhiteSpace(Value8) ? "" : $", Value8=\"{Value8}\"";
            var v9 = string.IsNullOrWhiteSpace(Value9) ? "" : $", Value9=\"{Value9}\"";
            var c = string.IsNullOrWhiteSpace(Comment) ? "" : $", Comment=\"{Comment}\"";
            var res = $"Key=\"{Key}\"{t}{p}{s}{d}{u}{cs}{cd}{cp}{ud}{un}{up}{v1}{v2}{v3}{v4}{v5}{v6}{v7}{v8}{v9}{c}";
            return res;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="element">app.config から取得した接続先情報</param>
        /// <param name="crypt">暗号復号化クラス</param>
        public ConnectionConfig(ConnectionConfigElement element, Func<string, string> Decrypter)
        {
            bool IsContains<T>(IEnumerable<T> a, IEnumerable<T> b)
            {
                return a.Where(x => b.Contains(x)).Any();
            }

            if (element == null)
                return;

            var flags = element.Crypt.ToLower().Split(new char[] { ' ', ',' }).Select(s => s.Trim()).ToList();
            Key = element.Key;
            Crypt = element.Crypt;
            Type = IsContains(flags, new string[] { "all", "type" }) ? (Decrypter(element.Type)) : element.Type;
            Path = IsContains(flags, new string[] { "all", "path" }) ? (Decrypter(element.Path)) : element.Path;
            Src = IsContains(flags, new string[] { "all", "src" }) ? (Decrypter(element.Src)) : element.Src;
            Dst = IsContains(flags, new string[] { "all", "dst" }) ? (Decrypter(element.Dst)) : element.Dst;
            Url = IsContains(flags, new string[] { "all", "url" }) ? (Decrypter(element.Url)) : element.Url;
            ConnString = IsContains(flags, new string[] { "all", "c", "conn", "cs", "connstring" }) ? (Decrypter(element.ConnString)) : element.ConnString;
            ConnDest = IsContains(flags, new string[] { "all", "c", "conn", "cd", "conndest" }) ? (Decrypter(element.ConnDest)) : element.ConnDest;
            ConnPort = IsContains(flags, new string[] { "all", "c", "conn", "cp", "connport" }) ? (Decrypter(element.ConnPort)) : element.ConnPort;
            UserDom = IsContains(flags, new string[] { "all", "u", "user", "ud", "userdom" }) ? (Decrypter(element.UserDom)) : element.UserDom;
            UserName = IsContains(flags, new string[] { "all", "u", "user", "un", "username" }) ? (Decrypter(element.UserName)) : element.UserName;
            UserPW = IsContains(flags, new string[] { "all", "u", "user", "up", "userpw" }) ? (Decrypter(element.UserPW)) : element.UserPW;
            Value1 = IsContains(flags, new string[] { "all", "v", "value", "v1", "value1" }) ? (Decrypter(element.Value1)) : element.Value1;
            Value2 = IsContains(flags, new string[] { "all", "v", "value", "v2", "value2" }) ? (Decrypter(element.Value2)) : element.Value2;
            Value3 = IsContains(flags, new string[] { "all", "v", "value", "v3", "value3" }) ? (Decrypter(element.Value3)) : element.Value3;
            Value4 = IsContains(flags, new string[] { "all", "v", "value", "v4", "value4" }) ? (Decrypter(element.Value4)) : element.Value4;
            Value5 = IsContains(flags, new string[] { "all", "v", "value", "v5", "value5" }) ? (Decrypter(element.Value5)) : element.Value5;
            Value6 = IsContains(flags, new string[] { "all", "v", "value", "v6", "value6" }) ? (Decrypter(element.Value6)) : element.Value6;
            Value7 = IsContains(flags, new string[] { "all", "v", "value", "v7", "value7" }) ? (Decrypter(element.Value7)) : element.Value7;
            Value8 = IsContains(flags, new string[] { "all", "v", "value", "v8", "value8" }) ? (Decrypter(element.Value8)) : element.Value8;
            Value9 = IsContains(flags, new string[] { "all", "v", "value", "v9", "value9" }) ? (Decrypter(element.Value9)) : element.Value9;
            Comment = element.Comment;
        }
    }


    /// <summary>
    /// 構成ファイル内の接続先情報の構成要素
    /// </summary>
    public class ConnectionConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key { get => (string)this["key"]; }

        [ConfigurationProperty("type", DefaultValue = "")]
        public string Type { get => (string)this["type"]; }

        [ConfigurationProperty("path", DefaultValue = "")]
        public string Path { get => (string)this["path"]; }

        [ConfigurationProperty("src", DefaultValue = "")]
        public string Src { get => (string)this["src"]; }

        [ConfigurationProperty("dst", DefaultValue = "")]
        public string Dst { get => (string)this["dst"]; }

        [ConfigurationProperty("url", DefaultValue = "")]
        public string Url { get => (string)this["url"]; }

        [ConfigurationProperty("crypt", DefaultValue = "")]
        public string Crypt { get => (string)this["crypt"]; }

        [ConfigurationProperty("connString", DefaultValue = "")]
        public string ConnString { get => (string)this["connString"]; }

        [ConfigurationProperty("connDest", DefaultValue = "")]
        public string ConnDest { get => (string)this["connDest"]; }

        [ConfigurationProperty("connPort", DefaultValue = "")]
        public string ConnPort { get => (string)this["connPort"]; }

        [ConfigurationProperty("userDom", DefaultValue = "")]
        public string UserDom { get => (string)this["userDom"]; }

        [ConfigurationProperty("userName", DefaultValue = "")]
        public string UserName { get => (string)this["userName"]; }

        [ConfigurationProperty("userPW", DefaultValue = "")]
        public string UserPW { get => (string)this["userPW"]; }

        [ConfigurationProperty("value1", DefaultValue = "")]
        public string Value1 { get => (string)this["value1"]; }

        [ConfigurationProperty("value2", DefaultValue = "")]
        public string Value2 { get => (string)this["value2"]; }

        [ConfigurationProperty("value3", DefaultValue = "")]
        public string Value3 { get => (string)this["value3"]; }

        [ConfigurationProperty("value4", DefaultValue = "")]
        public string Value4 { get => (string)this["value4"]; }

        [ConfigurationProperty("value5", DefaultValue = "")]
        public string Value5 { get => (string)this["value5"]; }

        [ConfigurationProperty("value6", DefaultValue = "")]
        public string Value6 { get => (string)this["value6"]; }

        [ConfigurationProperty("value7", DefaultValue = "")]
        public string Value7 { get => (string)this["value7"]; }

        [ConfigurationProperty("value8", DefaultValue = "")]
        public string Value8 { get => (string)this["value8"]; }

        [ConfigurationProperty("value9", DefaultValue = "")]
        public string Value9 { get => (string)this["value9"]; }

        [ConfigurationProperty("comment", DefaultValue = "")]
        public string Comment { get => (string)this["comment"]; }
    }


    /// <summary>
    /// 構成ファイル内の子要素である接続先情報のコレクションを格納する構成要素
    /// </summary>
    public class ConnectionConfigElementCollection : ConfigurationElementCollection
    {
        public Func<string, string> Decrypter;

        /// <summary>
        /// 新しい ConfigurationElementを生成
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionConfigElement();
        }

        /// <summary>
        /// 指定した構成要素の要素キーを取得
        /// </summary>
        /// <param name="element">構成要素</param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConnectionConfigElement)element).Key;
        }

        /// <summary>
        /// キー指定で要素を取り出す
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ConnectionConfig Get(string key)
        {
            var element = (ConnectionConfigElement)BaseGet((object)key);
            if (element == null)
                return null;

            var config = new ConnectionConfig(element, Decrypter);
            return config;
        }

        /// <summary>
        /// 全要素を取り出す
        /// </summary>
        /// <returns></returns>
        public List<ConnectionConfig> GetAll()
        {
            var list = BaseGetAllKeys().Select(key => Get((string)key)).ToList();
            return list;
        }
    }


    /// <summary>
    /// 接続先情報のコレクション要素
    /// </summary>
    // app.config 構成例１
    // <configuration>
    //   <configSections>
    //     <section name = "Connections" type="GeneralUtility.ConnectionConfigSection, GetWindowsTaskList" />
    //   </configSections>
    //   <Connections>
    //     <add key = "KeyName" connDest="192.168.1.1" crypt="u" userName="m8Tkqn3TWZ2vlsYMiqxzbw==" userPW="Ws1W9+ZEQTw1VVjShju0Kg==" />
    //   </Connections>
    // </configuration>
    public class ConnectionConfigSection : ConfigurationSection
    {
        public const string DefaultSectionTagName = "Connections";

        /// <summary>セクションを表すタグ名</summary>
        public string SectionTagName { get; protected set; }

        /// <summary>セクションを表すタグ名</summary>
        [ConfigurationProperty("cryptKey", DefaultValue = "9Fix4L4HB4PKeKWY")]
        public string CryptKey { get => (string)this["cryptKey"]; }

        /// <summary>セクションを表すタグ名</summary>
        [ConfigurationProperty("cryptIV", DefaultValue = "pf69DL6GrWFyZcMK")]
        public string CryptIV { get => (string)this["cryptIV"]; }

        /// <summary>接続先情報のコレクション要素</summary>
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ConnectionConfigElementCollection Elements
        {
            get {
                var collection = (ConnectionConfigElementCollection)base[""];
                collection.Decrypter = Decrypter;
                return collection;
            }
        }

        /// <summary>
        /// 暗号化された文字列を復号するメソッド
        /// </summary>
        /// <param name="base64Text"></param>
        /// <returns></returns>
        public string Decrypter(string base64Text)
        {
            if (string.IsNullOrWhiteSpace(CryptKey) || string.IsNullOrWhiteSpace(CryptIV) || string.IsNullOrWhiteSpace(base64Text))
                return base64Text;

            return Helper.DecryptFromBase64(base64Text, CryptKey, CryptIV);
        }

        /// <summary>
        /// ConnectionConfigSection オブジェクトを取得する
        /// </summary>
        /// <param name="SectionName"></param>
        /// <returns></returns>
        static public ConnectionConfigSection GetSection(string SectionName = DefaultSectionTagName)
        {
            var name = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            var configNames = new[] { $"{name}.exe.config", $"{name}.dll.config", $"App.config" };
            foreach (var configFile in configNames)
            {
                if (File.Exists(configFile))
                {
                    var exeFileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile };
                    var config = ConfigurationManager.OpenMappedExeConfiguration(exeFileMap, ConfigurationUserLevel.None);
                    var configSection = config.GetSection(SectionName) as ConnectionConfigSection;
                    configSection.SectionTagName = SectionName;
                    return configSection;
                }
            }

            var section = ConfigurationManager.GetSection(SectionName) as ConnectionConfigSection;
            section.SectionTagName = SectionName;
            return section;
        }

        /// <summary>
        /// 接続先情報を全て取得する
        /// </summary>
        /// <returns></returns>
        public List<ConnectionConfig> GetAll()
        {
            var configList = Elements.GetAll();
            return configList;
        }

        /// <summary>
        /// 接続先情報をキー指定で取得する
        /// </summary>
        /// <returns></returns>
        public ConnectionConfig Get(string key)
        {
            var config = Elements.Get(key);
            return config;
        }



    }

    public static class Helper
    {
        /// <summary>
        /// 暗号化されたBase64形式の入力文字列をAES復号して平文の文字列を返す静的メソッド
        /// </summary>
        /// <param name="base64Text"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string DecryptFromBase64(string base64Text, string key, string iv)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteIv = Encoding.UTF8.GetBytes(iv);
            return DecryptFromBase64(base64Text, byteKey, byteIv);
        }

        /// <summary>
        /// 暗号化されたBase64形式の入力文字列をAES復号して平文の文字列を返す静的メソッド
        /// </summary>
        /// <param name="base64Text"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        /// <remarks>https://www.atmarkit.co.jp/ait/articles/1709/06/news020.html</remarks>
        public static string DecryptFromBase64(string base64Text, byte[] key, byte[] iv)
        {
            // Base64文字列をバイト型配列に変換
            byte[] src = Convert.FromBase64String(base64Text);

            // Decryptor（復号器）を用意する
            using var am = new AesManaged();
            using var decryptor = am.CreateDecryptor(key, iv);
            // 入力ストリームを開く
            using var inStream = new MemoryStream(src, false);
            // 出力ストリームを用意する
            using var outStream = new MemoryStream();
            // 復号して一定量ずつ読み出し、それを出力ストリームに書き出す
            using (var cs = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read))
            {
                byte[] buffer = new byte[4096]; // バッファーサイズはBlockSizeの倍数にする
                int len = 0;
                while ((len = cs.Read(buffer, 0, 4096)) > 0)
                    outStream.Write(buffer, 0, len);
            }
            // 出力がファイルなら、以上で完了

            // 文字列に変換して返す
            byte[] result = outStream.ToArray();
            return Encoding.Unicode.GetString(result);
        }
    }
}
