using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Web.Models.LandingPage
{
    public class ValueSetSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string Code { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Description { get; set; }
        public int NumberOfMembers { get; set; }

        public static ValueSetSummaryViewModel AdaptFromValueSet(ValueSet v)
        {
            return new ValueSetSummaryViewModel()
            {
                Id = v.Id,
                Name = v.Name,
                Oid = v.Oid,
                Code = v.Code,
                LastUpdated = v.LastUpdate,
                Description = v.Description,
                NumberOfMembers = v.Members.Count
            };
        }
    }
}