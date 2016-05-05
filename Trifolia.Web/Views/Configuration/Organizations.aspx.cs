using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using TemplateDatabase.Shared;

namespace TemplateDatabase.Web.Account.OrganizationManagement
{
    public partial class Organizations : System.Web.UI.Page
    {
        #region Private Fields

        private string _urlReferrer = null;

        #endregion

        #region Properties

        public string RequestMessage
        {
            get
            {
                return Request["Message"];
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (this.Request.UrlReferrer != null) _urlReferrer = this.Request.UrlReferrer.ToString();
                if (!string.IsNullOrEmpty(RequestMessage))
                {
                    this.MessageLabel.Text = RequestMessage;
                    this.MessageLabel.Visible = true;
                }

                OrganizationDisplay.Initialize();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            OrganizationDisplay.Save();

            Response.Redirect("Organizations.aspx?Message=Successfully saved organizations @ " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/home/index");
        }

        [Serializable]
        public class OrganizationDisplay
        {
            #region Session Properties

            private const string AllOrganizationsName = "AllOrganizations";
            private const string DeletedOrganizationsName = "DeletedOrganizations";

            public static List<OrganizationDisplay> AllOrgainzations
            {
                get
                {
                    if (HttpContext.Current.Session[AllOrganizationsName] == null)
                        HttpContext.Current.Session[AllOrganizationsName] = new List<OrganizationDisplay>();

                    return (List<OrganizationDisplay>)HttpContext.Current.Session[AllOrganizationsName];
                }
                set
                {
                    HttpContext.Current.Session[AllOrganizationsName] = value;
                }
            }

            public static List<OrganizationDisplay> DeletedOrgainzations
            {
                get
                {
                    if (HttpContext.Current.Session[DeletedOrganizationsName] == null)
                        HttpContext.Current.Session[DeletedOrganizationsName] = new List<OrganizationDisplay>();

                    return (List<OrganizationDisplay>)HttpContext.Current.Session[DeletedOrganizationsName];
                }
                set
                {
                    HttpContext.Current.Session[DeletedOrganizationsName] = value;
                }
            }

            #endregion

            #region Properties

            private bool isNew;

            public bool IsNew
            {
                get { return isNew; }
                set { isNew = value; }
            }
            private int id;

            public int Id
            {
                get { return id; }
                set { id = value; }
            }
            private string name;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            private string contactName;

            public string ContactName
            {
                get { return contactName; }
                set { contactName = value; }
            }
            private string contactPhone;

            public string ContactPhone
            {
                get { return contactPhone; }
                set { contactPhone = value; }
            }
            private string contactEmail;

            public string ContactEmail
            {
                get { return contactEmail; }
                set { contactEmail = value; }
            }

            #endregion

            #region CRUD

            public static void Initialize()
            {
                using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
                {
                    AllOrgainzations = (from o in tdb.Organizations
                                        select new OrganizationDisplay()
                                        {
                                            Id = o.Id,
                                            Name = o.Name,
                                            ContactName = o.ContactName,
                                            ContactPhone = o.ContactPhone,
                                            ContactEmail = o.ContactEmail
                                        }).ToList();
                }
            }

            public static void Save()
            {
                using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
                {
                    foreach (OrganizationDisplay cOrganizationDisplay in DeletedOrgainzations)
                    {
                        Organization foundOrganization = tdb.Organizations.SingleOrDefault(y => y.Id == cOrganizationDisplay.Id);

                        if (foundOrganization != null)
                            tdb.Organizations.DeleteObject(foundOrganization);
                    }

                    foreach (OrganizationDisplay cOrganizationDisplay in AllOrgainzations)
                    {
                        Organization organization = null;

                        if (cOrganizationDisplay.IsNew)
                        {
                            organization = new Organization();
                            tdb.Organizations.AddObject(organization);
                        }
                        else
                        {
                            organization = tdb.Organizations.SingleOrDefault(y => y.Id == cOrganizationDisplay.id);
                        }

                        if (organization != null)
                        {
                            organization.Name = cOrganizationDisplay.Name;
                            organization.ContactName = cOrganizationDisplay.ContactName;
                            organization.ContactPhone = cOrganizationDisplay.ContactPhone;
                            organization.ContactEmail = cOrganizationDisplay.ContactEmail;
                        }
                    }

                    tdb.SaveChanges();
                }
            }

            public static List<OrganizationDisplay> GetOrganizations()
            {
                return AllOrgainzations;
            }

            public static void AddOrganization(OrganizationDisplay organizationDisplay)
            {
                List<OrganizationDisplay> allOrganizations = AllOrgainzations.ToList();
                
                organizationDisplay.IsNew = true;
                organizationDisplay.Id = allOrganizations.Count > 0 ? allOrganizations.Max(y => y.Id) + 1 : 1;

                allOrganizations.Add(organizationDisplay);
                AllOrgainzations = allOrganizations;
            }

            public static void UpdateOrganization(OrganizationDisplay organizationDisplay)
            {
                List<OrganizationDisplay> allOrganizations = AllOrgainzations.ToList();

                OrganizationDisplay foundOrganization = allOrganizations.SingleOrDefault(y => y.Id == organizationDisplay.Id);

                if (foundOrganization != null)
                {
                    foundOrganization.Name = organizationDisplay.Name;
                    foundOrganization.ContactName = organizationDisplay.ContactName;
                    foundOrganization.ContactPhone = organizationDisplay.ContactPhone;
                    foundOrganization.ContactEmail = organizationDisplay.ContactEmail;
                }

                AllOrgainzations = allOrganizations;
            }

            public static void DeleteOrganization(OrganizationDisplay organizationDisplay)
            {
                List<OrganizationDisplay> allOrganizations = AllOrgainzations.ToList();
                List<OrganizationDisplay> deletedOrganizations = DeletedOrgainzations.ToList();

                OrganizationDisplay foundOrganization = allOrganizations.SingleOrDefault(y => y.Id == organizationDisplay.Id);

                if (foundOrganization != null)
                {
                    allOrganizations.Remove(foundOrganization);

                    if (foundOrganization.IsNew != true)
                        deletedOrganizations.Add(foundOrganization);
                }

                AllOrgainzations = allOrganizations;
                DeletedOrgainzations = deletedOrganizations;
            }

            #endregion
        }
    }
}