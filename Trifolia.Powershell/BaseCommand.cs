using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Powershell
{
    public abstract class BaseCommand : PSCmdlet
    {
        protected IObjectRepository tdb;

        protected override void BeginProcessing()
        {
            AppConfig.Change();

            this.tdb = new TemplateDatabaseDataSource();
        }

        protected override void StopProcessing()
        {
            AppConfig.Change();
        }
    }
}
