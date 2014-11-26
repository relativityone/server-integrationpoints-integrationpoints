using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class TestController : ApiController
    {
			[HttpGet]
	    public HttpResponseMessage Test()
			{
				var list = new List<Tuple<string, int>>();
				list.Add(new Tuple<string, int>("test1",1234));
				list.Add(new Tuple<string, int>("test2", 5678));

				return Request.CreateResponse(HttpStatusCode.OK, list);
			}

    }
}
