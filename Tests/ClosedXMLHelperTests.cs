using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeneralUtility;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ClosedXML.Attributes;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class ClosedXMLHelperTest
    {
        [TestMethod, TestCategory("GeneralUtility/ClosedXMLHelper")]
        public void Open()
        {
            var fileName = "UTest_ClosedXMLHelper.xlsx";
            if (File.Exists(fileName)) File.Delete(fileName);
            // TEST || GeneralUtility\ClosedXMLHelper.cs || ClosedXMLHelper.Open() || ファイルが存在しない場合にOpen可能なこと。(メモリ上に生成)
            using (var excel = new ClosedXMLHelper(fileName, true))
            {
                excel.FileName.Is(fileName);
                excel.Workbook.IsNotNull();
            }
            File.Exists(fileName).IsFalse();

            // TEST || GeneralUtility\ClosedXMLHelper.cs || ClosedXMLHelper.Open() || ファイルが存在する場合にOpen可能なこと。(既存ファイル)
            using (var excel = new ClosedXMLHelper(fileName, true))
            {
                IXLWorksheet sheet;
                if (!excel.Workbook.TryGetWorksheet("UTestSheet", out sheet))
                    sheet = excel.Workbook.Worksheets.Add("UTestSheet");
                excel.Save();
            }
            using (var excel = new ClosedXMLHelper(fileName, true))
            {
                excel.FileName.Is(fileName);
                excel.Workbook.IsNotNull();
            }
            File.Exists(fileName).IsTrue();
        }


        [TestMethod, TestCategory("GeneralUtility/ClosedXMLHelper")]
        public void Save()
        {
            var fileName = "UTest_ClosedXMLHelper.xlsx";
            if (File.Exists(fileName)) File.Delete(fileName);
            // TEST || GeneralUtility\ClosedXMLHelper.cs || ClosedXMLHelper.Save() || ファイルが存在しない場合にSave可能なこと。(新規作成)
            using (var excel = new ClosedXMLHelper(fileName, true))
            {
                IXLWorksheet sheet;
                if (!excel.Workbook.TryGetWorksheet("UTestSheet", out sheet))
                    sheet = excel.Workbook.Worksheets.Add("UTestSheet");
                excel.Save();
            }
            File.Exists(fileName).IsTrue();

            // TEST || GeneralUtility\ClosedXMLHelper.cs || ClosedXMLHelper.Save() || ファイルが存在する場合Save可能なこと。(上書保存)
            using (var excel = new ClosedXMLHelper(fileName, true))
            {
                IXLWorksheet sheet;
                if (!excel.Workbook.TryGetWorksheet("UTestSheet", out sheet))
                    sheet = excel.Workbook.Worksheets.Add("UTestSheet");
                excel.Save();
            }
            File.Exists(fileName).IsTrue();
        }


        [TestMethod, TestCategory("GeneralUtility/ClosedXMLHelper")]
        public void GetTable()
        {
            var fileName = "UTest_ClosedXMLHelper.xlsx";
            if (File.Exists(fileName)) File.Delete(fileName);
            using (var excel = new ClosedXMLHelper(fileName, true))
            {
                IXLWorksheet sheet;
                if (!excel.Workbook.TryGetWorksheet("UTestSheet", out sheet))
                    sheet = excel.Workbook.Worksheets.Add("UTestSheet");
                var list = new List<UTestForExcel> {
                    new UTestForExcel{ Server = "Server1", Name = "Name1" },
                    new UTestForExcel{ Server = "Server2", Name = "Name2" },
                };
                var table = sheet.Cell(1, 1).InsertTable(list, true);
                table.Name = "UTestTable";
                table.Theme = XLTableTheme.TableStyleMedium13;

                // TEST || GeneralUtility\ClosedXMLHelper.cs || ClosedXMLHelper.GetTable() || テーブルを取得すること
                var xlTable = excel.GetTable("UTestTable");
                xlTable.Fields.ToList()[0].DataCells.ToList()[0].Value.Is("Server1");
                xlTable.Fields.ToList()[0].DataCells.ToList()[1].Value.Is("Server2");
                xlTable.Fields.ToList()[1].DataCells.ToList()[0].Value.Is("Name1");
                xlTable.Fields.ToList()[1].DataCells.ToList()[1].Value.Is("Name2");
            }
        }


        //[TestMethod, TestCategory("GeneralUtility/ClosedXMLHelper")]
        //public void ToFormattedStringDataTable()
        //{
        //    var fileName = "UTest_ClosedXMLHelper.xlsx";
        //    if (File.Exists(fileName)) File.Delete(fileName);
        //    using (var excel = new ClosedXMLHelper(fileName, true))
        //    {
        //        IXLWorksheet sheet;
        //        if (!excel.Workbook.TryGetWorksheet("UTestSheet", out sheet))
        //            sheet = excel.Workbook.Worksheets.Add("UTestSheet");
        //        var list = new List<UTestForExcel> {
        //            new UTestForExcel{ Server = "Server1", Name = "Name1" },
        //            new UTestForExcel{ Server = "Server2", Name = "Name2" },
        //        };
        //        var table = sheet.Cell(1, 1).InsertTable(list, true);
        //        table.Name = "UTestTable";
        //        table.Theme = XLTableTheme.TableStyleMedium13;

        //        // TEST || GeneralUtility\ClosedXMLHelper.cs || ClosedXMLHelper.AsDataTable() || テーブルをDataTableとして取得すること
        //        var xlTable = excel.GetTable("UTestTable");
        //        var dbTable = xlTable.ToFormattedStringDataTable();
        //        dbTable.Columns[0].ColumnName.Is("サーバー");
        //        dbTable.Columns[1].ColumnName.Is("名称");
        //        dbTable.Rows[0][0].Is("Server1");
        //        dbTable.Rows[0][1].Is("Name1");
        //        dbTable.Rows[1][0].Is("Server2");
        //        dbTable.Rows[1][1].Is("Name2");
        //    }
        //}

        //[TestMethod, TestCategory("GeneralUtility/ClosedXMLHelper")]
        //public void ToEntity()
        //{
        //    var fileName = Path.GetFullPath(@"!FakeData\UTestDataPattern.xlsx");
        //    using (var excel = new ClosedXMLHelper(fileName))
        //    {
        //        var xlTable = excel.GetTable("RepoSel_KintaiEntity");
        //        var dbt = xlTable.ToFormattedStringDataTable();
        //        var a = dbt.AsEnumerable().Select(s => s.ToEntity(p => new KintaiEntity(p))).ToList();
        //        var b = dbt.AsEnumerable().Select(s => s.ToDictionary()).ToList();
        //        var c = b.Where(w => w["対象"].ToString() == "o").Select(s => new KintaiEntity(s)).ToList();
        //    }
        //}


        public class UTestForExcel
        {
            [XLColumn(Header = "サーバー")]
            public string Server { get; set; }
            [XLColumn(Header = "名称")]
            public string Name { get; set; }
        }
    }
}
