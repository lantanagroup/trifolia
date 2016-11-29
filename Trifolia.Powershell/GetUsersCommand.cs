using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Get, "TrifoliaUsers")]
    public class GetUsersCommand : BaseCommand
    {
        protected override void ProcessRecord()
        {
            var users = (from u in this.tdb.Users
                                        select new
                                        {
                                            u.Id,
                                            u.FirstName,
                                            u.LastName,
                                            u.Email,
                                            u.Phone,
                                            u.OkayToContact,
                                        }).OrderBy(y => y.FirstName).ThenBy(y => y.LastName);

            this.WriteObject(users);
        }
    }
}
