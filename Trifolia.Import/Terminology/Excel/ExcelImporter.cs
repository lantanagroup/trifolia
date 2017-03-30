using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Logging;

namespace Trifolia.Import.Terminology.Excel
{
    public class ExcelImporter
    {
        private IObjectRepository tdb;

        public ExcelImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public void Import(ImportCheckResponse checkResponse)
        {
            foreach (var checkValueSet in checkResponse.ValueSets)
            {
                Log.For(this).Info("Importing excel value set {0} ({1})", checkValueSet.Name, checkValueSet.Oid);

                ValueSet valueSet = null;

                if (checkValueSet.ChangeType == ImportValueSetChange.ChangeTypes.None || checkValueSet.ChangeType == ImportValueSetChange.ChangeTypes.Update)
                {
                    valueSet = this.tdb.ValueSets.Single(y => y.Id == checkValueSet.Id);

                    if (checkValueSet.ChangeType == ImportValueSetChange.ChangeTypes.Update)
                    {
                        valueSet.Name = checkValueSet.Name;
                        valueSet.LastUpdate = DateTime.Now;
                    }
                }
                else if (checkValueSet.ChangeType == ImportValueSetChange.ChangeTypes.Add)
                {
                    valueSet = new ValueSet()
                    {
                        Name = checkValueSet.Name,
                        LastUpdate = DateTime.Now
                    };

                    ValueSetIdentifier vsIdentifier = new ValueSetIdentifier();
                    vsIdentifier.Identifier = checkValueSet.Oid;

                    if (checkValueSet.Oid.StartsWith("http://") || checkValueSet.Oid.StartsWith("https://"))
                        vsIdentifier.Type = ValueSetIdentifierTypes.HTTP;
                    else if (checkValueSet.Oid.StartsWith("urn:hl7ii:"))
                        vsIdentifier.Type = ValueSetIdentifierTypes.HL7II;
                    else
                        vsIdentifier.Type = ValueSetIdentifierTypes.Oid;

                    valueSet.Identifiers.Add(vsIdentifier);
                    this.tdb.ValueSets.Add(valueSet);
                }

                // Import concepts
                foreach (var checkConcept in checkValueSet.Concepts)
                {
                    this.ExcelImportConcept(valueSet, checkConcept);
                }
            }

            this.tdb.SaveChanges();
        }

        public ImportCheckResponse GetCheckResponse(ImportCheckRequest request)
        {
            ImportCheckResponse response = new ImportCheckResponse();

            using (MemoryStream ms = new MemoryStream(request.Data))
            {
                SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(ms, false);
                WorkbookPart wbPart = spreadsheet.WorkbookPart;
                var sheets = wbPart.Workbook.Descendants<Sheet>();

                if (sheets.Count() != 2)
                {
                    response.Errors.Add("Workbook must contain two spreadsheets. The first representing 'Value Sets', the second representing 'Concepts'");
                    return response;
                }

                var valuesetSheet = sheets.First();
                var conceptSheet = sheets.Last();

                WorksheetPart valuesetWSPart = (WorksheetPart)(wbPart.GetPartById(valuesetSheet.Id));
                Worksheet valuesetWorksheet = valuesetWSPart.Worksheet;
                SheetData valuesetSheetData = valuesetWorksheet.GetFirstChild<SheetData>();

                WorksheetPart conceptWSPart = (WorksheetPart)(wbPart.GetPartById(conceptSheet.Id));
                Worksheet conceptWorksheet = conceptWSPart.Worksheet;
                SheetData conceptSheetData = conceptWorksheet.GetFirstChild<SheetData>();

                ProcessValueSetSheet(response, valuesetSheetData, wbPart, request.FirstRowIsHeader);
                ProcessConceptSheet(response, conceptSheetData, wbPart, request.FirstRowIsHeader);
            }

            return response;
        }

        private void ProcessValueSetSheet(ImportCheckResponse response, SheetData sheetData, WorkbookPart wbPart, bool firstRowIsHeader)
        {
            var rows = sheetData.Descendants<Row>();

            foreach (var row in rows)
            {
                if (firstRowIsHeader && row.RowIndex.Value == 1)
                    continue;

                var cells = row.Descendants<Cell>();

                if (cells.Count() < 2)
                {
                    response.Errors.Add(string.Format("Row {0} on valueset sheet does not have the required number of cells (2)", row.RowIndex.Value));
                    continue;
                }

                Cell nameCell = cells.SingleOrDefault(y => y.CellReference == "A" + row.RowIndex.Value.ToString());
                Cell oidCell = cells.SingleOrDefault(y => y.CellReference == "B" + row.RowIndex.Value.ToString());
                var name = GetCellValue(nameCell, wbPart);
                var identifier = GetCellValue(oidCell, wbPart);

                if (string.IsNullOrEmpty(identifier))
                {
                    response.Errors.Add(string.Format("Row {0} on valueset sheet does not specify an identifier for the value set", row.RowIndex.Value));
                    continue;
                }

                if (!identifier.StartsWith("http://") && !identifier.StartsWith("https://") && !identifier.StartsWith("urn:oid:"))
                {
                    response.Errors.Add(string.Format("Row {0}'s identifier on valueset sheet must be correctly formatted as one of: http[s]://XXXX or urn:oid:XXXX", row.RowIndex.Value));
                    continue;
                }

                ImportValueSetChange change = new ImportValueSetChange();
                ValueSet foundValueSet = (from vs in this.tdb.ValueSets
                                          join vsi in this.tdb.ValueSetIdentifiers on vs.Id equals vsi.ValueSetId
                                          where vsi.Identifier.ToLower().Trim() == identifier.ToLower().Trim()
                                          select vs)
                                          .Distinct()
                                          .FirstOrDefault();

                if (foundValueSet == null)
                {
                    change.ChangeType = ImportValueSetChange.ChangeTypes.Add;
                }
                else
                {
                    change.ValueSet = foundValueSet;
                    change.Id = foundValueSet.Id;

                    if (foundValueSet.Name != name)
                        change.ChangeType = ImportValueSetChange.ChangeTypes.Update;
                }

                change.Oid = identifier;
                change.Name = name;

                response.ValueSets.Add(change);
            }
        }

        private void ProcessConceptSheet(ImportCheckResponse response, SheetData sheetData, WorkbookPart wbPart, bool firstRowIsHeader)
        {
            var rows = sheetData.Descendants<Row>();

            foreach (var row in rows)
            {
                if (firstRowIsHeader && row.RowIndex.Value == 1)
                    continue;

                var cells = row.Descendants<Cell>();

                if (cells.Count() < 6)
                {
                    response.Errors.Add(string.Format("Row {0} on concept sheet does not have the required number of cells (6)", row.RowIndex.Value));
                    continue;
                }

                Cell valuesetOidCell = cells.SingleOrDefault(y => y.CellReference == "A" + row.RowIndex.Value.ToString());
                Cell codeCell = cells.SingleOrDefault(y => y.CellReference == "B" + row.RowIndex.Value.ToString());
                Cell displayCell = cells.SingleOrDefault(y => y.CellReference == "C" + row.RowIndex.Value.ToString());
                Cell codeSystemOidCell = cells.SingleOrDefault(y => y.CellReference == "D" + row.RowIndex.Value.ToString());
                Cell statusCell = cells.SingleOrDefault(y => y.CellReference == "E" + row.RowIndex.Value.ToString());
                Cell statusDateCell = cells.SingleOrDefault(y => y.CellReference == "F" + row.RowIndex.Value.ToString());

                string valuesetOid = GetCellValue(valuesetOidCell, wbPart);
                string code = GetCellValue(codeCell, wbPart);
                string display = GetCellValue(displayCell, wbPart);
                string codeSystemOid = GetCellValue(codeSystemOidCell, wbPart);
                string status = GetCellValue(statusCell, wbPart);
                string statusDateText = GetCellValue(statusDateCell, wbPart);

                if (string.IsNullOrEmpty(valuesetOid))
                {
                    response.Errors.Add(string.Format("Row {0}'s value set identifier on concepts sheet is not identifier", row.RowIndex.Value));
                    continue;
                }

                if (!valuesetOid.StartsWith("http://") && !valuesetOid.StartsWith("https://") && !valuesetOid.StartsWith("urn:oid:"))
                {
                    response.Errors.Add(string.Format("Row {0}'s value set identifier on concepts sheet must be correctly formatted as one of: http[s]://XXXX or urn:oid:XXXX", row.RowIndex.Value));
                    continue;
                }

                if (string.IsNullOrEmpty(codeSystemOid))
                {
                    response.Errors.Add(string.Format("Row {0}'s code system identifier on the concept sheet is not specified", row.RowIndex.Value));
                    continue;
                }

                if (!codeSystemOid.StartsWith("http://") && !codeSystemOid.StartsWith("https://") && !codeSystemOid.StartsWith("urn:oid:"))
                {
                    response.Errors.Add(string.Format("Row {0}'s code system identifier on concepts sheet must be correctly formatted as one of: http[s]://XXXX or urn:oid:XXXX", row.RowIndex.Value));
                    continue;
                }

                var foundValueSetChange = response.ValueSets.SingleOrDefault(y => y.Oid == valuesetOid);
                var foundCodeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Oid == codeSystemOid);

                if (foundValueSetChange == null)
                {
                    response.Errors.Add(string.Format("Row {0} on concept sheet does not have a valid valueset OID.", row.RowIndex.Value));
                    continue;
                }

                if (string.IsNullOrEmpty(code))
                {
                    response.Errors.Add(string.Format("Row {0} on concept sheet does not have a valid code.", row.RowIndex.Value));
                    continue;
                }

                if (string.IsNullOrEmpty(display))
                {
                    response.Errors.Add(string.Format("Row {0} on concept sheet does not have a valid display name.", row.RowIndex.Value));
                    continue;
                }

                if (foundCodeSystem == null)
                {
                    response.Errors.Add(string.Format("Could not find specified code system {0} on row {1} of concept sheet.", codeSystemOid, row.RowIndex.Value));
                    continue;
                }

                if (!string.IsNullOrEmpty(status) && status.ToLower() != "active" && status.ToLower() != "inactive")
                {
                    response.Errors.Add(string.Format("Row {0} on concept sheet does not have a valid status ('active' or 'inactive').", row.RowIndex.Value));
                    continue;
                }

                DateTime parsedStatusDate = DateTime.MinValue;
                int parsedStatusDateInt = 0;
                if (!string.IsNullOrEmpty(statusDateText) && !Int32.TryParse(statusDateText, out parsedStatusDateInt) && !DateTime.TryParse(statusDateText, out parsedStatusDate))
                {
                    response.Errors.Add(string.Format("Row {0} on concept sheet does not have a valid date (format 'mm/dd/yyyy').", row.RowIndex.Value));
                    continue;
                }

                DateTime? statusDate = null;

                if (parsedStatusDateInt > 0)
                    statusDate = FromExcelSerialDate(parsedStatusDateInt);
                else if (parsedStatusDate != DateTime.MinValue)
                    statusDate = parsedStatusDate;

                ImportValueSetChange.ConceptChange conceptChange = new ImportValueSetChange.ConceptChange();
                ValueSetMember foundConcept = null;

                if (foundValueSetChange.ValueSet != null)
                    foundConcept = foundValueSetChange.ValueSet.Members.SingleOrDefault(y => y.Code == code && y.Status == status && y.StatusDate == statusDate);

                if (foundConcept == null)
                {
                    conceptChange.ChangeType = ImportValueSetChange.ChangeTypes.Add;
                }
                else
                {
                    conceptChange.Id = foundConcept.Id;

                    if (foundConcept.DisplayName != display)
                        conceptChange.ChangeType = ImportValueSetChange.ChangeTypes.Update;
                }

                conceptChange.Code = code;
                conceptChange.DisplayName = display;
                conceptChange.CodeSystemOid = codeSystemOid;
                conceptChange.CodeSystemName = foundCodeSystem.Name;
                conceptChange.Status = status;
                conceptChange.StatusDate = statusDate;

                foundValueSetChange.Concepts.Add(conceptChange);
            }
        }

        private static string GetCellValue(Cell cell, WorkbookPart wbPart)
        {
            if (cell == null)
                return string.Empty;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                SharedStringTablePart shareStringPart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
                SharedStringItem[] items = shareStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                return items[int.Parse(cell.CellValue.Text)].InnerText;
            }

            return cell.CellValue.Text;
        }

        public static DateTime FromExcelSerialDate(int SerialDate)
        {
            if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug   
            return new DateTime(1899, 12, 31).AddDays(SerialDate);
        }

        private void ExcelImportConcept(ValueSet valueSet, ImportValueSetChange.ConceptChange checkConcept)
        {
            ValueSetMember member = null;

            if (checkConcept.ChangeType == ImportValueSetChange.ChangeTypes.None)
                return;

            if (checkConcept.ChangeType == ImportValueSetChange.ChangeTypes.Update)
            {
                member = this.tdb.ValueSetMembers.Single(y => y.Id == checkConcept.Id);

                if (member.DisplayName != checkConcept.DisplayName)
                    member.DisplayName = checkConcept.DisplayName;
            }
            else // Add
            {
                CodeSystem codeSystem = this.tdb.CodeSystems.Single(y => y.Oid == checkConcept.CodeSystemOid);

                member = new ValueSetMember()
                {
                    ValueSet = valueSet,
                    Code = checkConcept.Code,
                    DisplayName = checkConcept.DisplayName,
                    CodeSystem = codeSystem,
                    Status = checkConcept.Status,
                    StatusDate = checkConcept.StatusDate
                };
                this.tdb.ValueSetMembers.Add(member);
            }
        }
    }
}
