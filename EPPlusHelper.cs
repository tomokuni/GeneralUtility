using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Data;
using System.IO;

namespace GeneralUtility
{
    /// <summary>
    /// EPPlusを使用したExcel操作用ユーティリティクラス
    /// </summary>
    public static partial class EPPlusHelper
    {
        /// <summary>
        /// Excelにシートを追加して、DataTableの内容を書き込む
        /// </summary>
        /// <param name="package">操作するExcelPackageオブジェクト</param>
        /// <param name="sheetname">追加するシート名</param>
        /// <param name="table">書き込むDataTalbe</param>
        /// <param name="afterAction">シートを操作するデリゲート</param>
        /// <param name="tableStyle">書き込んだデータに対するTableStyles</param>
        public static void ExportToExcelAddSheet(ExcelPackage package, string sheetname, DataTable table, 
                                                 Action<ExcelWorksheet> afterAction = null, TableStyles? tableStyle = null)
        {
            ExportToExcelAddSheet(package, sheetname, sheet =>
            {
                // A1セルを始点として書き込み
                if (tableStyle != null)
                {
                    sheet.Cells["A1"].LoadFromDataTable(table, true, (TableStyles)tableStyle);
                }
                else
                {
                    sheet.Cells["A1"].LoadFromDataTable(table, true);
                }

                // 追加したシートへの後処理
                afterAction?.Invoke(sheet);
            });
        }


        /// <summary>
        /// Excelにシートを追加する
        /// </summary>
        /// <param name="filename">書き込むファイル名</param>
        /// <param name="sheetname">追加するシート名</param>
        /// <param name="edit">追加したシートを操作するデリゲート</param>
        public static void ExportToExcelAddSheet(string filename, string sheetname, Action<ExcelWorksheet> edit)
        {
            try
            {
                // 出力ファイルの準備（実行ファイルと同じフォルダに出力される）
                var newFile = new FileInfo(filename);

                // Excelファイルの作成
                using (var package = new ExcelPackage(newFile))
                {
                    ExportToExcelAddSheet(package, sheetname, edit);

                    // 保存
                    package.Save();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }


        /// <summary>
        /// Excelにシートを追加する
        /// </summary>
        /// <param name="package">操作するExcelPackageオブジェクト</param>
        /// <param name="sheetname">追加するシート名</param>
        /// <param name="edit">追加したシートを操作するデリゲート</param>
        public static void ExportToExcelAddSheet(ExcelPackage package, string sheetname, Action<ExcelWorksheet> edit)
        {
            try
            {
                // ワークシートを1枚追加
                var sheet = package.Workbook.Worksheets.Add(sheetname);

                // 追加したシートへの処理
                edit?.Invoke(sheet);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }
    }
}
