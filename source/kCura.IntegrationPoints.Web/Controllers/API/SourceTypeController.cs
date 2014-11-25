using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{

    public class SourceTypeController : ApiController
    {
	    [HttpGet]
	    public HttpResponseMessage Get()
	    {
		    var list = new List<dynamic>();
				list.Add(new { name="Ldap", value= "LDAP" });
		    return Request.CreateResponse(HttpStatusCode.OK, list);
	    }
    }
}
