using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using Relativity.API;

namespace Relativity.IntegrationPoints.Services.Helpers
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

        public IIntegrationPointService CreateIntegrationPointRuntimeService(IntegrationPointDto dto)
        {
            return _serviceFactory.CreateIntegrationPointService(_helper);
        }
    }
}
