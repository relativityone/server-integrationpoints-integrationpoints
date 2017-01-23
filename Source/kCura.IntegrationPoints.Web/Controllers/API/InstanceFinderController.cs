using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class InstanceFinderController : ApiController
    {
		private readonly ICPHelper _helper;
	    private readonly IContextContainerFactory _contextContainerFactory;
	    private readonly IManagerFactory _managerFactory;

	    public InstanceFinderController(ICPHelper helper, IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory)
	    {
		    _managerFactory = managerFactory;
		    _contextContainerFactory = contextContainerFactory;
		    _helper = helper;
	    }

	    [HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve federated instances.")]
		public HttpResponseMessage Get()
		{
			IContextContainer sourceContextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IFederatedInstanceManager federatedInstanceManager =
				_managerFactory.CreateFederatedInstanceManager(sourceContextContainer);

			var results = federatedInstanceManager.RetrieveAll();

			return Request.CreateResponse(HttpStatusCode.OK, results);
		}
	}
}
