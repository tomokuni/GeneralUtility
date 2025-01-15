using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GeneralUtility
{
    /// <summary>
    /// 属性値を格納するクラス
    /// </summary>
    public class TextConfig
    {
        /// <summary>Key値 [必須]</summary>
        public string Key { get; set; }
        /// <summary>Comment値 [任意]</summary>
        public string Comment { get; set; }
        /// <summary>Text01値 [任意]</summary>
        public string Text01 { get; set; }
        /// <summary>Text02値 [任意]</summary>
        public string Text02 { get; set; }
        /// <summary>Text03値 [任意]</summary>
        public string Text03 { get; set; }
        /// <summary>Text04値 [任意]</summary>
        public string Text04 { get; set; }
        /// <summary>Text05値 [任意]</summary>
        public string Text05 { get; set; }
        /// <summary>Text06値 [任意]</summary>
        public string Text06 { get; set; }
        /// <summary>Text07値 [任意]</summary>
        public string Text07 { get; set; }
        /// <summary>Text08値 [任意]</summary>
        public string Text08 { get; set; }
        /// <summary>Text09値 [任意]</summary>
        public string Text09 { get; set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="element">app.config から取得した接続先情報</param>
        public TextConfig(TextConfigElement element)
        {
            Key = element.Key;
            Comment = element.Comment;
            Text01 = element.Text01;
            Text02 = element.Text02;
            Text03 = element.Text03;
            Text04 = element.Text04;
            Text05 = element.Text05;
            Text06 = element.Text06;
            Text07 = element.Text07;
            Text08 = element.Text08;
            Text09 = element.Text09;
        }

        /// <summary>
        /// クラス内容を表すサマリ文字列を取得する
        /// </summary>
        public override string ToString()
        {
            var c = string.IsNullOrWhiteSpace(Comment) ? "" : $", comment=\"{Comment}\"";
            var t01 = string.IsNullOrWhiteSpace(Text01) ? "" : $", text01=\"{Text01}\"";
            var t02 = string.IsNullOrWhiteSpace(Text02) ? "" : $", text02=\"{Text02}\"";
            var t03 = string.IsNullOrWhiteSpace(Text03) ? "" : $", text03=\"{Text03}\"";
            var t04 = string.IsNullOrWhiteSpace(Text04) ? "" : $", text04=\"{Text04}\"";
            var t05 = string.IsNullOrWhiteSpace(Text05) ? "" : $", text05=\"{Text05}\"";
            var t06 = string.IsNullOrWhiteSpace(Text06) ? "" : $", text06=\"{Text06}\"";
            var t07 = string.IsNullOrWhiteSpace(Text07) ? "" : $", text07=\"{Text07}\"";
            var t08 = string.IsNullOrWhiteSpace(Text08) ? "" : $", text08=\"{Text08}\"";
            var t09 = string.IsNullOrWhiteSpace(Text09) ? "" : $", text09=\"{Text09}\"";
            var res = $"Key=\"{Key}\"{c}{t01}{t02}{t03}{t04}{t05}{t06}{t07}{t08}{t09}";
            return res;
        }
    }


    /// <summary>
    /// 構成ファイル内の接続先情報の構成要素
    /// </summary>
    public class TextConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key { get => (string)this["key"]; }

        [ConfigurationProperty("comment", DefaultValue = "")]
        public string Comment { get => (string)this["comment"]; }

        [ConfigurationProperty("text01", DefaultValue = "")]
        public string Text01 { get => (string)this["text01"]; }

        [ConfigurationProperty("text02", DefaultValue = "")]
        public string Text02 { get => (string)this["text02"]; }

        [ConfigurationProperty("text03", DefaultValue = "")]
        public string Text03 { get => (string)this["text03"]; }

        [ConfigurationProperty("text04", DefaultValue = "")]
        public string Text04 { get => (string)this["text04"]; }

        [ConfigurationProperty("text05", DefaultValue = "")]
        public string Text05 { get => (string)this["text05"]; }

        [ConfigurationProperty("text06", DefaultValue = "")]
        public string Text06 { get => (string)this["text06"]; }

        [ConfigurationProperty("text07", DefaultValue = "")]
        public string Text07 { get => (string)this["text07"]; }

        [ConfigurationProperty("text08", DefaultValue = "")]
        public string Text08 { get => (string)this["text08"]; }

        [ConfigurationProperty("text09", DefaultValue = "")]
        public string Text09 { get => (string)this["text09"]; }
    }


    /// <summary>
    /// 構成ファイル内の子要素である接続先情報のコレクションを格納する構成要素
    /// </summary>
    public class TextConfigElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// 新しい ConfigurationElementを生成
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new TextConfigElement();
        }

        /// <summary>
        /// 指定した構成要素の要素キーを取得
        /// </summary>
        /// <param name="element">構成要素</param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TextConfigElement)element).Key;
        }

        /// <summary>
        /// キー指定で要素を取り出す
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TextConfig GetConfig(string key)
        {
            var element = (TextConfigElement)BaseGet((object)key);
            var config = new TextConfig(element);
            return config;
        }

        /// <summary>
        /// 全要素を取り出す
        /// </summary>
        /// <returns></returns>
        public List<TextConfig> GetAllConfig()
        {
            var list = BaseGetAllKeys().Select(key => GetConfig((string)key)).ToList();
            return list;
        }
    }

    public class TextConfigSection : ConfigurationSection
    {
        /// <summary>セクションを表すタグ名</summary>
        public string SectionTagName { get; set; } = "TextConfig";


        /// <summary>
        /// 接続先情報のコレクション要素
        /// </summary>
        // app.config 構成例
        // <configuration>
        //   <configSections>
        //     <section name = "TextConfig" type="GUtil.TextConfigSection, ConnectionTest" />
        //   </configSections>
        //   <TextConfig>
        //     <add key = "KeyName" comment="CommentText==" text01="Text01" text02="Text02" />
        //   </TextConfig>
        // </configuration>
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public TextConfigElementCollection Servers
        {
            get {
                var collection = (TextConfigElementCollection)base[""];
                return collection;
            }
        }

        /// <summary>
        /// 接続先情報を全て取得する
        /// </summary>
        /// <returns></returns>
        public List<TextConfig> GetAllConfig()
        {
            var configSection = ConfigurationManager.GetSection(SectionTagName) as TextConfigSection;
            var configList = configSection.Servers.GetAllConfig();
            return configList;
        }

        /// <summary>
        /// 接続先情報をキー指定で取得する
        /// </summary>
        /// <returns></returns>
        public TextConfig GetConfig(string key)
        {
            var configSection = ConfigurationManager.GetSection(SectionTagName) as TextConfigSection;
            var config = configSection.Servers.GetConfig(key);
            return config;
        }
    }
}
