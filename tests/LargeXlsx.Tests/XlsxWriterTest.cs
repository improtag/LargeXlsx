﻿using System.IO;
using FluentAssertions;
using NUnit.Framework;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace LargeXlsx.Tests
{
    [TestFixture]
    public class XlsxWriterTest
    {
        [Test]
        public void InsertionPoint()
        {
            using (var stream = new MemoryStream())
            using (var xlsxWriter = new XlsxWriter(stream))
            {
                xlsxWriter.BeginWorksheet("Sheet1")
                    .BeginRow().Write("A1").Write("B1")
                    .BeginRow().Write("A2");

                xlsxWriter.CurrentRowNumber.Should().Be(2);
                xlsxWriter.CurrentColumnNumber.Should().Be(2);
            }
        }

        [Test]
        public void InsertionPointAfterSkipColumn()
        {
            using (var stream = new MemoryStream())
            using (var xlsxWriter = new XlsxWriter(stream))
            {
                xlsxWriter.BeginWorksheet("Sheet1")
                    .BeginRow().Write("A1").Write("B1")
                    .BeginRow().Write("A2").SkipColumns(2);

                xlsxWriter.CurrentRowNumber.Should().Be(2);
                xlsxWriter.CurrentColumnNumber.Should().Be(4);
            }
        }

        [Test]
        public void InsertionPointAfterSkipRows()
        {
            using (var stream = new MemoryStream())
            using (var xlsxWriter = new XlsxWriter(stream))
            {
                xlsxWriter.BeginWorksheet("Sheet1")
                    .BeginRow().Write("A1").Write("B1")
                    .SkipRows(2);

                xlsxWriter.CurrentRowNumber.Should().Be(3);
                xlsxWriter.CurrentColumnNumber.Should().Be(0);
            }
        }

        [Test]
        public void Simple()
        {
            using (var stream = new MemoryStream())
            {
                using (var xlsxWriter = new XlsxWriter(stream))
                {
                    var whiteFont = xlsxWriter.Stylesheet.CreateFont("Calibri", 11, "ffffff", bold: true);
                    var blueFill = xlsxWriter.Stylesheet.CreateSolidFill("004586");
                    var headerStyle = xlsxWriter.Stylesheet.CreateStyle(whiteFont, blueFill, XlsxBorder.None, XlsxNumberFormat.General);

                    xlsxWriter.BeginWorksheet("Sheet1")
                        .BeginRow().Write("Col1", headerStyle).Write("Col2", headerStyle).Write("Col3", headerStyle)
                        .BeginRow().Write(headerStyle).Write("Sub2", headerStyle).Write("Sub3", headerStyle)
                        .BeginRow().Write("Row3").Write(42).Write(-1)
                        .BeginRow().Write("Row4").SkipColumns(1).Write(1234)
                        .SkipRows(2)
                        .BeginRow().AddMergedCell(1, 2).Write("Row7").SkipColumns(1).Write(3.14159265359);
                }

                using (var package = new ExcelPackage(stream))
                {
                    package.Workbook.Worksheets.Count.Should().Be(1);
                    var sheet = package.Workbook.Worksheets[0];
                    sheet.Name.Should().Be("Sheet1");

                    sheet.Cells["A1"].Value.Should().Be("Col1");
                    sheet.Cells["B1"].Value.Should().Be("Col2");
                    sheet.Cells["C1"].Value.Should().Be("Col3");
                    sheet.Cells["A2"].Value.Should().BeNull();
                    sheet.Cells["B2"].Value.Should().Be("Sub2");
                    sheet.Cells["C2"].Value.Should().Be("Sub3");
                    sheet.Cells["A3"].Value.Should().Be("Row3");
                    sheet.Cells["B3"].Value.Should().Be(42);
                    sheet.Cells["C3"].Value.Should().Be(-1);
                    sheet.Cells["A4"].Value.Should().Be("Row4");
                    sheet.Cells["B4"].Value.Should().BeNull();
                    sheet.Cells["C4"].Value.Should().Be(1234);
                    sheet.Cells["A5"].Value.Should().BeNull();
                    sheet.Cells["A6"].Value.Should().BeNull();
                    sheet.Cells["A7"].Value.Should().Be("Row7");
                    sheet.Cells["B7"].Value.Should().BeNull();
                    sheet.Cells["C7"].Value.Should().Be(3.14159265359);

                    sheet.Cells["A7:B7"].Merge.Should().BeTrue();

                    sheet.Cells["A1:C2"].Style.Fill.PatternType.Should().Be(ExcelFillStyle.Solid);
                    sheet.Cells["A1:C2"].Style.Fill.BackgroundColor.Rgb.Should().Be("004586");
                    sheet.Cells["A1:C2"].Style.Font.Bold.Should().BeTrue();
                    sheet.Cells["A1:C2"].Style.Font.Color.Rgb.Should().Be("ffffff");
                    sheet.Cells["A1:C2"].Style.Font.Name.Should().Be("Calibri");
                    sheet.Cells["A1:C2"].Style.Font.Size.Should().Be(11);

                    sheet.Cells["A3:C7"].Style.Fill.PatternType.Should().Be(ExcelFillStyle.None);
                    sheet.Cells["A3:C7"].Style.Font.Bold.Should().BeFalse();
                    sheet.Cells["A3:C7"].Style.Font.Color.Rgb.Should().Be("000000");
                    sheet.Cells["A3:C7"].Style.Font.Name.Should().Be("Calibri");
                    sheet.Cells["A3:C7"].Style.Font.Size.Should().Be(11);
                }
            }
        }
    }
}