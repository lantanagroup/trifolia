using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Get, "TrifoliaImplementationGuides")]
    public class GetImplementationGuides : BaseCommand
    {
        protected override void ProcessRecord()
        {
            var implementationGuides = (from ig in this.tdb.ImplementationGuides
                                        select new
                                        {
                                            ig.Id,
                                            ig.Name,
                                            ig.Version,
                                            ig.DisplayName,
                                            Status = ig.PublishStatus != null ? ig.PublishStatus.Status : null,
                                            ig.PublishDate,
                                            Type = ig.ImplementationGuideType.Name
                                        }).OrderBy(y => y.Name);

            this.WriteObject(implementationGuides);
        }
    }
}
