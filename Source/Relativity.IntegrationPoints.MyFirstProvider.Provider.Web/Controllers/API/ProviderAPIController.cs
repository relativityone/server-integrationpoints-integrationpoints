using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Relativity.IntegrationPoints.MyFirstProvider.Web.Controllers.API
{
    public class ProviderAPIController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage GetViewFields([FromBody] object data)
        {
            string fileLocation = data.ToString();
            var model = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("File Location", fileLocation)
            };
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

    }
}