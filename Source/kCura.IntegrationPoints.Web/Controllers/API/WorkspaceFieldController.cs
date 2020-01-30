﻿using System;
using System.Collections.Generic;
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
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{
		private readonly ISynchronizerFactory _appDomainRdoSynchronizerFactory;
		private readonly ISerializer _serializer;
		private readonly IAPILog _apiLog;

		public WorkspaceFieldController(ISynchronizerFactory appDomainRdoSynchronizerFactory, ISerializer serializer, ICPHelper helper)
		{
			_appDomainRdoSynchronizerFactory = appDomainRdoSynchronizerFactory;
			_serializer = serializer;
			_apiLog = helper.GetLoggerFactory().GetLogger().ForContext<WorkspaceFieldController>();
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve workspace fields.")]
		public HttpResponseMessage Post([FromBody] SynchronizerSettings settings)
		{
			ImportSettings importSettings = _serializer.Deserialize<ImportSettings>(settings.Settings);
			importSettings.FederatedInstanceCredentials = settings.Credentials;
			_apiLog.LogDebug($"Import Settings has been extracted successfully for workspace field retival process: {_serializer.Serialize(importSettings)}");
			IDataSynchronizer synchronizer = _appDomainRdoSynchronizerFactory.CreateSynchronizer(Guid.Empty, settings.Settings);
			var fields = synchronizer.GetFields(new DataSourceProviderConfiguration(_serializer.Serialize(importSettings), settings.Credentials)).ToList();

			List<FieldClassificationResult> result = fields.Select(x => new FieldClassificationResult
			{
				ClassificationLevel = ClassificationLevel.AutoMap,
				FieldIdentifier = x.FieldIdentifier,
				IsIdentifier = x.IsIdentifier,
				IsRequired = x.IsRequired,
				Name = x.ActualName
			}).ToList();

			return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
		}
	}
}