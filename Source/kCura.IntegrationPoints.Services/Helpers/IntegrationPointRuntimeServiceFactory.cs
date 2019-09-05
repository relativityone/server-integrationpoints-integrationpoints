﻿using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Helpers
{
	public class IntegrationPointRuntimeServiceFactory : IIntegrationPointRuntimeServiceFactory
	{
		private readonly IHelper _helper;
		private readonly IServiceFactory _serviceFactory;

		public IntegrationPointRuntimeServiceFactory(IHelper helper, IServiceFactory serviceFactory)
		{
			_helper = helper;
			_serviceFactory = serviceFactory;
		}

		public IIntegrationPointService CreateIntegrationPointRuntimeService(Core.Models.IntegrationPointModel model)
		{
			return _serviceFactory.CreateIntegrationPointService(_helper);
		}
	}
}
