using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Serialization;
using System.ServiceModel.Activation;

using Trifolia.Logging;
using Trifolia.Shared;
using TDBImplementationGuide = Trifolia.DB.ImplementationGuide;
using TDBValueSet = Trifolia.DB.ValueSet;
using Trifolia.DB;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;

namespace Trifolia.Terminology
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class VocabularyService : IVocabularyService
    {
        private IObjectRepository tdb;
        private bool isCDA;

        #region Constructors

        public VocabularyService(bool isCDA = true)
            : this(new TemplateDatabaseDataSource(), isCDA)
        {

        }

        public VocabularyService(IObjectRepository tdb, bool isCDA = true)
        {
            this.tdb = tdb;
            this.isCDA = isCDA;
        }

        #endregion

        public int GetValueSetMemberLength(string valueSetOid)
        {
            try
            {
                int count = 0;

                if (tdb.ValueSets.Any(y => y.Oid == valueSetOid))
                {
                    TDBValueSet valueSet = tdb.ValueSets.SingleOrDefault(y => y.Oid == valueSetOid);
                    count = valueSet.Members.Count();
                }

                return count;
            }
            catch (Exception ex)
            {
                Log.For(this).Critical("Error occurred while retrieving valueset member length for valueset oid '" + valueSetOid + "'.", ex);
                throw;
            }
        }

        public string GetValueSet(string valueSetOid, int vocabOutputType, string encoding)
        {
            try
            {
                TDBValueSet valueSet = tdb.ValueSets.SingleOrDefault(y => y.Oid == valueSetOid);

                if (valueSet == null)
                    throw new Exception("Could not find ValueSet specified.");

                VocabularySystems schema = new VocabularySystems();
                schema.Systems = new VocabularySystem[] { GetSystem(tdb, valueSet, DateTime.Now) };
                VocabularyOutputTypeAdapter adapter = new VocabularyOutputTypeAdapter(schema, VocabularyOutputTypeTranslator.FromInt(vocabOutputType), Encoding.GetEncoding(encoding));

                return adapter.AsXML();
            }
            catch (Exception ex)
            {
                Log.For(this).Critical("Error occurred while retrieving a valueset for oid '" + valueSetOid + "'", ex);
                throw;
            }
        }

        public string[] GetValueSetOidsForImplementationGuide(int implementationGuideId)
        {
            try
            {
                TDBImplementationGuide ig = tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);

                if (ig == null)
                    throw new Exception("Could not find ImplementationGuide specified.");

                List<Template> templates = ig.GetRecursiveTemplates(tdb);
                return (from t in templates
                        join tc in tdb.TemplateConstraints on t.Id equals tc.TemplateId
                        join vs in tdb.ValueSets on tc.ValueSetId equals vs.Id
                        where tc.ValueSetId != null
                        select vs).Distinct().Select(vs => vs.Oid).ToArray();
            }
            catch (Exception ex)
            {
                Log.For(this).Critical("Error occurred while retrieving valueset oids for an implementation guide.", ex);
                throw;
            }
        }

        public byte[] GetImplementationGuideVocabularySpreadsheet(int implementationGuideId, int maxValueSetMembers)
        {
            try
            {
                TDBImplementationGuide ig = tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);

                if (ig == null)
                    throw new Exception("Could not find ImplementationGuide specified.");

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
                            XmlEncodeText(cValueSet.ValueSet.Oid));

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
                                XmlEncodeText(cValueSet.ValueSet.Oid),
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

        private string XmlEncodeText(string text)
        {
            if (text == null)
                return text;

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
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

        public string GetImplementationGuideVocabulary(int implementationGuideId, int maxValueSetMembers, int vocabOutputType, string encoding)
        {
            try
            {
                TDBImplementationGuide ig = tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);

                if (ig == null)
                    throw new Exception("Could not find ImplementationGuide specified.");

                bool? onlyStatic = VocabularyOutputTypeTranslator.FromInt(vocabOutputType) != VocabularyOutputType.FHIR ? (bool?)true : null;
                List<ImplementationGuideValueSet> valueSets = ig.GetValueSets(tdb, onlyStatic);
                List<VocabularySystem> systems = new List<VocabularySystem>();

                foreach (ImplementationGuideValueSet cValueSet in valueSets)
                {
                    VocabularySystem newSystem = this.GetSystem(tdb, cValueSet.ValueSet, cValueSet.BindingDate);
                    systems.Add(newSystem);
                }

                VocabularySystems schema = new VocabularySystems();
                schema.Systems = systems.ToArray();
                VocabularyOutputTypeAdapter adapter = new VocabularyOutputTypeAdapter(schema, VocabularyOutputTypeTranslator.FromInt(vocabOutputType), Encoding.GetEncoding(encoding));
                return adapter.AsXML();
            }
            catch (Exception ex)
            {
                Log.For(this).Critical("Error occurred while retrieving vocabulary for an implementation guide.", ex);
                throw;
            }
        }

        public List<ImplementationGuide> GetImplementationGuides()
        {
            try
            {
                return (from ig in tdb.ImplementationGuides
                        select new ImplementationGuide()
                        {
                            Id = ig.Id,
                            Name = ig.Name
                        }).ToList();
            }
            catch (Exception ex)
            {
                Log.For(this).Critical("Error occurred while retrieving list of implementation guides.", ex);
                throw;
            }
        }

        public VocabularyOutputTypeSpecifier[] GetVocabularyOutputTypes()
        {
            int id = 1;
            return System.Enum.GetNames(typeof(VocabularyOutputType)).Select(v => new VocabularyOutputTypeSpecifier() { Id = id++, OutputType = v }).ToArray();
        }

        private VocabularySystem GetSystem(IObjectRepository tdb, TDBValueSet valueSet, DateTime? bindingDate)
        {
            if (valueSet == null)
                throw new Exception("Could not find ValueSet specified.");

            VocabularySystems schema = new VocabularySystems();
            VocabularySystem schemaValueSet = new VocabularySystem()
            {
                ValueSetOid = valueSet.Oid,
                ValueSetName = valueSet.Name
            };

            if (isCDA && schemaValueSet.ValueSetOid.StartsWith("urn:oid:"))
                schemaValueSet.ValueSetOid = schemaValueSet.ValueSetOid.Substring(8);

            schemaValueSet.Codes = GetCodes(valueSet, bindingDate).ToArray();

            return schemaValueSet;
        }

        private List<VocabularyCode> GetCodes(TDBValueSet valueSet, DateTime? bindingDate)
        {
            List<ValueSetMember> members = valueSet.GetActiveMembers(bindingDate);
            List<VocabularyCode> vocabularyCodes = new List<VocabularyCode>();

            foreach (var vc in members)
            {
                VocabularyCode vocabularyCode = new VocabularyCode()
                {
                    Value = vc.Code,
                    DisplayName = vc.DisplayName,
                    CodeSystem = vc.CodeSystem.Oid,
                    CodeSystemName = vc.CodeSystem.Name
                };

                if (this.isCDA && vocabularyCode.CodeSystem.StartsWith("urn:oid:"))
                    vocabularyCode.CodeSystem = vocabularyCode.CodeSystem.Substring(8);

                vocabularyCodes.Add(vocabularyCode);
            }

            return vocabularyCodes;
        }

        public string[] GetSupportedEncodings()
        {
            //TODO: make more dynamic
            return new string[] { "UTF-8", "UTF-7", "UTF-32", "Unicode", "ASCII" };
        }
    }
}
