using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace GeneralUtility
{ 
/// <summary>
/// ClosedXMLを使用したExcel操作用ユーティリティクラス
/// </summary>
public class ClosedXMLHelper : IDisposable
{
    /// <summary>ファイル名</summary>
    public string FileName { get; protected set; } = "";

    /// <summary>ワークブック・オブジェクト</summary>
    public XLWorkbook? Workbook { get; protected set; } = null;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ClosedXMLHelper()
    {
    }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="openAsSoon"></param>
    public ClosedXMLHelper(string fileName, bool isNew = false) : this()
    {
        if (isNew)
            OpenNew(fileName);
        else
            Open(fileName);
    }


    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        if (Workbook != null)
        {
            Workbook.Dispose();
            FileName = "";
            Workbook = null;
        }
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// ファイルを開く
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="isNew"></param>
    public void OpenNew(string fileName)
    {
        if (Workbook != null)
            throw new InvalidOperationException("Workbook is already open");

        Workbook = new XLWorkbook();
        FileName = fileName;
    }


    /// <summary>
    /// ファイルを開く
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="isNew"></param>
    public void Open(string fileName)
    {
        if (Workbook != null)
            throw new InvalidOperationException("Workbook is already open");

        if (File.Exists(fileName))
            Workbook = new XLWorkbook(fileName);
        else
            throw new FileNotFoundException(string.Format("File:'{0}' is not found", fileName));

        FileName = fileName; 
    }


    /// <summary>
    /// ファイルをセーブせずに閉じる
    /// </summary>
    public void Close()
    {
        Dispose();
    }


    /// <summary>
    /// ファイルを上書保存する
    /// </summary>
    public void Save()
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        Workbook.SaveAs(FileName);
    }


    /// <summary>
    /// ファイルを名前を付けて保存する
    /// </summary>
    /// <param name="fileName"></param>
    public void SaveAs(string fileName)
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        Workbook.SaveAs(fileName);
        FileName = fileName;
    }


    /// <summary>
    /// テーブルを取得する
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public IXLTable GetTable(string tableName)
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        var table = Workbook.Table(tableName);
        return table;
    }


    /// <summary>
    /// 指定したシートを選択してアクティブにする
    /// </summary>
    /// <param name="sheet"></param>
    public void SetActiveSheet(IXLWorksheet sheet)
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        Workbook.SetActiveSheet(sheet);
    }


    /// <summary>
    /// 指定した名前のシートが存在するか？
    /// </summary>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public bool IsExistSheet(string sheetName)
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        var isExist = Workbook.TryGetWorksheet(sheetName, out _);
        return isExist;
    }


    /// <summary>
    /// 指定した名前のシートを取得する
    /// </summary>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public IXLWorksheet GetSheet(string sheetName)
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        var sheet = Workbook.Worksheet(sheetName);
        return sheet;
    }


    /// <summary>
    /// 指定した名前のシートを追加する
    /// </summary>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public IXLWorksheet AddSheet(string sheetName)
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        var sheet = Workbook.Worksheets.Add(sheetName);
        return sheet;
    }


    /// <summary>
    /// Excelにシートを追加する
    /// </summary>
    /// <param name="workbook">操作するExcelPackageオブジェクト</param>
    /// <param name="sheetname">追加するシート名</param>
    /// <param name="edit">追加したシートを操作するデリゲート</param>
    public void ExportToExcel(string sheetname, Action<IXLWorksheet>? edit = null)
    {
        if (Workbook == null)
            throw new InvalidOperationException("Workbook is not open");

        // 指定したワークシートが存在する場合は削除
        var existSheet = Workbook.TryGetWorksheet(sheetname, out IXLWorksheet sheet);
        if (existSheet)
            sheet.Delete();

        // ワークシートを追加
        sheet = Workbook.Worksheets.Add(sheetname);

        // 追加したシートへの処理
        if (edit != null)
            edit.Invoke(sheet);
    }


    /// <summary>
    /// Excelにシートを追加する
    /// </summary>
    /// <param name="filename">書き込むファイル名</param>
    /// <param name="sheetname">追加するシート名</param>
    /// <param name="edit">追加したシートを操作するデリゲート</param>
    public static void ExportToExcel(string filename, string sheetname, bool isNew = false, Action<IXLWorksheet>? edit = null)
    {
        using var excel = new ClosedXMLHelper(filename, isNew);
        excel.ExportToExcel(sheetname, edit);
        excel.Save();
    }
}

    static public class ClosedXMLHelperExtensions
    {
        /// <summary>
        /// IXLTable 行単位に Dictionary として取得する
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<IDictionary<string, object>> ToDictionary(this IXLTable table)
        {
            var colNames = table.Fields.Select(field => field.Name).ToList();
            foreach (var excelRow in table.DataRange.Rows())
            {
                var row = colNames.Aggregate(new Dictionary<string, object>(),
                    (a, p) => { a.Add(p, excelRow.Field(p).GetFormattedString()); return a; });
                yield return row;

                // 若干だが LINQ の Aggregate を使用したほうが早いっぽい
                // var row = new Dictionary<string, object>();
                // foreach (var colName in colNames)
                // {
                //     row.Add(colName, excelRow.Field(colName).GetFormattedString());
                // }
            }
        }


        /// <summary>
        /// IXLTable 行単位に Dictionary として取得して、Entity に変換する
        /// <para>Field は FormattedString として取得する</para>
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToEntity<T>(this IXLTable table, Func<IDictionary<string, object>, T> createEntiry)
        {
            return table.ToDictionary().Select(s => createEntiry(s));
        }


        /// <summary>
        /// IXLTable 行単位に Dictionary として取得して、ExpandoObject に変換する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> ToExpando(this IXLTable table)
        {
            return table.ToDictionary().Select(
                item => item.Aggregate(new ExpandoObject(),
                    (a, p) => { (a as IDictionary<string, object>).Add(p.Key, p.Value); return a; }));

            // 若干だが LINQ の Aggregate を使用したほうが早いっぽい
            // foreach (var d in table.ToDictionary())
            // {
            //     var eo = d.Aggregate(new ExpandoObject(), (a, p) => { (a as IDictionary<string, Object>).Add(p.Key, p.Value); return a; });
            //     yield return eo;
            // }
        }


        /// <summary>
        /// IXLTable を DataTable に変換する
        /// <para>Field は FormattedString として取得して DataTable に設定する</para>
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IXLTable table)
        {
            var dbt = new DataTable(table.Name);
            var excelColumnCount = table.Fields.Count();
            foreach (var excelCol in table.Fields)
            {
                dbt.Columns.Add(excelCol.Name);
            }
            foreach (var excelRow in table.DataRange.Rows())
            {
                var dataRow = dbt.NewRow();
                for (int i = 0; i < excelColumnCount; i++)
                {
                    dataRow[i] = excelRow.Field(i).GetFormattedString();
                }
                dbt.Rows.Add(dataRow);
            }
            return dbt;
        }


        /// <summary>
        /// 指定したシートを選択してアクティブにする
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheet"></param>
        public static void SetActiveSheet(this XLWorkbook workbook, IXLWorksheet sheet)
        {
            foreach (var s in workbook.Worksheets)
            {
                var isCurrentSheet = s.Equals(sheet);
                s.SetTabActive(isCurrentSheet);
                s.SetTabSelected(isCurrentSheet);
            }
        }


        /// <summary>
        /// 指定したシートを選択してアクティブにする
        /// </summary>
        /// <param name="sheet"></param>
        public static void SetActive(this IXLWorksheet sheet)
        {
            sheet.Workbook.SetActiveSheet(sheet);
        }
    }
}
