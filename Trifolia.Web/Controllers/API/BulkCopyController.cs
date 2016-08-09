using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;
using System.Xml.Serialization;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;

using Trifolia.Authorization;
using Trifolia.Web.Models.BulkCopy;
using Trifolia.DB;
using Trifolia.Web.Extensions;
using System.Web.Http.Description;

namespace Trifolia.Web.Controllers.API
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BulkCopyController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public BulkCopyController()
            : this(DBContext.Create())
        {

        }

        public BulkCopyController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        #region Parsing

        [HttpPost, Route("api/BulkCopy/Parse")]
        public List<ExcelSheet> Parse(ExcelUpload uploadModel)
        {
            List<ExcelSheet> returnSheets = new List<ExcelSheet>();

            using (MemoryStream ms = new MemoryStream(uploadModel.ExcelFile))
            {
                SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(ms, false);
                WorkbookPart wbPart = spreadsheet.WorkbookPart;
                var sheets = wbPart.Workbook.Descendants<Sheet>();

                foreach (Sheet sheet in sheets)
                {
                    ExcelSheet newReturnSheet = new ExcelSheet();
                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(sheet.Id));
                    Worksheet worksheet = wsPart.Worksheet;
                    SheetData sheetData = worksheet.GetFirstChild<SheetData>();

                    newReturnSheet.SheetName = sheet.Name;

                    var firstRowCells = worksheet.Descendants<Cell>().Where(y => GetCellRow(y.CellReference.Value) == 1);

                    foreach (var firstRowCell in firstRowCells)
                    {
                        ExcelColumn newReturnColumn = new ExcelColumn()
                        {
                            Letter = GetCellLetter(firstRowCell.CellReference.Value),
                            Name = GetCellLetter(firstRowCell.CellReference.Value)
                        };

                        if (uploadModel.FirstRowIsHeader)
                            newReturnColumn.Name = GetCellValue(firstRowCell, wbPart);

                        newReturnSheet.Columns.Add(newReturnColumn);
                    }

                    var rows = sheetData.Descendants<Row>();

                    foreach (var row in rows)
                    {
                        ExcelRow newReturnRow = new ExcelRow()
                        {
                            RowNumber = (int)row.RowIndex.Value
                        };
                        
                        if (uploadModel.FirstRowIsHeader && newReturnRow.RowNumber == 1)
                            continue;

                        foreach (ExcelColumn column in newReturnSheet.Columns)
                        {
                            var cellReference = string.Format("{0}{1}", column.Letter, newReturnRow.RowNumber);
                            Cell cell = worksheet.Descendants<Cell>().SingleOrDefault(y => y.CellReference.Value == cellReference);
                            ExcelCell newReturnCell = new ExcelCell()
                            {
                                Letter = column.Letter,
                                Value = GetCellValue(cell, wbPart)
                            };

                            newReturnRow.Cells.Add(newReturnCell);
                        }

                        newReturnSheet.Rows.Add(newReturnRow);
                    }

                    returnSheets.Add(newReturnSheet);
                }
            }

            return returnSheets;
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

        private static string GetCellLetter(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);

            return match.Value;
        }

        // Given a cell name, parses the specified cell to get the row index.
        private static uint GetCellRow(string cellReference)
        {
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(cellReference);

            return uint.Parse(match.Value);
        }

        #endregion

        #region Copy

        [HttpPost, Route("api/BulkCopy/Copy"), SecurableAction(SecurableNames.TEMPLATE_COPY)]
        public CopyResults Copy(CopyModel model)
        {
            CopyResults results = new CopyResults();
            Template baseTemplate = this.tdb.Templates.Single(y => y.Id == model.BaseTemplateId);
            List<Template> templates = new List<Template>();

            if (!CheckPoint.Instance.GrantEditImplementationGuide(baseTemplate.OwningImplementationGuideId))
                throw new AuthorizationException("You do not have permission to modify this implementation guide");

            foreach (var templateModel in model.TemplateSheet.Rows)
            {
                string templateOid = GetRowField(model.TemplateSheet, templateModel, Fields.TemplateOid);
                var foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == templateOid);

                // If a template with the same OID already exists, try to delete it
                if (foundTemplate != null)
                {
                    if (foundTemplate.OwningImplementationGuide.IsPublished())
                    {
                        results.Errors.Add("Template " + templateOid + " already exists and cannot be re-created because it is part of an implementation guide that has already been published.");
                        continue;
                    }

                    if (!CheckPoint.Instance.GrantEditTemplate(foundTemplate.Id))
                    {
                        results.Errors.Add("Template " + templateOid + " already exists and cannot be re-created because you do not have permission to edit the template.");
                        continue;
                    }

                    this.DeleteTemplate(foundTemplate);
                }

                var user = CheckPoint.Instance.GetUser(this.tdb);

                // Create the new template
                var newTemplate = new Template()
                {
                    Name = GetRowField(model.TemplateSheet, templateModel, Fields.TemplateName),
                    Oid = templateOid,
                    Bookmark = GetRowField(model.TemplateSheet, templateModel, Fields.TemplateBookmark),
                    Description = GetRowField(model.TemplateSheet, templateModel, Fields.TemplateDescription),
                    Author = user,
                    PrimaryContext = baseTemplate.PrimaryContext,
                    PrimaryContextType = baseTemplate.PrimaryContextType,
                    ImplementationGuideType = baseTemplate.ImplementationGuideType,
                    ImpliedTemplate = baseTemplate.ImpliedTemplate,
                    IsOpen = baseTemplate.IsOpen,
                    Notes = baseTemplate.Notes,
                    OwningImplementationGuide = baseTemplate.OwningImplementationGuide,
                    Status = baseTemplate.Status,
                    TemplateType = baseTemplate.TemplateType
                };

                this.tdb.Templates.AddObject(newTemplate);
                results.NewTemplates.Add(new CopyResults.TemplateEntry()
                {
                    Name = newTemplate.Name,
                    Oid = newTemplate.Oid,
                    Url = "/TemplateManagement/View/" + templateOid
                });

                var changes = model.ConstraintSheet.Rows.Where(y => GetRowField(model.ConstraintSheet, y, Fields.ConstraintTemplate) == newTemplate.Oid);

                this.CopyAndChangeConstraints(baseTemplate, newTemplate, model.ConstraintSheet, changes, results);
            }

            if (results.Errors.Count == 0)
            {
                try
                {
                    this.tdb.SaveChanges();
                }
                catch (Exception ex)
                {
                    results.Errors.Add("System error: " + ex.Message);
                }
            }
            else
            {
                results.NewTemplates.Clear();
            }

            return results;
        }

        private void CopyAndChangeConstraints(Template baseTemplate, Template destinationTemplate, ExcelSheet constraintsSheet, IEnumerable<ExcelRow> changes, CopyResults results, TemplateConstraint baseParentConstraint = null, TemplateConstraint destinationParentConstraint = null)
        {
            var baseConstraints = baseParentConstraint != null ?
                baseParentConstraint.ChildConstraints.ToList() :
                baseTemplate.ChildConstraints.Where(y => y.ParentConstraintId == null).ToList();

            foreach (var baseConstraint in baseConstraints)
            {
                TemplateConstraint newConstraint = new TemplateConstraint()
                {
                    Cardinality = baseConstraint.Cardinality,
                    CodeSystemId = baseConstraint.CodeSystemId,
                    Conformance = baseConstraint.Conformance,
                    ContainedTemplateId = baseConstraint.ContainedTemplateId,
                    Context = baseConstraint.Context,
                    DataType = baseConstraint.DataType,
                    Description = baseConstraint.Description,
                    DisplayName = baseConstraint.DisplayName,
                    HeadingDescription = baseConstraint.HeadingDescription,
                    IsBranch = baseConstraint.IsBranch,
                    IsBranchIdentifier = baseConstraint.IsBranchIdentifier,
                    IsHeading = baseConstraint.IsHeading,
                    IsInheritable = baseConstraint.IsInheritable,
                    IsSchRooted = baseConstraint.IsSchRooted,
                    IsPrimitive = baseConstraint.IsPrimitive,
                    IsStatic = baseConstraint.IsStatic,
                    Label = baseConstraint.Label,
                    Notes = baseConstraint.Notes,
                    Order = baseConstraint.Order,
                    PrimitiveText = baseConstraint.PrimitiveText,
                    Schematron = baseConstraint.Schematron,
                    Value = baseConstraint.Value,
                    ValueConformance = baseConstraint.ValueConformance,
                    ValueSetId = baseConstraint.ValueSetId,
                    ValueSetDate = baseConstraint.ValueSetDate
                };

                // Find changes for this constraint
                var constraintChanges = changes.Where(y => GetRowField(constraintsSheet, y, Fields.ConstraintNumber) == baseConstraint.Number.ToString());

                foreach (var constraintChange in constraintChanges)
                {
                    string dataType = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintDataType);
                    string conformance = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintConformance);
                    string cardinality = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintCardinality);
                    string valueConformance = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintValueConformance);
                    string code = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintCode);
                    string display = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintDisplay);
                    string valueSet = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintValueSet);
                    string valueSetDate = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintValueSetDate);
                    string codeSystem = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintCodeSystem);
                    string containedTemplate = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintContainedTemplate);
                    string description = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintDescription);
                    string label = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintLabel);
                    string binding = GetRowField(constraintsSheet, constraintChange, Fields.ConstraintBinding);

                    if (!string.IsNullOrEmpty(conformance) && conformance != "SHALL" && conformance != "SHOULD" && conformance != "MAY" && conformance != "SHALL NOT" && conformance != "SHOULD NOT" && conformance != "MAY NOT")
                        results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " has an invalid conformance value. Valid options are: SHALL, SHOULD, MAY, SHALL NOT, SHOULD NOT, MAY NOT");

                    if (!string.IsNullOrEmpty(valueConformance) && valueConformance != "SHALL" && valueConformance != "SHOULD" && valueConformance != "MAY" && valueConformance != "SHALL NOT" && valueConformance != "SHOULD NOT" && valueConformance != "MAY NOT")
                        results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " has an invalid value-conformance value. Valid options are: SHALL, SHOULD, MAY, SHALL NOT, SHOULD NOT, MAY NOT");

                    if (!string.IsNullOrEmpty(cardinality) && !System.Text.RegularExpressions.Regex.IsMatch(cardinality, @"^(\d*|\*)\.\.(\d*|\*)$"))
                        results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " has an invalid format. Format should be \"X..Y\"");

                    if (!string.IsNullOrEmpty(binding) && binding.ToLower() != "<remove>" && binding.ToLower() != "static" && binding.ToLower() != "dynamic")
                        results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " has an invalid binding type. Valid values are \"STATIC\" and \"DYNAMIC\"");

                    SetConstraintStringValue(newConstraint, "DataType", dataType);
                    SetConstraintStringValue(newConstraint, "Conformance", conformance);
                    SetConstraintStringValue(newConstraint, "Cardinality", cardinality);
                    SetConstraintStringValue(newConstraint, "DataType", dataType);
                    SetConstraintStringValue(newConstraint, "Value", code);
                    SetConstraintStringValue(newConstraint, "DisplayName", display);
                    SetConstraintStringValue(newConstraint, "Description", description);
                    SetConstraintStringValue(newConstraint, "Label", label);

                    // ValueSet
                    if (!string.IsNullOrEmpty(valueSet))
                    {
                        if (valueSet.ToLower() == "<remove>")
                        {
                            newConstraint.ValueSetId = null;
                        }
                        else
                        {
                            ValueSet foundValueSet = this.tdb.ValueSets.SingleOrDefault(y => y.Oid == valueSet);

                            if (foundValueSet == null)
                                results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " defines a value set \"" + valueSet + "\" which could not be found.");
                            else
                                newConstraint.ValueSetId = foundValueSet.Id;
                        }
                    }

                    // Value Set Date
                    if (!string.IsNullOrEmpty(valueSetDate))
                    {
                        if (valueSetDate.ToLower() == "<remove>")
                        {
                            newConstraint.ValueSetDate = null;
                        }
                        else
                        {
                            DateTime parsedValueSetDate = DateTime.MinValue;

                            if (!DateTime.TryParse(valueSetDate, out parsedValueSetDate))
                                results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " has an invalid date/time format for the value set date");
                            else
                                newConstraint.ValueSetDate = parsedValueSetDate;
                        }
                    }

                    // CodeSystem
                    if (!string.IsNullOrEmpty(codeSystem))
                    {
                        if (valueSet.ToLower() == "<remove>")
                        {
                            newConstraint.CodeSystemId = null;
                        }
                        else
                        {
                            CodeSystem foundCodeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Oid == codeSystem);

                            if (foundCodeSystem == null)
                                results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " defines a code system \"" + codeSystem + "\" which could not be found.");
                            else
                                newConstraint.CodeSystemId = foundCodeSystem.Id;
                        }
                    }

                    // Contained Template
                    if (!string.IsNullOrEmpty(containedTemplate))
                    {
                        if (containedTemplate.ToLower() == "<remove>")
                        {
                            newConstraint.ContainedTemplateId = null;
                        }
                        else
                        {
                            Template foundContainedTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == containedTemplate);

                            if (foundContainedTemplate == null)
                                results.Errors.Add("Constraint change #" + constraintChange.RowNumber.ToString() + " defines a contained template \"" + containedTemplate + "\" which could not be found.");
                            else
                                newConstraint.ContainedTemplateId = foundContainedTemplate.Id;
                        }
                    }

                    // Binding
                    if (!string.IsNullOrEmpty(binding))
                    {
                        if (binding.ToLower() == "<remove>")
                            newConstraint.IsStatic = null;
                        else if (binding.ToLower() == "static")
                            newConstraint.IsStatic = true;
                        else if (binding.ToLower() == "dynamic")
                            newConstraint.IsStatic = false;
                    }
                }
                
                // Add the constraint to the template and to the correct parent (if one is specified)
                if (destinationParentConstraint != null)
                    destinationParentConstraint.ChildConstraints.Add(newConstraint);
                destinationTemplate.ChildConstraints.Add(newConstraint);

                // Recursively copy additional constraints
                CopyAndChangeConstraints(baseTemplate, destinationTemplate, constraintsSheet, changes, results, baseConstraint, newConstraint);
            }
        }

        private void SetConstraintStringValue(TemplateConstraint constraint, string propertyName, string value)
        {
            System.Reflection.PropertyInfo property = constraint.GetType().GetProperty(propertyName);

            if (!string.IsNullOrEmpty(value))
            {
                if (value == "<REMOVE>")
                    property.SetValue(constraint, null);
                else
                    property.SetValue(constraint, value);
            }
        }

        private void DeleteTemplate(Template template)
        {
            template.GreenTemplates.ToList().ForEach(y =>
            {
                y.ChildGreenConstraints.ToList().ForEach(x => this.tdb.GreenConstraints.DeleteObject(x));
                this.tdb.GreenTemplates.DeleteObject(y);
            });

            template.ChildConstraints.ToList().ForEach(y =>
            {
                y.Samples.ToList().ForEach(x => this.tdb.TemplateConstraintSamples.DeleteObject(x));
                this.tdb.TemplateConstraints.DeleteObject(y);
            });

            template.TemplateSamples.ToList().ForEach(y => this.tdb.TemplateSamples.DeleteObject(y));

            this.tdb.Templates.DeleteObject(template);
        }

        private static string GetRowField(ExcelSheet sheet, ExcelRow row, Fields field)
        {
            ExcelColumn column = sheet.Columns.SingleOrDefault(y => y.MappedField == field);

            if (column == null)
                return null;

            ExcelCell cell = row.Cells.SingleOrDefault(y => y.Letter == column.Letter);

            if (cell == null)
                return null;

            return cell.Value.Trim();
        }

        #endregion

        #region Config

        [HttpPost, FileDownload, Route("api/BulkCopy/Config")]
        public HttpResponseMessage GetConfig(CopyModel model)
        {
            CopyConfig config = new CopyConfig()
            {
                TemplateMetaDataSheet = model.TemplateSheet.SheetName,
                ConstraintChangesSheet = model.ConstraintSheet != null ? model.ConstraintSheet.SheetName : string.Empty
            };

            foreach (var templateColumn in model.TemplateSheet.Columns)
            {
                CopyConfig.ColumnConfig columnConfig = new CopyConfig.ColumnConfig()
                {
                    Letter = templateColumn.Letter,
                    MappedField = templateColumn.MappedField.ToString()
                };

                config.TemplateColumns.Add(columnConfig);
            }

            if (model.ConstraintSheet != null)
            {
                foreach (var templateColumn in model.ConstraintSheet.Columns)
                {
                    CopyConfig.ColumnConfig columnConfig = new CopyConfig.ColumnConfig()
                    {
                        Letter = templateColumn.Letter,
                        MappedField = templateColumn.MappedField.ToString()
                    };

                    config.ConstraintColumns.Add(columnConfig);
                }
            }

            XmlSerializer serializer = new XmlSerializer(typeof(CopyConfig));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, config);

                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StreamContent(new MemoryStream(ms.ToArray()));
                message.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                message.Content.Headers.ContentLength = ms.Length;
                message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                message.Content.Headers.ContentDisposition.FileName = "config.xml";

                return message;
            }
        }

        #endregion

        #region Existing Templates

        [HttpGet, Route("api/BulkCopy/Templates/Existing")]
        public List<ExistingTemplateModel> CheckExistingTemplates([FromUri]int sourceTemplateId, [FromUri]List<string> templateOids)
        {
            Template sourceTemplate = this.tdb.Templates.Single(y => y.Id == sourceTemplateId);

            var existingTemplates = (from t in this.tdb.Templates
                                     join to in templateOids on t.Oid equals to
                                     select t);

            return (from e in existingTemplates
                    select new ExistingTemplateModel()
                    {
                        Name = e.Name,
                        Oid = e.Oid,
                        IsSameImplementationGuide = e.OwningImplementationGuideId == sourceTemplate.OwningImplementationGuideId
                    }).ToList();
        }

        #endregion
    }
}
