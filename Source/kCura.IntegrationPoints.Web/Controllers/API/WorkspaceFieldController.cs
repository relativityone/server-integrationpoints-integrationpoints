using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{
		private readonly ISynchronizerFactory _appDomainRdoSynchronizerFactory;
		public WorkspaceFieldController(ISynchronizerFactory appDomainRdoSynchronizerFactory)
		{
			_appDomainRdoSynchronizerFactory = appDomainRdoSynchronizerFactory;
		}

		[HttpPost]
		[Route("{workspaceID}/api/WorkspaceField/")]
		[LogApiExceptionFilter(Message = "Unable to retrieve workspace fields.")]
		public HttpResponseMessage Post([FromBody] SynchronizerSettings settings)
		{
			IDataSynchronizer synchronizer = _appDomainRdoSynchronizerFactory.CreateSynchronizer(Guid.Empty, settings.Settings);
			var fields = synchronizer.GetFields(settings.Settings).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}
	}
}