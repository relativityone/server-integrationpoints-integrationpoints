﻿using System;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.Monitoring.Instrumentation.Model;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Monitoring.Instrumentation
{
	internal class ExternalServiceLogsInstrumentation : IExternalServiceInstrumentation, IExternalServiceInstrumentationStarted
	{
		private readonly IAPILog _logger;
		private readonly InstrumentationServiceCallContext _serviceCallContext;

		public ExternalServiceLogsInstrumentation(InstrumentationServiceCallContext callContext, IAPILog logger)
		{
			_logger = logger.ForContext<ExternalServiceLogsInstrumentation>();
			_serviceCallContext = callContext;
		}

		public IExternalServiceInstrumentationStarted Started()
		{
			return this;
		}

		public void Completed()
		{
		}

		public void Failed(string reason)
		{
			_logger.LogError("Call to external service failed. Service: {@serviceCallContext}, reason: {reason}", _serviceCallContext, reason);
		}

		public void Failed(Exception ex)
		{
			_logger.LogError(ex, "Call to external service failed. Service: {@serviceCallContext}", _serviceCallContext);
		}
	}
}
