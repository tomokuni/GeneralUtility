using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneralUtility
{
    /// <summary>
    /// ユーティリティクラス
    /// </summary>
    public static partial class GuUtil
    {
        /// <summary>
        /// App.config の XPath /configuration/appSettings/add/@key 属性値が致するvalue属性値を取得する
        /// </summary>
        /// <param name="configKey">Key属性値</param>
        /// <returns>value属性値</returns>
        public static string GetConfigValue(string configKey)
        {
            return ConfigurationManager.AppSettings[configKey]?.ToString();
        }

        /// <summary>
        /// ファイルにメッセージを追記する
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="msgs"></param>
        public static void WriteFile(string fileFullPath, IEnumerable<string> msgs, Encoding encoding = null)
        {
            DirectoryInfo hDirInfo = Directory.GetParent(fileFullPath);
            if (!Directory.Exists(hDirInfo.FullName))
                Directory.CreateDirectory(hDirInfo.FullName);

            encoding ??= Encoding.UTF8;
            StreamWriter writer = new(fileFullPath, true, encoding);
            foreach (var m in msgs)
                writer.WriteLine(m);
            writer.Close();
        }

        /// <summary>
        /// ファイルにメッセージを追記する
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="msg"></param>
        public static void WriteFile(string fileFullPath, string msg)
        {
            WriteFile(fileFullPath, new string[] { msg });
        }

        /// <summary>
        /// C# で実行ファイルのフォルダを取得
        /// </summary>
        /// <returns></returns>
        public static string GetBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        }
    }

    /// <summary>
    /// 拡張メソッドの定義
    /// </summary>
    static public class GuUtilExtensions
    {

        /// <summary>
        /// boolに変換する拡張メソッド
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool? ToBool<T>(this T self)
        {
            if (self is bool src)
                return src;
            else
                return (bool.TryParse(self.ToString(), out bool val)) ? (bool?)val : null;
        }

        /// <summary>
        /// intに変換する拡張メソッド
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int? ToInt<T>(this T self)
        {
            if (self is int src)
                return src;
            else
                return (int.TryParse(self.ToString(), out int val)) ? (int?)val : null;
        }


        /// <summary>
        /// longに変換する拡張メソッド
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long? ToLong<T>(this T self)
        {
            if (self is long src)
                return src;
            else
                return (long.TryParse(self.ToString(), out long val)) ? (int?)val : null;
        }


        /// <summary>
        /// decimalに変換する拡張メソッド
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static decimal? ToDecimal<T>(this T self)
        {
            if (self is decimal src)
                return src;
            else
                return (decimal.TryParse(self.ToString(), out decimal val)) ? (decimal?)val : null;
        }


        /// <summary>
        /// DataTimeに変換する拡張メソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static DateTime? ToDateTime<T>(this T self, string format = null)
        {
            if (self is DateTime src)
                return src;
            else
                if (string.IsNullOrWhiteSpace(format))
                    return (DateTime.TryParse(self.ToString(), out DateTime val)) ? (DateTime?)val : null;
                else
                {
                    var res = DateTime.TryParseExact(self.ToString(), format, new CultureInfo("ja-JP"), DateTimeStyles.None, out DateTime val);
                    return res ? (DateTime?)val : null;
                }
        }


        /// <summary>
        /// 指定した要素がこの要素内に含まれるかを示す値を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="list"></param>
        /// <returns>true:含む, false:含まない</returns>
        public static bool Contains<T>(this IEnumerable<T> self, IEnumerable<T> list)
        {
            return self.Where(x => list.Contains(x)).Any();
        }


        /// <summary>
        /// 日付範囲をループするための列挙子を返す
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> EachDay(this DateTime start, DateTime end)
        {
            return Enumerable.Range(0, 1 + end.Subtract(start).Days).Select(offset => start.AddDays(offset));
        }


        /// <summary>
        /// 指定したキーに関連付けられている要素を取得&削除します。
        /// <para>要素を取得できなかった場合は、createDefalut で取得される値を返します。</para>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="createDefalut"></param>
        /// <returns></returns>
        public static V TakeOrDefalut<K, V>(this IDictionary<K, V> self, K key, Func<V> createDefalut)
        {
            V outValue;
            if (self.TryGetValue(key, out outValue))
                self.Remove(key);
            else
                outValue = createDefalut();
            return outValue;
        }


        /// <summary>
        /// 指定したキーに関連付けられている要素を取得&削除します。
        /// <para>要素を取得できなかった場合は、既定値 もしくは defalutValue で取得される値を返します。</para>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="createDefalut"></param>
        /// <returns></returns>
        public static V TakeOrDefalut<K, V>(this IDictionary<K, V> self, K key, V defalutValue = default(V))
        {
            return TakeOrDefalut(self, key, () => defalutValue);
        }


        /// <summary>
        /// 指定したキーに関連付けられている要素を取得します。
        /// <para>要素を取得できなかった場合は、createDefalut で取得される値を返します。</para>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public static V GetOrDefalut<K, V>(this IDictionary<K, V> self, K key, Func<V> createDefalut)
        {
            V outValue;
            return self.TryGetValue(key, out outValue) ? outValue : createDefalut();
        }


        /// <summary>
        /// TimeSpan の短い日本語表記文字列を返す
        /// </summary>
        /// <param name="span"></param>
        /// <param name="fixFormat">時分秒を固定で出力するか</param>
        /// <returns>1日02時03分04秒05ﾐﾘ秒 の形式</returns>
        public static string ToStringShortJp(this TimeSpan span, bool fixFormat = true)
        {
            string d = span.Days == 0 ? "" : $"{span.Days}日";
            string h = (span.Hours, d != "", fixFormat) switch
            {
                (0, true, true) => $"{span.Hours:00}時間",
                (0, false, true) => $"{span.Hours}時間",
                (0, _, false) => "",
                (_, true, _) => $"{span.Hours:00}時間",
                (_, false, _) => $"{span.Hours}時間",
            };
            string m = (span.Minutes, $"{d}{h}" != "", fixFormat) switch
            {
                (0, true, true) => $"{span.Minutes:00}分",
                (0, false, true) => $"{span.Minutes}分",
                (0, _, false) => "",
                (_, true, _) => $"{span.Minutes:00}分",
                (_, false, _) => $"{span.Minutes}分",
            };
            string s = (span.Seconds, $"{d}{h}{m}" != "", fixFormat) switch
            {
                (0, true, true) => $"{span.Seconds:00}秒",
                (0, false, true) => $"{span.Seconds}秒",
                (0, _, false) => "",
                (_, true, _) => $"{span.Seconds:00}秒",
                (_, false, _) => $"{span.Seconds}秒",
            };
            string ms = (span.Milliseconds, $"{d}{h}{m}{s}" != "", fixFormat) switch
            {
                (0, _, _) => "",
                (_, true, _) => $"{span.Milliseconds:00}ﾐﾘ秒",
                (_, false, _) => $"{span.Milliseconds}ﾐﾘ秒",
            };
            string res = $"{d}{h}{m}{s}{ms}";
            return res;
        }


        /// <summary>
        /// IEnumerable&lt;T&gt;をDataTableに変換するメソッド
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        private static DataTable ClassToDataTable<T>(IEnumerable<T> list) where T : class
        {
            DataTable table = new();
            // 列情報の設定
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            // 行の格納
            foreach (T item in list)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            //
            return table;
        }


        /// <summary>
        /// IEnumerable&lt;IDictionary&gt;をDataTableに変換するメソッド
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        private static DataTable DictionaryToDataTable<IDictionary>(IEnumerable<IDictionary> list)
        {
            DataTable table = new();
            // 列情報の設定
            var first = list.First() as IDictionary<string, object>;
            foreach (var kvp in first)
            {
                var t = kvp.Value?.GetType() ?? typeof(object);
                table.Columns.Add(kvp.Key, Nullable.GetUnderlyingType(t) ?? t);
            }
            // 行の格納
            foreach (IDictionary<string, object> record in list)
            {
                DataRow row = table.NewRow();
                foreach (var kvp in record)
                {
                    row[kvp.Key] = kvp.Value ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            //
            return table;
        }


        /// <summary>
        /// IEnumerable&lt;T&gt;をDataTableに変換する拡張メソッド
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : class
        {
            return ClassToDataTable(list);
        }


        /// <summary>
        /// IEnumerable&lt;ExpandoObject&gt;をDataTableに変換する拡張メソッド
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IEnumerable<IDictionary> list)
        {
            return DictionaryToDataTable(list);
        }


        /// <summary>
        /// DataTableをList&lt;IDictionary&lt;string, T&gt;&gt;に変換する拡張メソッド
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<IDictionary<string, object>> ToDictionaryList(this DataTable table)
        {
            var result = table.AsEnumerable().Select(row =>
            {
                var dict = new ExpandoObject() as IDictionary<string, object>;
                foreach (DataColumn col in table.Columns)
                {
                    var value = row[col.ColumnName];
                    dict[col.ColumnName] = Convert.IsDBNull(value) ? null : value;
                }
                return dict;
            }).ToList();
            return result;
        }
    }
}
