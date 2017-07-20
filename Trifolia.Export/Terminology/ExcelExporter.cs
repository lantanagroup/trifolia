using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trifolia.DB;
using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using TDBImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Export.Terminology
{
    public class ExcelExporter
    {
        private IObjectRepository tdb;

        public ExcelExporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public byte[] GetSpreadsheet(int implementationGuideId, int maxValueSetMembers)
        {
            try
            {
                TDBImplementationGuide ig = tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);

                if (ig == null)
                    throw new Exception("Could not find ImplementationGuide specified.");

                var igTypePlugin = ig.ImplementationGuideType.GetPlugin();
                List<ImplementationGuideValueSet> valueSets = ig.GetValueSets(tdb);

                using (MemoryStream ms = new MemoryStream())
                {
                    SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true);

                    WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();
                    workbookpart.Workbook.AppendChild<Sheets>(new Sheets());

                    Worksheet sheet1 = CreateWorksheet(workbookpart.Workbook, "Affected Value Sets");
                    SheetData sheet1Data = sheet1.GetFirstChild<SheetData>();
                    Worksheet sheet2 = CreateWorksheet(workbookpart.Workbook, "Value Set Members");
                    SheetData sheet2Data = sheet2.GetFirstChild<SheetData>();
                    int sheet1Count = 2;
                    int sheet2Count = 2;

                    // Sheet 1 column widths
                    Columns sheet1Cols = new Columns(
                        new Column()
                        {
                            Min = 1,
                            Max = 1,
                            Width = 56,
                            BestFit = BooleanValue.FromBoolean(true),
                            CustomWidth = BooleanValue.FromBoolean(true)
                        },
                        new Column()
                        {
                            Min = 2,
                            Max = 2,
                            Width = 38,
                            BestFit = BooleanValue.FromBoolean(true),
                            CustomWidth = BooleanValue.FromBoolean(true)
                        });
                    sheet1.InsertBefore<Columns>(sheet1Cols, sheet1Data);

                    // Sheet 2 column widths
                    Columns sheet2Cols = new Columns(
                        new Column()
                        {
                            Min = 1,
                            Max = 1,
                            Width = 32,
                            BestFit = BooleanValue.FromBoolean(true),
                            CustomWidth = BooleanValue.FromBoolean(true)
                        },
                        new Column()
                        {
                            Min = 2,
                            Max = 2,
                            Width = 51,
                            BestFit = BooleanValue.FromBoolean(true),
                            CustomWidth = BooleanValue.FromBoolean(true)
                        },
                        new Column()
                        {
                            Min = 3,
                            Max = 3,
                            Width = 26,
                            BestFit = BooleanValue.FromBoolean(true),
                            CustomWidth = BooleanValue.FromBoolean(true)
                        },
                        new Column()
                        {
                            Min = 4,
                            Max = 4,
                            Width = 39,
                            BestFit = BooleanValue.FromBoolean(true),
                            CustomWidth = BooleanValue.FromBoolean(true)
                        },
                        new Column()
                        {
                            Min = 5,
                            Max = 5,
                            Width = 23,
                            BestFit = BooleanValue.FromBoolean(true),
                            CustomWidth = BooleanValue.FromBoolean(true)
                        });
                    sheet2.InsertBefore<Columns>(sheet2Cols, sheet2Data);

                    // Add headers
                    sheet1Data.AppendChild(
                        new Row(
                            new Cell(
                                new CellValue("Value Set Name"))
                            {
                                CellReference = "A1",
                                DataType = CellValues.String
                            },
                            new Cell(
                                new CellValue("Value Set OID"))
                            {
                                CellReference = "B1",
                                DataType = CellValues.String
                            })
                        {
                            RowIndex = 1
                        });
                    sheet2Data.AppendChild(
                        new Row(
                            new Cell(
                                new CellValue("Value Set OID"))
                            {
                                CellReference = "A1",
                                DataType = CellValues.String
                            },
                            new Cell(
                                new CellValue("Value Set Name"))
                            {
                                CellReference = "B1",
                                DataType = CellValues.String
                            },
                            new Cell(
                                new CellValue("Code"))
                            {
                                CellReference = "C1",
                                DataType = CellValues.String
                            },
                            new Cell(
                                new CellValue("Display Name"))
                            {
                                CellReference = "D1",
                                DataType = CellValues.String
                            },
                            new Cell(
                                new CellValue("Code System Name"))
                            {
                                CellReference = "E1",
                                DataType = CellValues.String
                            })
                        {
                            RowIndex = 1
                        });

                    for (int x = 0; x < valueSets.Count; x++)
                    {
                        var cValueSet = valueSets[x];
                        string summaryXml = string.Format(
                            "<row r=\"{0}\" xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                            "  <c r=\"A{0}\" t=\"str\"><v>{1}</v></c>" +
                            "  <c r=\"B{0}\" t=\"str\"><v>{2}</v></c>" +
                            "</row>",
                            sheet1Count++,
                            XmlEncodeText(cValueSet.ValueSet.Name),
                            XmlEncodeText(cValueSet.ValueSet.GetIdentifier(igTypePlugin)));

                        Row newSummaryRow = new Row(summaryXml);
                        sheet1Data.AppendChild(newSummaryRow);

                        List<ValueSetMember> members = cValueSet.ValueSet.GetActiveMembers(cValueSet.BindingDate);

                        for (int i = 0; i < members.Count && (maxValueSetMembers == 0 || i < maxValueSetMembers); i++)
                        {
                            var cMember = members[i];

                            string memberXml = string.Format(
                                "<row r=\"{0}\" xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                                "  <c r=\"A{0}\" t=\"str\"><v>{1}</v></c>" +
                                "  <c r=\"B{0}\" t=\"str\"><v>{2}</v></c>" +
                                "  <c r=\"C{0}\" t=\"str\"><v>{3}</v></c>" +
                                "  <c r=\"D{0}\" t=\"str\"><v>{4}</v></c>" +
                                "  <c r=\"E{0}\" t=\"str\"><v>{5}</v></c>" +
                                "</row>",
                                sheet2Count++,
                                XmlEncodeText(cValueSet.ValueSet.GetIdentifier(igTypePlugin)),
                                XmlEncodeText(cValueSet.ValueSet.Name),
                                XmlEncodeText(cMember.Code),
                                XmlEncodeText(cMember.DisplayName),
                                XmlEncodeText(cMember.CodeSystem.Name));

                            Row newMemberRow = new Row(memberXml);
                            sheet2Data.AppendChild(newMemberRow);
                        }
                    }

                    workbookpart.Workbook.Save();
                    spreadsheet.Close();

                    ms.Position = 0;
                    byte[] buffer = new byte[ms.Length];
                    ms.Read(buffer, 0, (int)ms.Length);
                    return buffer;
                }
            }
            catch (Exception ex)
            {
                Log.For(this).Critical("Error occurred while generating spreadsheet for IG vocabulary", ex);
                throw;
            }
        }

        private Worksheet CreateWorksheet(Workbook workbook, string name)
        {
            SheetData newSheetData = new SheetData();
            WorksheetPart newWorksheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(newSheetData);

            Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = workbook.WorkbookPart.GetIdOfPart(newWorksheetPart);

            // Get a unique ID for the new sheet.
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            Sheet newSheet = new Sheet()
            {
                Id = relationshipId,
                Name = name,
                SheetId = sheetId
            };

            sheets.Append(newSheet);

            return newWorksheetPart.Worksheet;
        }

        private string XmlEncodeText(string text)
        {
            if (text == null)
                return text;

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}
