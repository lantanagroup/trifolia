using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Web.Models.IGManagement
{
    public class MyAccessRequestsModel
    {
        public MyAccessRequestsModel()
        {
            this.MyApprovals = new List<Request>();
            this.MyRequests = new List<Request>();
        }

        public List<MyAccessRequestsModel.Request> MyApprovals { get; set; }
        public List<MyAccessRequestsModel.Request> MyRequests { get; set; }

        public class Request
        {
            public Request()
            {

            }

            public Request(ImplementationGuideAccessRequest igAccessRequest)
            {
                this.Id = igAccessRequest.Id;
                this.ImplementationGuideId = igAccessRequest.ImplementationGuideId;
                this.ImplementationGuideName = igAccessRequest.ImplementationGuide.NameWithVersion;
                this.RequestUserId = igAccessRequest.RequestUserId;
                this.RequestUserName = string.Format("{0} {1}", igAccessRequest.RequestUser.FirstName, igAccessRequest.RequestUser.LastName);
                this.RequestDate = igAccessRequest.RequestDate;
                this.RequestMessage = igAccessRequest.Message;
                this.RequestPermission = igAccessRequest.Permission;
            }

            public int Id { get; set; }
            public int ImplementationGuideId { get; set; }
            public string ImplementationGuideName { get; set; }
            public int RequestUserId { get; set; }
            public string RequestUserName { get; set; }
            public DateTime RequestDate { get; set; }
            public string RequestPermission { get; set; }
            public string RequestMessage { get; set; }
        }
    }
}