using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{
		private readonly kCura.IntegrationPoints.Contracts.ISynchronizerFactory _appDomainRdoSynchronizerFactory;
		public WorkspaceFieldController(ISynchronizerFactory appDomainRdoSynchronizerFactory)
		{
			_appDomainRdoSynchronizerFactory = appDomainRdoSynchronizerFactory;
		}

		[HttpPost]
		[Route("{workspaceID}/api/WorkspaceField/")]
		public HttpResponseMessage Post([FromBody] SyncronizerSettings settings)
		{
			IDataSynchronizer syncronizer = _appDomainRdoSynchronizerFactory.CreateSyncronizer(Guid.Empty, settings.Settings);
			var fields = syncronizer.GetFields(settings.Settings).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}
	}
}