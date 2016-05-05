using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using Trifolia.Web.Formatters.FHIR.DSTU1;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU1
{
    public class DSTU1ConfigAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Clear();
            controllerSettings.Formatters.Add(new JSONFHIRMediaTypeFormatter());
            controllerSettings.Formatters.Add(new XMLFHIRMediaTypeFormatter());
        }
    }
}