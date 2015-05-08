using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{
		private readonly IDataSyncronizerFactory _factory;
		private GeneralWithCustodianRdoSynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		public WorkspaceFieldController(IDataSyncronizerFactory factory,
			GeneralWithCustodianRdoSynchronizerFactory appDomainRdoSynchronizerFactoryFactory)
		{
			_factory = factory;
			_appDomainRdoSynchronizerFactoryFactory = appDomainRdoSynchronizerFactoryFactory;
		}

		[HttpPost]
		[Route("{workspaceID}/api/WorkspaceField/")]
		public HttpResponseMessage Post([FromBody] SyncronizerSettings settings)
		{
			Contracts.PluginBuilder.Current.SetSynchronizerFactory(_appDomainRdoSynchronizerFactoryFactory);
			var syncronizer = _factory.GetSyncronizer(Guid.Empty, settings.Settings);
			var fields = syncronizer.GetFields(settings.Settings).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}
	}

}
	