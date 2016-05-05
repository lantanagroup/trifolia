using System;

using Trifolia.DB;
using Trifolia.Config;

namespace Trifolia.Web.Extensions
{
    public static class TemplateTypeExtensions
    {
        public static string GetAbbreviation(this TemplateType templateType)
        {
            BookmarkSection bookmarkSection = BookmarkSection.GetSection();

            if (bookmarkSection.TemplateTypes == null)
                return null;

            foreach (BookmarkTemplateTypeElement cTemplateTypeAbbr in bookmarkSection.TemplateTypes)
            {
                if (cTemplateTypeAbbr.TemplateTypeName.ToLower() == templateType.Name.ToLower())
                {
                    return cTemplateTypeAbbr.BookmarkAbbreviation;
                }
            }

            return null;
        }
    }
}