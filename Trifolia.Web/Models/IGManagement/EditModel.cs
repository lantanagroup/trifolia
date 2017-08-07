using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script;

using Trifolia.Web.Models.PermissionManagement;

namespace Trifolia.Web.Models.IGManagement
{
    public class EditModel
    {
        public EditModel()
        {
            this.ConsolidatedFormat = true;
            this.TemplateTypes = new List<TemplateTypeItem>();
            this.CustomSchematrons = new List<CustomSchematronItem>();
            this.ViewPermissions = new List<Permission>();
            this.EditPermissions = new List<Permission>();
            this.DefaultViewPermissions = new List<Permission>();
            this.DefaultEditPermissions = new List<Permission>();
            this.PreviousIgs = new Dictionary<int, string>();
            this.DeletedCustomSchematrons = new List<int>();
            this.CustomSchematrons = new List<CustomSchematronItem>();
            this.Sections = new List<Section>();
            this.Categories = new List<string>();
        }

        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string WebDisplayName { get; set; }
        public string WebDescription { get; set; }
        public string WebReadmeOverview { get; set; }
        public int? TypeId { get; set; }
        public bool ConsolidatedFormat { get; set; }
        public bool TestIg { get; set; }
        public string PreviousVersionName { get; set; }
        public int? PreviousVersionId { get; set; }
        public int? CurrentVersion { get; set; }
        public bool DisableVersionFields { get; set; }
        public int? OrganizationId { get; set; }
        public string Html { get; set; }

        public string CardinalityZeroOrOne { get; set; }
        public string CardinalityExactlyOne { get; set; }
        public string CardinalityAtLeastOne { get; set; }
        public string CardinalityZeroOrMore { get; set; }
        public string CardinalityZero { get; set; }

        public List<TemplateTypeItem> TemplateTypes { get; set; }
        public List<CustomSchematronItem> CustomSchematrons { get; set; }
        public List<int> DeletedCustomSchematrons { get; set; }
        public Dictionary<int, string> PreviousIgs { get; set; }
        public List<Permission> ViewPermissions { get; set; }
        public List<Permission> EditPermissions { get; set; }
        public List<Permission> DefaultViewPermissions { get; set; }
        public List<Permission> DefaultEditPermissions { get; set; }
        public List<Section> Sections { get; set; }
        public List<string> Categories { get; set; }
        public int? AccessManagerId { get; set; }
        public bool AllowAccessRequests { get; set; }
        public bool NotifyNewPermissions { get; set; }

        public class TemplateTypeItem
        {
            public string DefaultName { get; set; }
            public string Description { get; set; }
            public string Name { get; set; }
        }

        public class CustomSchematronItem
        {
            public int Id { get; set; }
            public string Phase { get; set; }
            public string PatternId { get; set; }
            public string PatternContent { get; set; }
        }

        public class Permission
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public PermissionTypes Type { get; set; }
        }

        public class Section
        {
            public int? Id { get; set; }
            public string Heading { get; set; }
            public string Content { get; set; }
            public int Order { get; set; }
            public int Level { get; set; }
        }
    }
}