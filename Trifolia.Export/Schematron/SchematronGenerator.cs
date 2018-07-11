using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Trifolia.DB;
using Trifolia.Export.MSWord;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Export.Schematron.Model;
using Trifolia.Logging;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Export.Schematron
{
    public class SchematronGenerator
    {
        private IObjectRepository rep;
        private ImplementationGuide ig;
        private VocabularyOutputType vocabularyOutputType;
        private bool includeCustom;
        private string vocFileName;
        private IEnumerable<Template> templates;
        private Dictionary<int, string> clonedConstraints = new Dictionary<int, string>();            // int = constraint id, string = original business conformance
        private Dictionary<int, string> constraintNumbers = null;
        private string schemaPrefix;
        private IIGTypePlugin igTypePlugin;
        private SimpleSchema igTypeSchema;
        private List<string> categories;
        private string defaultSchematron;
        private TemplateContextBuilder templateContextBuilder;
        private IGSettingsManager igSettings;
        private List<ConstraintReference> constraintReferences = null;

        /// <summary>
        /// Creates a new instance of SchematronGenerator.
        /// </summary>
        /// <param name="rep">The repository (object context) to get the data from.</param>
        /// <param name="ig">The implementation guide to export schematron for.</param>
        /// <param name="includeInferred">Indicates whether or not to include inferred templates, in addition to the templates directly owned by the implementation guide</param>
        public SchematronGenerator(IObjectRepository rep,
            ImplementationGuide ig,
            List<Template> templates,
            bool includeCustom,
            VocabularyOutputType vocabularyOutputType = VocabularyOutputType.Default,
            string vocFileName = "voc.xml",
            List<string> categories = null,
            string defaultSchematron = "not(.)")
        {
            var templateIds = templates.Select(y => y.Id).ToList();

            this.rep = rep;
            this.ig = ig;
            this.igSettings = new IGSettingsManager(rep, ig.Id);
            this.templates = rep.Templates.Where(y => templateIds.Contains(y.Id))
                              .Include(y => y.ImpliedTemplate)
                              .Include("ChildConstraints")
                              .Include("ChildConstraints.ValueSet")
                              .Include("ChildConstraints.CodeSystem")
                              .Include("ChildConstraints.References")
                              .Include("ChildConstraints.ChildConstraints")
                              .Include("ChildConstraints.ChildConstraints.ValueSet")
                              .Include("ChildConstraints.ChildConstraints.CodeSystem")
                              .Include("ChildConstraints.ChildConstraints.References")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ValueSet")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.CodeSystem")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.References")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ValueSet")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.CodeSystem")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.References")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ValueSet")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.CodeSystem")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.References")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ValueSet")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.CodeSystem")
                              .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.References")
                              .ToList();
            this.constraintReferences = (from tcr in this.rep.TemplateConstraintReferences                                      // all constraint references
                                         join t in this.rep.Templates on tcr.ReferenceIdentifier equals t.Oid                   // get the template the reference is point to
                                         join stc in this.rep.TemplateConstraints on tcr.TemplateConstraintId equals stc.Id     // get the constraint that the reference is on
                                         join stid in templateIds on stc.TemplateId equals stid                                 // limit constraints to only templates being included in the ig/export
                                         select new ConstraintReference()
                                         {
                                             Bookmark = t.Bookmark,
                                             Identifier = t.Oid,
                                             Name = t.Name,
                                             TemplateConstraintId = tcr.TemplateConstraintId
                                         }).ToList();
            this.vocabularyOutputType = vocabularyOutputType;
            this.includeCustom = includeCustom;
            this.vocFileName = vocFileName;
            this.categories = categories;
            this.defaultSchematron = defaultSchematron;

            this.igTypePlugin = this.ig.ImplementationGuideType.GetPlugin();
            this.igTypeSchema = ig.ImplementationGuideType.GetSimpleSchema();
            this.templateContextBuilder = new TemplateContextBuilder(this.rep, ig.ImplementationGuideType, this.igTypeSchema);

            var allConstraint = (from t in templates
                                 from tc in t.ChildConstraints
                                 select new { tc.Id, tc.Template.OwningImplementationGuideId, tc.Number.Value })
                                 .Distinct();
            this.constraintNumbers = allConstraint.ToDictionary(y => y.Id, y => string.Format("{0}-{1}", y.OwningImplementationGuideId, y.Value));

            this.schemaPrefix = ig.ImplementationGuideType.SchemaPrefix;

            if (!this.schemaPrefix.EndsWith(":"))
                this.schemaPrefix += ":";
        }

        public static string Generate(IObjectRepository rep, 
            ImplementationGuide ig, 
            bool includeCustom, 
            VocabularyOutputType vocabularyOutputType = VocabularyOutputType.Default, 
            string vocFileName = "voc.xml",
            List<Template> templates = null,
            List<string> categories = null,
            string defaultSchematron = "not(.)")
        {
            SchematronGenerator generator = new SchematronGenerator(rep, ig, templates, includeCustom, vocabularyOutputType, vocFileName, categories, defaultSchematron);
            return generator.Generate();
        }

        /// <summary>
        /// Generates the close template section constraints for a particular template
        /// </summary>
        /// <param name="aClosedTemplate"></param>
        /// <returns>xpath test for assertion</returns>
        public string GenerateClosedTemplateConstraints(Template aClosedTemplate)
        {
            List<string> xpaths = new List<string>();
            xpaths.Add(GenerateClosedTemplateIdentifierXpath(aClosedTemplate.Oid));

            if (aClosedTemplate.ImpliedTemplate != null)
                xpaths.Add(GenerateClosedTemplateIdentifierXpath(aClosedTemplate.ImpliedTemplate.Oid));

            var childOids = this.GetAllChildTemplateOids(this.rep, aClosedTemplate);

            foreach (var oid in childOids)
            {
                xpaths.Add(GenerateClosedTemplateIdentifierXpath(oid));
            }

            if (xpaths.Count > 0)
                return string.Format(this.igTypePlugin.ClosedTemplateXpath, this.schemaPrefix, string.Join(" and ", xpaths));

            return null;
        }

        /// <summary>
        /// Given a template, returns all child template oid's by recursively walking the child collection
        /// </summary>
        /// <param name="parentTemplate"></param>
        /// <param name="aChildOids"></param>
        /// <returns>string list of all unique oids</returns>
        public IList<string> GetAllChildTemplateOids(IObjectRepository tdb, Template parentTemplate, List<string> aChildOids = null)
        {
            var childOids = aChildOids == null ? new List<string>() : aChildOids;
            var impliedTemplate = this.templates.SingleOrDefault(y => y.Id == parentTemplate.ImpliedTemplateId);

            if (impliedTemplate != null)
            {
                if (!childOids.Contains(parentTemplate.ImpliedTemplate.Oid))
                {
                    string oid = impliedTemplate.Oid;
                    childOids.Add(oid);
                }
            }

            foreach (var childConstraint in parentTemplate.ChildConstraints)
            {
                var containedTemplates = (from tcr in childConstraint.References
                                          join t in this.templates on tcr.ReferenceIdentifier equals t.Oid
                                          where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                          select t);

                foreach (var containedTemplate in containedTemplates)
                {
                    if (!childOids.Contains(containedTemplate.Oid))
                    {
                        childOids.Add(containedTemplate.Oid);
                        GetAllChildTemplateOids(tdb, containedTemplate, childOids);
                    }
                }
            }

            return childOids;
        }

        private string GenerateClosedTemplateIdentifierXpath(string templateIdentifier)
        {
            string oid;
            string root;
            string extension;
            string urn;

            if (IdentifierHelper.GetIdentifierOID(templateIdentifier, out oid))
            {
                return string.Format(this.igTypePlugin.ClosedTemplateIdentifierXpath, this.schemaPrefix, oid);
            }
            else if (IdentifierHelper.GetIdentifierII(templateIdentifier, out root, out extension))
            {
                return string.Format(this.igTypePlugin.ClosedTemplateVersionIdentifierXpath, this.schemaPrefix, root, extension);
            }
            else if (IdentifierHelper.GetIdentifierURL(templateIdentifier, out urn))
            {
                return string.Format(this.igTypePlugin.ClosedTemplateIdentifierXpath, this.schemaPrefix, urn);
            }
            else
            {
                throw new Exception("Unexpected/invalid identifier for template found when processing template reference for closed template identifier xpath");
            }
        }

        public string GenerateDocumentLevelTemplateConstraints(ImplementationGuide aImplementationGuide, Phase aErrorPhase)
        {
            var firstTemplateType = aImplementationGuide.ImplementationGuideType.TemplateTypes.OrderBy(y => y.OutputOrder).FirstOrDefault();

            if (firstTemplateType == null)
                return string.Empty;

            var documentLevelTemplates = this.templates.Where(y => y.TemplateTypeId == firstTemplateType.Id);

            string lDeprecatedStatus = Shared.PublishStatuses.Deprecated.ToString();
            documentLevelTemplates.ToList().RemoveAll(t => t.Status != null && t.Status.Status == lDeprecatedStatus);

            List<string> requiredTemplates = new List<string>();
            TemplateContextBuilder tcb = new TemplateContextBuilder(this.rep, aImplementationGuide.ImplementationGuideType, this.igTypeSchema);

            foreach (var template in documentLevelTemplates)
            {
                string xpath = tcb.BuildContextString(template);
                requiredTemplates.Add(xpath);
            }

            return string.Join(" or ", requiredTemplates);
        }

        public Pattern AddDocumentLevelTemplateConstraints(ImplementationGuide aImplementationGuide, Phase aPhase)
        {

            Pattern newPattern = null;
            string test = GenerateDocumentLevelTemplateConstraints(aImplementationGuide, aPhase);

            if (!string.IsNullOrEmpty(test))
            {
                string templateContext = string.Format("{0}:ClinicalDocument", this.ig.ImplementationGuideType.SchemaPrefix);

                newPattern = new Pattern()
                {
                    ID = "p-DOCUMENT-TEMPLATE",
                    Name = "p-DOCUMENT-TEMPLATE"
                };

                Rule newRule = new Rule()
                {
                    Id = "r-errors-DOC",
                    Context = templateContext
                };

                Assertion newAssertion = new Assertion()
                {
                    Id = "IG-" + aImplementationGuide.Id.ToString(),
                    IdPostFix = "-DOC",
                    AssertionMessage = "The document must contain at least 1 of the document level templates for this schematron to be applicable.",
                    Test = test
                };

                newRule.Assertions.Add(newAssertion);

                newPattern.Rules.Add(newRule);

                if (newPattern.Rules.Count > 0)
                {
                    aPhase.ActivePatterns.Add(newPattern);
                }
            }
            return newPattern;
        }


        /// <summary>
        /// Adds the constraints for the closed template to ensure that no template id's other than those explicitly named are used in the xml.
        /// </summary>
        /// <param name="aClosedTemplate">Closed template to generate the constraint for</param>
        /// <param name="aPhase">The error phase for the schematron</param>
        /// <returns>
        /// Pattern that was added to the schematron
        /// </returns>
        public Pattern AddClosedTemplateConstraints(Template aClosedTemplate, Phase aPhase)
        {

            if (aClosedTemplate.IsOpen)
            {
                return null;
            }

            Pattern newPattern = null;
            string test = GenerateClosedTemplateConstraints(aClosedTemplate);

            if (!string.IsNullOrEmpty(test))
            {
                string templateContext = this.templateContextBuilder.BuildContextString(aClosedTemplate);

                newPattern = new Pattern()
                {
                    ID = string.Format("p-{0}-CLOSEDTEMPLATE", aClosedTemplate.Oid),
                    Name = string.Format("p-{0}-CLOSEDTEMPLATE", aClosedTemplate.Oid)
                };

                Rule newRule = new Rule()
                {
                    Id = string.Format("r-{0}-errors-CL", aClosedTemplate.Oid),
                    Context = templateContext
                };

                Assertion newAssertion = new Assertion()
                {
                    Id = string.Format("{0}-{1}", aClosedTemplate.OwningImplementationGuideId, aClosedTemplate.Id),
                    IdPostFix = "-CL",
                    AssertionMessage = string.Format("'{0}' is a closed template, only defined templates are allowed.", aClosedTemplate.Oid),
                    Test = test
                };

                newRule.Assertions.Add(newAssertion);

                newPattern.Rules.Add(newRule);

                if (newPattern.Rules.Count > 0)
                {
                    aPhase.ActivePatterns.Add(newPattern);
                }
            }
            return newPattern;
        }

        /// <summary>
        /// Generates a schematron xml (string) for the given IG in the database.
        /// </summary>
        /// <returns>Returns a string representing the generated schematron document (XML).</returns>
        public string Generate()
        {
            if (ig == null)
                return null; //thow an exception?

            // Clear the cloned constraints whenever we run generate so that we don't get duplicate exceptions
            this.clonedConstraints.Clear();

            var errorPhase = new Phase()
            {
                ID = "errors"
            };
            var warningPhase = new Phase()
            {
                ID = "warnings"
            };
            var schematronDocument = new SchematronDocument();

            if (this.includeCustom)
            {
                IEnumerable<ImplementationGuideSchematronPattern> customErrorPatterns = ig.SchematronPatterns.Where(y => y.Phase == "errors");
                IEnumerable<ImplementationGuideSchematronPattern> customWarningPatterns = ig.SchematronPatterns.Where(y => y.Phase == "warnings");

                AddCustomPatterns(errorPhase, customErrorPatterns);
                AddCustomPatterns(warningPhase, customWarningPatterns);
            }

            // TRIF-909: Remove closed template rules from Trifolia Schematron generation
            //AddDocumentLevelTemplateConstraints(this.ig, errorPhase);

            foreach (var template in templates)
            {
                try
                {
                    AddTemplate(template, errorPhase, warningPhase);

                    if (!template.IsOpen)
                    {
                        AddClosedTemplateConstraints(template, errorPhase);
                    }

                }
                catch (Exception ex)
                {
                    Log.For(this).Error("Error generating schematron for template {0} ({1})", ex, template.Name, template.Oid);
                    throw new Exception(
                        string.Format(
                            "Unknown error occurred while generating schematron at {0}. Please notify a system administrator of this error.", 
                            DateTime.Now.ToShortTimeString()), ex);
                }
            }

            schematronDocument.Phases.Add(errorPhase);
            schematronDocument.Phases.Add(warningPhase);

            // Prepare a list of namespaces to add to the schematron document
            var namespacesList = (from t in templates
                                  join tt in this.rep.TemplateTypes on t.TemplateTypeId equals tt.Id
                                  join igt in this.rep.ImplementationGuideTypes on tt.ImplementationGuideTypeId equals igt.Id
                                  select new { Prefix = igt.SchemaPrefix, URI = igt.SchemaURI }).Distinct();

            Dictionary<string, string> namespaces = new Dictionary<string,string>();

            foreach (var cNamespaceItem in namespacesList)
            {
                namespaces.Add(cNamespaceItem.Prefix, cNamespaceItem.URI);
            }

            return SerializeSchematronDocument(schematronDocument, namespaces);
        }

        private void AddCustomPatterns(Phase phase, IEnumerable<ImplementationGuideSchematronPattern> schPatterns)
        {
            foreach (ImplementationGuideSchematronPattern schPattern in schPatterns)
            {
                Pattern newPattern = new Pattern()
                {
                    ID = schPattern.PatternId,
                    Name = schPattern.PatternId,
                    CustomXML = schPattern.PatternContent,
                };

                phase.ActivePatterns.Add(newPattern);
            }
        }

        private bool IsBranchedParent(TemplateConstraint aConstraint)
        {
            TemplateConstraint current = aConstraint;
            while (current.Parent != null)
            {
                current = current.ParentConstraint;
                if (current.IsBranch)
                {
                    return true;
                }
            }

            return false;
        }

        internal void AddTemplate(Template template, Phase errorPhase, Phase warningPhase, bool isImplied=false)
        {
            string errorId = template.Oid.Trim() + "-errors";
            string warningId = template.Oid.Trim() + "-warnings";
            string patternErrorId = "p-" + errorId;
            string patternWarningId ="p-" + warningId;
            string ruleErrorId = "r-" + errorId;
            string ruleWarningId = "r-" + warningId;
            string impliedPatternErrorId = null;
            string impliedPatternWarningId = null;
            string impliedRuleErrorId = null;
            string impliedRuleWarningId = null;

            // Add the implied template first
            if (template.ImpliedTemplate != null && template.ImpliedTemplateId != template.Id && this.templates.Contains(template.ImpliedTemplate))
            {
                string impliedErrorId = template.ImpliedTemplate.Oid.Trim() + "-errors";
                string impliedWarningId = template.ImpliedTemplate.Oid.Trim() + "-warnings";
                impliedPatternErrorId = "p-" + impliedErrorId;
                impliedPatternWarningId = "p-" + impliedWarningId;
                impliedRuleErrorId = "r-" + impliedErrorId;
                impliedRuleWarningId = "r-" + impliedWarningId;

                // Make sure we haven't already added the implied template and that this is not an endless recursive loop
                if (!errorPhase.ActivePatterns.Exists(y => y.ID == impliedPatternErrorId) && !warningPhase.ActivePatterns.Exists(y => y.ID == impliedPatternWarningId))
                    AddTemplate(template.ImpliedTemplate, errorPhase, warningPhase, true);
            }

            bool alreadyGeneratedErrorPattern = errorPhase.ActivePatterns.Exists(y => y.ID == patternErrorId);
            bool alreadyGeneratedWarningPattern = warningPhase.ActivePatterns.Exists(y => y.ID == patternWarningId);

            if (alreadyGeneratedErrorPattern && alreadyGeneratedWarningPattern)
                return;

            //realize the list so that all of them are pulled from database
            var constraints = template.ChildConstraints.ToList();

            //NOTE: there are also child templates on the template class. If GetRecursiveTemplates does not flatten the heirarchy, then we'll need to walk the tree. We could do this in the linq query, but I' not certain 
            //whether the context is set explicitly in the child templates, or whether it's implied that the implementer (me) would have to discover it.

            //get only SHALLS
            var requiredConformance = (from c in constraints
                                       where c.IsRequiredConformance() && c.IsNotBranchIdentifierOrIsValidIdentifier()
                                       orderby c.Id
                                       select c);

            //get all SHOULDs
            var optionalConformance = (from c in constraints
                                       where c.IsOptionalConformance() && !c.IsBranchIdentifier
                                       orderby c.Id
                                       select c).ToList();

            //add in optional value conformance on required constraints
            AddOptionalValueConformanceOnRequiredElement(optionalConformance, constraints);

            //get only SHALLs that are leaf level of a branch and don't have optional parents
            var branchedRequiredConformance = (from c in constraints
                                               where c.IsRequiredConformance() && c.HasParentBranch()
                                               orderby c.Id
                                               select c);

            //get all SHOULDs and only SHALLs having optional parents but that are leaf level part of a branch
            var branchedOptionalConformance = (from c in constraints
                                               where (c.IsOptionalConformance() || (c.HasOptionalParent() && c.HasRequiredParent())) && c.IsBranchDescendent()
                                               orderby c.Id
                                               select c);
            string templateContext = string.Empty;

            templateContext = this.templateContextBuilder.BuildContextString(template);

            Pattern ErrorPattern = null;
            ErrorPattern = GeneratePattern(Conformance.SHALL, requiredConformance, errorPhase, patternErrorId, ruleErrorId, templateContext, impliedPatternErrorId, impliedRuleErrorId, isImplied);

            Pattern WarningPattern = null;
            WarningPattern = GeneratePattern(Conformance.SHOULD, optionalConformance, warningPhase, patternWarningId, ruleWarningId, templateContext, impliedPatternWarningId, impliedRuleWarningId, isImplied);

            if (branchedRequiredConformance.Count() > 0)
                GenerateBranchedPattern(Conformance.SHALL, ErrorPattern, branchedRequiredConformance, constraints, template, errorPhase);

            if (branchedOptionalConformance.Count() > 0)
                GenerateBranchedPattern(Conformance.SHOULD, WarningPattern, branchedOptionalConformance, constraints, template, warningPhase);
        }

        /// <summary>
        /// Add constraints to OptionalConformance list where business conformance is required but value conformance is optional
        /// </summary>
        /// <param name="aOptionalConformance"></param>
        /// <param name="aConstraints"></param>
        private void AddOptionalValueConformanceOnRequiredElement(List<TemplateConstraint> aOptionalConformance, List<TemplateConstraint> aConstraints)
        {
            var valueConformance = (from c in aConstraints
                                    where !string.IsNullOrEmpty(c.ValueConformance)
                                        && c.BusinessConformanceType == Conformance.SHALL
                                        && c.ValueConformanceType != Conformance.SHALL
                                        && string.IsNullOrEmpty(c.Schematron)
                                        && !c.IsPrimitive
                                    select c);
            
            foreach(var c in valueConformance)
            {
                clonedConstraints.Add(c.Id, c.Conformance);

                var clone = (TemplateConstraint) c.Clone(); //create new instance
                clone.Conformance = clone.ValueConformance;
                aOptionalConformance.Add(clone);
            }

            aOptionalConformance = aOptionalConformance.OrderBy(c => c.Id).ToList();
        }

        /// <summary>
        /// Returns the first branch starting with aConstraint.Parent (does not consider aConstraint in the search)
        /// </summary>
        /// <param name="aConstraint"></param>
        /// <returns></returns>
        internal TemplateConstraint GetNearestBranch(TemplateConstraint aConstraint)
        {
            TemplateConstraint current = aConstraint.ParentConstraint;
            while ((current != null) && (current.Parent != null) && (!current.IsBranch))
            {
                current = current.ParentConstraint;
            }

            return current;
        }

        /// <summary>
        /// Returns first branch occurrence of IsBranch (if aConstraint is branch then returns aConstraint)
        /// </summary>
        /// <param name="aConstraint"></param>
        /// <returns></returns>
        internal TemplateConstraint GetBranch(TemplateConstraint aConstraint)
        {
            TemplateConstraint current = aConstraint;
            while ((current.Parent != null) && (!current.IsBranch))
            {
                current = current.ParentConstraint;
            }

            return current;
        }

        internal bool MeetsConformance(Conformance aMinimumConformanceLevel, Conformance aTestConformance)
        {
            switch (aMinimumConformanceLevel)
            {
                case Conformance.SHALL :
                case Conformance.SHALL_NOT:
                    return aTestConformance == Conformance.SHALL || aTestConformance == Conformance.SHALL_NOT
                        || aTestConformance == Conformance.SHOULD || aTestConformance == Conformance.SHOULD_NOT;  //only allow SHALL, SHALL_NOT AND SHOULD AND SHOULD NOT
                case Conformance.SHOULD:
                case Conformance.SHOULD_NOT:
                case Conformance.MAY:
                case Conformance.MAY_NOT:
                    return aTestConformance != Conformance.SHALL && aTestConformance != Conformance.SHALL_NOT;  //only allow SHOULD or SHOULD_NOT OR MAY AND MAY_NOT;
                default:
                    return false;
            }

        }

        internal Conformance GetMinimumConformanceForConstraint(TemplateConstraint aConstraint)
        {
            if (aConstraint == null)
                return Conformance.UNKNOWN;
            Conformance minLevel = aConstraint.BusinessConformanceType;
            TemplateConstraint constraint = aConstraint;

            if (!aConstraint.IsBranchDescendent() || (aConstraint.IsPrimitive && aConstraint.IsSchRooted))
                return minLevel;

            while (constraint != null)
            {
                if (minLevel < constraint.BusinessConformanceType)
                    minLevel = constraint.BusinessConformanceType;
                if (constraint.IsBranch)
                {
                    break;
                }
                constraint = constraint.ParentConstraint;                
            }

            return minLevel;
        }

        internal void GenerateBranchedPattern(Conformance MinimumConformanceLevel, Pattern aPattern, IEnumerable<TemplateConstraint> aConstraints, IEnumerable<TemplateConstraint> aTemplateConstraints, Template aTemplate, Phase aPhase)
        {
            string rulePostFix = MinimumConformanceLevel == Conformance.SHALL ? "-errors" : "-warnings";
            string errorId = aTemplate.Oid.Trim() + "-{0}";
            string patternErrorId = "p-" + errorId;
            string ruleErrorId = "r-" + errorId + "-branch-{0}" + rulePostFix;
            List<int> AlreadyGeneratedRoots = new List<int>();

            foreach (var branch in aConstraints)
            {
                if (!branch.CategoryIsMatch(this.categories))
                    continue;

                TemplateConstraint root = GetBranch(branch);
                //get all constraints that have this root as their parent
                if (!AlreadyGeneratedRoots.Contains(root.Id))
                {
                    AlreadyGeneratedRoots.Add(root.Id);
                    string context = this.templateContextBuilder.CreateFullBranchedParentContext(aTemplate, root);
                    var rule = new Rule();
                    rule.Id = string.Format(ruleErrorId, root.Number);
                    rule.Context = context;

                    // No branch identifiers unless there is custom schematron
                    var branchedConstraints = aTemplateConstraints.Where(c => (c.Id != root.Id) && (root == GetNearestBranch(c)))
                        .Where(y => !y.IsBranchIdentifier || !string.IsNullOrEmpty(y.Schematron));

                    foreach (var constraint in branchedConstraints)
                    {
                        if (!constraint.CategoryIsMatch(this.categories))
                            continue;

                        try
                        {
                            Conformance leastConformanceForGraph = GetMinimumConformanceForConstraint(constraint);
                            //exclude MAYs (we never want a constraint whose conformance is MAY, we will take graph MAYs but not directly on the constraint)
                            if (constraint.BusinessConformanceType == Conformance.MAY || constraint.BusinessConformanceType == Conformance.MAY_NOT ||
                                 ShouldSkipConstraint(constraint) || !MeetsConformance(MinimumConformanceLevel, leastConformanceForGraph))
                                continue;

                            //skip direct SHOULD on SHALL
                            if (constraint.BusinessConformanceType == Conformance.SHOULD && MinimumConformanceLevel == Conformance.SHALL)
                                continue;

                            //skip direct SHALL on SHOULD 
                            if (constraint.BusinessConformanceType == Conformance.SHALL && MinimumConformanceLevel == Conformance.SHOULD)
                                continue;

                            // Skip for constraints that are rooted, but only when there is either custom schematron or it is a primitive
                            if (constraint.IsSchRooted && (!string.IsNullOrEmpty(constraint.Schematron) || constraint.IsPrimitive))
                                continue;

                            rule.Assertions.Add(GetAssertion(constraint, string.Format("-branch-{0}", root.Number)));
                        }
                        catch (Exception ex)
                        {
                            Log.For(this).Error("Error getting assertion for constraint {0} on template {1}.", ex, constraint.Id, constraint.TemplateId);
                            throw ex;
                        }
                    }

                    if (rule.Assertions.Count > 0)
                    {
                        aPattern.Rules.Add(rule);
                    }
                }
            }
        }
                
        internal virtual string SerializeSchematronDocument(SchematronDocument doc, Dictionary<string, string> namespaces)
        {
            var ser = new SchematronDocumentSerializer();

            // Add the namespaces to the serializer
            foreach (string cPrefix in namespaces.Keys)
                ser.AddNamespace(cPrefix, namespaces[cPrefix]);

            var output = ser.SerializeDocument(doc);

            return output;
        }

        /// <summary>
        /// The role of this method is to take a list of constraints and produce a pattern with rules and assertions.
        /// </summary>
        /// <param name="constraints"></param>
        /// <param name="phase"></param>
        /// <param name="patternName"></param>
        /// <param name="context"></param>
        internal Pattern GeneratePattern(Conformance aMinimumConformance, IEnumerable<TemplateConstraint> constraints, Phase phase, string patternName, string ruleId, string context, string impliedPatternId=null, string impliedRuleId=null, bool isImplied=false)
        {
            var pattern = new Pattern();
            pattern.IsImplied = isImplied;
            pattern.ID = patternName;
            pattern.Name = patternName;
            pattern.IsA = impliedPatternId;

            var rule = new Rule();
            rule.Id = ruleId;
            rule.Context = context;
            rule.Extends = impliedRuleId;
            pattern.Rules.Add(rule);

            foreach (var constraint in constraints)
            {
                if (!constraint.CategoryIsMatch(this.categories))
                    continue;

                try
                {
                    Conformance conformance = GetMinimumConformanceForConstraint(constraint);

                    //ShouldSkipConstraint and turn on flag to skip any descendent of a branch b/c these get handled inside the branch context rules
                    if (ShouldSkipConstraint(constraint, true) || !MeetsConformance(aMinimumConformance, conformance)) 
                        continue;                    

                    //MAYs directly on the constraint should not be outputted (parent MAYs can be outputted, but not constraints with conformance MAY)
                    if (constraint.BusinessConformanceType == Conformance.MAY || constraint.BusinessConformanceType == Conformance.MAY_NOT)
                        continue;

                    rule.Assertions.Add(GetAssertion(constraint));
                }
                catch (Exception ex)
                {
                    Log.For(this).Error("Error getting assertion for constraint {0} on template {1}.", ex, constraint.Id, constraint.TemplateId);
                    throw ex;
                }
            }

            if (rule.Assertions.Count == 0)
            {
                rule.Assertions.Add(GenerateBlankAssertion());
            }

            // TODO: This should probably be moved to it's calling function
            phase.ActivePatterns.Add(pattern);

            return pattern; //return the newly created pattern
        }

        /// <summary>
        /// Determines if a constraint should be included as an individual assertion, or if it is part of a branched parent constraint and should be ignored.
        /// </summary>
        /// <param name="constraint">The constraint in question.</param>
        /// <returns>True if the constraint should be skipped from generation, false if it should be included.</returns>
        private bool ShouldSkipConstraint(TemplateConstraint constraint, bool SkipBranchDescendents=false)
        {
            bool isAttribute = !string.IsNullOrEmpty(constraint.Context) && constraint.Context.StartsWith("@");
            bool shouldSkip = constraint.IsBranchIdentifier && string.IsNullOrEmpty(constraint.Schematron);

            if (SkipBranchDescendents) //should we exclude all descendents of a branch
            {
                // Skip for constraints that are rooted, but only when there is either custom schematron or it is a primitive
                bool isSchRooted = constraint.IsSchRooted && (!string.IsNullOrEmpty(constraint.Schematron) || constraint.IsPrimitive);

                if (!shouldSkip && !isSchRooted)
                    shouldSkip = constraint.IsBranchDescendent();
            }

            return shouldSkip;
        }

        internal Assertion GenerateBlankAssertion()
        {
            var assertion = new Assertion();
            assertion.Test = ".";
            return assertion;
        }

        /// <summary>
        /// this method decides whether the assertion test is already in the data model or needs to be generated. It will also generate a message
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        internal Assertion GetAssertion(TemplateConstraint tc, string idPostFix = "")
        {
            var assertion = new Assertion()
            {
                Id = this.constraintNumbers[tc.Id],      // Set the assertion's ID to the constraint's ID
                IsInheritable = tc.IsInheritable,
                IdPostFix = idPostFix
            };

            if (clonedConstraints.ContainsKey(tc.Id) && tc.BusinessConformanceType == tc.ValueConformanceType)
            {
                assertion.IdPostFix += "-v";
            }

            if (!string.IsNullOrEmpty(tc.Schematron))
            {
                assertion.AssertionMessage = GetAssertionTestMessage(tc);
                assertion.Test = tc.Schematron;
                assertion.IdPostFix += "-c"; //custom schematron
            }
            else if (tc.IsPrimitive == true)
            {
                assertion.AssertionMessage = GetAssertionTestMessage(tc);
                assertion.Test = tc.Schematron;
                assertion.IdPostFix += "-c"; //custom schematron

                if (string.IsNullOrEmpty(tc.Schematron))
                {
                    assertion.Test = this.defaultSchematron;
                    assertion.Comment = string.Format("No schematron defined for primitive constraint {0} on template {1}", tc.Id, tc.TemplateId);
                }
            }
            else
            {
                var originalConformance = clonedConstraints.ContainsKey(tc.Id) ? clonedConstraints[tc.Id] : tc.Conformance;

                assertion.AssertionMessage = GetAssertionTestMessage(tc, originalConformance); //TODO: rather than generate this every time... should we just store it in the database when the UI stores the schematron test?

                if (!string.IsNullOrEmpty(tc.Schematron))
                {
                    assertion.Test = tc.Schematron;
                }
                else
                {
                    try
                    {
                        assertion.Test = GenerateAssertion(tc);

                        //assertion tests cannot be empty. generate a comment for the user to review.
                        if (string.IsNullOrEmpty(assertion.Test))
                        {
                            assertion.Test = "not(.)";
                            assertion.Comment = BuildErrorMessageComment(tc);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("Error generating assertion statement for constraint {0} on template '{1}': {2}", tc.Id, tc.Template.Oid, ex.Message);
                        Log.For(this).Critical(msg, ex);
                        assertion.Comment = msg;
                    }
                }
            }

            return assertion;
        }

        internal string BuildErrorMessageComment(TemplateConstraint tc)
        {
            var sbComment = new StringBuilder();

            //build up error msg so that user can possibly locate the problem in tdb
            sbComment.AppendFormat("Empty test generated! Replaced with \".\". Invalid schematron generated for constraint {0} on template {1}. ", tc.Id, tc.TemplateId);

            if (!string.IsNullOrEmpty(tc.Cardinality))
                sbComment.AppendFormat("Cardinality='{0}'. ", tc.Cardinality);

            if (!string.IsNullOrEmpty(tc.Conformance))
                sbComment.AppendFormat("Conformance='{0}'. ", tc.Conformance);

            if (!string.IsNullOrEmpty(tc.Value))
                sbComment.AppendFormat("Value='{0}'. ", tc.Conformance);

            if (tc.ValueSet != null && !string.IsNullOrEmpty(tc.ValueSet.Code))
                sbComment.AppendFormat("Valueset Code='{0}'. ", tc.ValueSet.Code);

            if (!string.IsNullOrEmpty(tc.DataType))
                sbComment.AppendFormat("Data Type='{0}'. ", tc.DataType);

            var containedTemplateReferences = tc.References
                .Where(y => y.ReferenceType == ConstraintReferenceTypes.Template)
                .Select(y => y.ReferenceIdentifier);

            if (containedTemplateReferences.Count() > 0)
                sbComment.AppendFormat("Templates='{0}'. ", string.Join("', '", containedTemplateReferences));

            return sbComment.ToString();
        }


        /// <summary>
        /// this simply calls out to the FormattedConstraint class to create a message. It's abstracted to be mockable.
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        internal string GetAssertionTestMessage(TemplateConstraint tc, string conformance = null)
        {
            List<string> messages = new List<string>();
            
            IFormattedConstraint currentFc = new FormattedConstraint(this.rep, this.igSettings, this.igTypePlugin, tc, this.constraintReferences);

            if (!string.IsNullOrEmpty(conformance))
                currentFc.Conformance = conformance;

            // Need to re-parse the properties of the formatted constraint since they have changed
            currentFc.ParseFormattedConstraint();

            messages.Add(currentFc.GetPlainText(false, false, false));

            if (tc.IsBranch || tc.IsBranchDescendent())
            {
                foreach (var child in tc.ChildConstraints)
                {
                    if (child.IsBranchIdentifier)
                    {
                        messages.Add(GetAssertionTestMessage(child));
                    }
                }
            }
            
            return string.Join(" ", messages);
        }

        /// <summary>
        /// this will generate an assertion if none is in the entity model
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        internal string GenerateAssertion(TemplateConstraint tc)
        {
            ConstraintParser cParser = new ConstraintParser(this.rep, tc, tc.Template.ImplementationGuideType, this.igTypeSchema, this.templates, this.vocFileName, this.vocabularyOutputType);

            if (tc.ValueSet != null)
                cParser.ValueSet = tc.ValueSet;

            if (tc.CodeSystem != null)
                cParser.CodeSystem = tc.CodeSystem;

            AssertionLineBuilder asb = cParser.CreateAssertionLineBuilder();
            return asb.ToString();
        }
    }

    public static class TemplateConstraintExtension
    {
        public static bool HasParentBranch(this TemplateConstraint constraint)
        {
            IConstraint parent = constraint.Parent;

            while (parent != null)
            {
                if (parent.IsBranch)
                    return true;

                parent = parent.Parent;
            }

            return false;
        }

        public static bool IsRequiredConformance(this TemplateConstraint constraint)
        {
            if (string.IsNullOrEmpty(constraint.Conformance))
                return false;

            return constraint.BusinessConformanceType == Conformance.SHALL || constraint.BusinessConformanceType == Conformance.SHALL_NOT;
        }

        public static bool IsOptionalConformance(this TemplateConstraint constraint)
        {
            if (string.IsNullOrEmpty(constraint.Conformance))
                return false;

            return constraint.BusinessConformanceType == Conformance.SHOULD || constraint.BusinessConformanceType == Conformance.SHOULD_NOT;
        }

        public static bool HasRequiredParent(this TemplateConstraint constraint)
        {
            TemplateConstraint current = constraint;
            while (current != null)
            {
                if (constraint.IsRequiredConformance())
                    return true;

                current = current.ParentConstraint;
            }

            return false;
        }

        public static bool HasOptionalParent(this TemplateConstraint constraint)
        {
            TemplateConstraint current = constraint;
            while (current != null)
            {
                if (current.IsOptionalConformance())
                    return true;

                current = current.ParentConstraint;
            }

            return false;
        }
        
        public static bool IsBranchDescendent(this TemplateConstraint constraint)
        {
            var current = constraint;

            //note this checks parent, which means if aConstraint is a branch this returns false (as it is not a branch descendent but a branch root)
            while (current.ParentConstraint != null)
            {
                current = current.ParentConstraint;

                if (current.IsBranch)
                    return true;
            }

            return false;
        }

        public static bool IsNotBranchIdentifierOrIsValidIdentifier(this TemplateConstraint constraint)
        {
            if (!constraint.IsBranchIdentifier)
                return true;

            if (!string.IsNullOrEmpty(constraint.Schematron))
                return false;

            return true;
        }
    }
}
