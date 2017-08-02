using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Authorization
{
    /// <summary>
    /// Defines the securables allowed to be associated with roles.
    /// Securables are granular role-based permissions that indicate what actions a user can perform.
    /// </summary>
    public class SecurableNames
    {
        #region Public Constants

        /// <summary>
        /// LandingPage
        /// </summary>
        public const string LANDING_PAGE = "LandingPage";

        /// <summary>
        /// IGManagementList
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_LIST = "IGManagementList";

        /// <summary>
        /// ImplementationGuideEdit
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_EDIT = "ImplementationGuideEdit";

        /// <summary>
        /// ImplementationGuideEditBookmarks
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS = "ImplementationGuideEditBookmarks";

        /// <summary>
        /// ImplementationGuideNotes
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_NOTES = "ImplementationGuideNotes";

        /// <summary>
        /// ImplementationGuidePrimitives
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_PRIMITIVES = "ImplementationGuidePrimitives";

        /// <summary>
        /// ImplementationGuideAuditTrail
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_AUDIT_TRAIL = "ImplementationGuideAuditTrail";

        /// <summary>
        /// IGFileManagement
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_FILE_MANAGEMENT = "IGFileManagement";

        /// <summary>
        /// IGFileView
        /// </summary>
        public const string IMPLEMENTATIONGUIDE_FILE_VIEW = "IGFileView";

        /// <summary>
        /// ValueSetList
        /// </summary>
        public const string VALUESET_LIST = "ValueSetList";

        /// <summary>
        /// ValueSetEdit
        /// </summary>
        public const string VALUESET_EDIT = "ValueSetEdit";

        /// <summary>
        /// TerminologyOverride
        /// </summary>
        public const string TERMINOLOGY_OVERRIDE = "TerminologyOverride";

        /// <summary>
        /// CodeSystemList
        /// </summary>
        public const string CODESYSTEM_LIST = "CodeSystemList";

        /// <summary>
        /// CodeSystemEdit
        /// </summary>
        public const string CODESYSTEM_EDIT = "CodeSystemEdit";

        /// <summary>
        /// ExportWordDocuments
        /// </summary>
        public const string EXPORT_WORD = "ExportWordDocuments";

        /// <summary>
        /// ExportVocabulary
        /// </summary>
        public const string EXPORT_VOCAB = "ExportVocabulary";

        /// <summary>
        /// ExportSchematron
        /// </summary>
        public const string EXPORT_SCHEMATRON = "ExportSchematron";
        
        /// <summary>
        /// ExportXML
        /// </summary>
        public const string EXPORT_XML = "ExportXML";

        /// <summary>
        /// ExportGreen
        /// </summary>
        public const string EXPORT_GREEN = "ExportGreen";

        /// <summary>
        /// TemplateList
        /// </summary>
        public const string TEMPLATE_LIST = "TemplateList";

        /// <summary>
        /// TemplateEdit
        /// </summary>
        public const string TEMPLATE_EDIT = "TemplateEdit";

        /// <summary>
        /// TemplateCopy
        /// </summary>
        public const string TEMPLATE_COPY = "TemplateCopy";

        /// <summary>
        /// TemplateDelete
        /// </summary>
        public const string TEMPLATE_DELETE = "TemplateDelete";

        /// <summary>
        /// TemplateMove
        /// </summary>
        public const string TEMPLATE_MOVE = "TemplateMove";

        /// <summary>
        /// PublishSettings
        /// </summary>
        public const string PUBLISH_SETTINGS = "PublishSettings";

        /// <summary>
        /// ReportTemplateReview
        /// </summary>
        public const string REPORT_TEMPLATE_REVIEW = "ReportTemplateReview";

        /// <summary>
        /// ReportTemplateCompliance
        /// </summary>
        public const string REPORT_TEMPLATE_COMPLIANCE = "ReportTemplateCompliance";

        /// <summary>
        /// GreenModel
        /// </summary>
        public const string GREEN_MODEL = "GreenModel";

        /// <summary>
        /// Admin
        /// </summary>
        public const string ADMIN = "Admin";

        /// <summary>
        /// OrganizationList
        /// </summary>
        public const string ORGANIZATION_LIST = "OrganizationList";

        /// <summary>
        /// OrganizationDetails
        /// </summary>
        public const string ORGANIZATION_DETAILS = "OrganizationDetails";

        /// <summary>
        /// WebIG
        /// </summary>
        public const string WEB_IG = "WebIG";

        /// <summary>
        /// Import
        /// </summary>
        public const string IMPORT = "Import";

        /// <summary>
        /// ImportVSAC
        /// </summary>
        public const string IMPORT_VSAC = "ImportVSAC";

        /// <summary>
        /// ImportPHINVADS
        /// </summary>
        public const string IMPORT_PHINVADS = "ImportPHINVADS";

        #endregion
    }
}