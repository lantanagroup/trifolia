using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Shared;

namespace Trifolia.Terminology
{
    /// <summary>
    /// Summary description for RestfulVocabulary
    /// </summary>
    public class RestfulVocabulary : IHttpHandler
    {
        private HttpContext context;

        #region Properties

        public string ValueSetOid
        {
            get
            {
                if (context == null)
                    return null;

                return context.Request["ValueSetOid"];
            }
        }

        public int? ImplementationGuideId
        {
            get
            {
                if (context == null || context.Request["ImplementationGuideId"] == null)
                    return null;

                return new Nullable<int>(int.Parse(context.Request["ImplementationGuideId"]));
            }
        }

        public int MaxValueSetMembers
        {
            get
            {
                if (context == null || context.Request["MaxValueSetMembers"] == null)
                    return 0;

                int retValue = 0;

                Int32.TryParse(context.Request["MaxValueSetMembers"], out retValue);

                return retValue;
            }
        }

        #endregion

        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            this.context = context;

            if (string.IsNullOrEmpty(ValueSetOid) && ImplementationGuideId == null)
                return;

            // TODO: Make this an actual call to the service, rather than an instatiation of the service
            VocabularyService service = new VocabularyService();

            context.Response.Clear();
            context.Response.ContentType = "text/xml";

            if (!string.IsNullOrEmpty(ValueSetOid))
            {
                context.Response.Write(service.GetValueSet(ValueSetOid, (int) VocabularyOutputType.Default, "UTF-8"));
            }
            else if (ImplementationGuideId != null)
            {
                context.Response.Write(service.GetImplementationGuideVocabulary(ImplementationGuideId.Value, MaxValueSetMembers, (int)VocabularyOutputType.Default, "UTF-8"));
            }

            context.Response.Flush();
            context.Response.End();
        }

        #endregion
    }
}