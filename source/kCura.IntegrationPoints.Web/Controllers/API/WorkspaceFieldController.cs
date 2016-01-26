using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{
		// TODO: This concrete class should be replaced by an interface -- biedrzycki: Jan 25, 2016
		private readonly kCura.IntegrationPoints.Contracts.ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		public WorkspaceFieldController(ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory)
		{
			_appDomainRdoSynchronizerFactoryFactory = appDomainRdoSynchronizerFactoryFactory;
		}

		[HttpPost]
		[Route("{workspaceID}/api/WorkspaceField/")]
		public HttpResponseMessage Post([FromBody] SyncronizerSettings settings)
		{
			IDataSynchronizer syncronizer = _appDomainRdoSynchronizerFactoryFactory.CreateSyncronizer(Guid.Empty,
				settings.Settings);
			var fields = syncronizer.GetFields(settings.Settings).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}
	}

}
	