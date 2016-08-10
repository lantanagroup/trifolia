using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Trifolia.Shared;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Web.Controllers
{
    public class SchemaTypesController : ApiController
    {
        private IObjectRepository tdb;
        private Trifolia.Shared.Helper helper;
        private Trifolia.Shared.SimpleSchema.SimpleSchemaFactory schemaFactory;

        //dependency injection for unit tests
        public SchemaTypesController(): this(
            DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress), 
            new Trifolia.Shared.Helper(),
            new Trifolia.Shared.SimpleSchema.SimpleSchemaFactory()) {}

        public SchemaTypesController(IObjectRepository tdb, Trifolia.Shared.Helper helper, SimpleSchema.SimpleSchemaFactory schemaFactory) { this.tdb = tdb; this.helper = helper; this.schemaFactory = schemaFactory; }

        /// <summary>
        /// Gets all of the types in a schema regardless if they are prefered or not
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/schematypes/{id}/all")]
        public IEnumerable<string> GetAll(int id)
        {
            //id, name, schemalocation
            ImplementationGuideType igType = tdb.ImplementationGuideTypes.Single(y => y.Id == id);

            var schema = schemaFactory.Create(helper.GetIGSimplifiedSchemaLocation2(igType));

            var types = from t in schema.ComplexTypes
                             orderby t.Name
                             select t.Name;

            return types;
        }

        // GET api/schematypes/5
        //  [Route("api/schemas/types/{id}")] I like this better, but let's not confuse things right now...
        /// <summary>
        /// Gets all the prefered types in a schema
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<string> Get(int id)
        {
            var types = from dt in tdb.ImplementationGuideTypeDataTypes
                            where dt.ImplementationGuideTypeId == id
                            select dt.DataTypeName;

            return types;
        }
    }
}
