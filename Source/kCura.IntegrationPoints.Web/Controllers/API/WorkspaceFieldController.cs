using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{
		private readonly ISynchronizerFactory _appDomainRdoSynchronizerFactory;
		private readonly ISerializer _serializer;

		public WorkspaceFieldController(ISynchronizerFactory appDomainRdoSynchronizerFactory, ISerializer serializer)
		{
			_appDomainRdoSynchronizerFactory = appDomainRdoSynchronizerFactory;
			_serializer = serializer;
		}

		[HttpPost]
		[Route("{workspaceID}/api/WorkspaceField/")]
		[LogApiExceptionFilter(Message = "Unable to retrieve workspace fields.")]
		public HttpResponseMessage Post([FromBody] SynchronizerSettings settings)
		{
			ImportSettings importSettings = _serializer.Deserialize<ImportSettings>(settings.Settings);
			importSettings.FederatedInstanceCredentials = settings.Credentials;
			IDataSynchronizer synchronizer = _appDomainRdoSynchronizerFactory.CreateSynchronizer(Guid.Empty, settings.Settings, settings.Credentials);
			var fields = synchronizer.GetFields(_serializer.Serialize(importSettings)).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}
	}
}