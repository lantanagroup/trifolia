using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Export
{
    public enum DocumentTableOptions
    {
        None = 0,
        Both = 1,
        List = 2,
        Containment = 3
    }

    public enum TemplateTableOptions
    {
        None = 0,
        Both = 1,
        Context = 2,
        ConstraintOverview = 3
    }

    public enum EncodingOptions
    {
        UTF8 = 0,
        UNICODE = 1
    }

    public enum TemplateSortOrderOptions
    {
        Alphabetically = 0,
        AlphaHierarchically = 1
    }
}