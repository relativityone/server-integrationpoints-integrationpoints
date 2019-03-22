using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Helpers
{
	public class IntegrationPointRuntimeServiceFactory : IIntegrationPointRuntimeServiceFactory
	{
		private readonly IHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IServiceFactory _serviceFactory;
		private readonly IIntegrationPointSerializer _serializer;

		public IntegrationPointRuntimeServiceFactory(IHelper helper, IHelperFactory helperFactory, IServiceFactory serviceFactory, IIntegrationPointSerializer serializer)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_serviceFactory = serviceFactory;
			_serializer = serializer;
		}

		public IIntegrationPointService CreateIntegrationPointRuntimeService(Core.Models.IntegrationPointModel model)
		{
			DestinationConfiguration importSettings = _serializer.Deserialize<DestinationConfiguration>(model.Destination);
			IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, importSettings.FederatedInstanceArtifactId, model.SecuredConfiguration);

			return _serviceFactory.CreateIntegrationPointService(_helper, targetHelper);
		}
	}
}
