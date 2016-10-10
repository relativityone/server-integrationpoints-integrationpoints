﻿using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.Services.Metrics;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IIntegrationPointService _reader;
		private readonly IRelativityUrlHelper _urlHelper;
		private readonly Core.Services.Synchronizer.IRdoSynchronizerProvider _provider;
		private readonly ICPHelper _cpHelper;

		public IntegrationPointsAPIController(IIntegrationPointService reader,
			IRelativityUrlHelper urlHelper,
			Core.Services.Synchronizer.IRdoSynchronizerProvider provider,
			ICPHelper cpHelper)
		{
			_reader = reader;
			_urlHelper = urlHelper;
			_provider = provider;
			_cpHelper = cpHelper;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrive integration point data.")]
		public HttpResponseMessage Get(int id)
		{
			var model = new IntegrationModel();
			model.ArtifactID = id;
			if (id > 0)
			{
				model = _reader.ReadIntegrationPoint(id);
			}
			if (model.DestinationProvider == 0)
			{
				try
				{
					model.DestinationProvider = _provider.GetRdoSynchronizerId(); //hard coded for your ease of use
				}
				catch
				{
					model.DestinationProvider = 0;
				}
			}
			return Request.CreateResponse(HttpStatusCode.Accepted, model);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to save or update integration point.")]
		public HttpResponseMessage Update(int workspaceID, IntegrationModel model)
		{
			using (IMetricsManager metricManager = _cpHelper.GetServicesManager().CreateProxy<IMetricsManager>(ExecutionIdentity.CurrentUser))
			{
				using (metricManager.LogDuration(Core.Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR, 
					Guid.Empty, model.Name, MetricTargets.APMandSUM ))
				{
					int createdId = _reader.SaveIntegration(model);
					string result = _urlHelper.GetRelativityViewUrl(workspaceID, createdId, Data.ObjectTypes.IntegrationPoint);

					return Request.CreateResponse(HttpStatusCode.OK, new {returnURL = result});
				}
			}
		}
	}
}