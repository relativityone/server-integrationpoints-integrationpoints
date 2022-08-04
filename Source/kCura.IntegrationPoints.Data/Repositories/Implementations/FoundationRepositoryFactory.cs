using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;
using Relativity.API.Foundation;
using Relativity.API.Foundation.Repositories;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class FoundationRepositoryFactory : IFoundationRepositoryFactory
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

        public FoundationRepositoryFactory(IServicesMgr servicesMgr, IExternalServiceInstrumentationProvider instrumentationProvider)
        {
            _servicesMgr = servicesMgr;
            _instrumentationProvider = instrumentationProvider;
        }

        public T GetRepository<T>(int workspaceId) where T : IRepository
        {
            IExternalServiceSimpleInstrumentation gatewayInstrumentation = _instrumentationProvider.CreateSimple(
                ExternalServiceTypes.API_FOUNDATION, 
                nameof(IWorkspaceGateway), 
                nameof(IWorkspaceGateway.GetWorkspaceContext));

            IExternalServiceSimpleInstrumentation contextInstrumentation = _instrumentationProvider.CreateSimple(
                ExternalServiceTypes.API_FOUNDATION,
                nameof(IWorkspaceContext),
                nameof(IWorkspaceContext.CreateRepository));

            using (var workspaceGateway = _servicesMgr.CreateProxy<IWorkspaceGateway>(ExecutionIdentity.CurrentUser))
            {
                IWorkspaceContext workspaceContext = gatewayInstrumentation
                    .Execute(() => workspaceGateway.GetWorkspaceContext(workspaceId));
                T repository = contextInstrumentation
                    .Execute(() => workspaceContext.CreateRepository<T>());
                return repository;
            }
        }
    }
}
