using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared
{
    public class JiraIssue
    {
        public JiraIssue()
        {
            this.fields = new JiraFields();
        }

        public JiraFields fields { get; set; }

        public class JiraFields
        {
            public JiraFields()
            {
                this.project = new JiraProject();
                this.priority = new JiraPriority();
                this.issuetype = new Shared.JiraIssueType();
                this.reporter = new JiraUser();
                this.labels = new List<string>();
            }

            public JiraProject project { get; set; }
            public string summary { get; set; }
            public JiraPriority priority { get; set; }
            public string description { get; set; }
            public JiraIssueType issuetype { get; set; }
            public JiraUser reporter { get; set; }
            public List<string> labels { get; set; }
        }
    }

    public class JiraProject
    {
        public string key { get; set; }
    }

    public class JiraPriority
    {
        public string id { get; set; }
    }

    public class JiraIssueType
    {
        public string id { get; set; }
    }

    public class JiraUser
    {
        public string name { get; set; }
    }
}
