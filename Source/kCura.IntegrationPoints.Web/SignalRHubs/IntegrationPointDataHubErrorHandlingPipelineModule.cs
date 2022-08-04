using Microsoft.AspNet.SignalR.Hubs;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.SignalRHubs
{
    public class IntegrationPointDataHubErrorHandlingPipelineModule : HubPipelineModule
    {
        private readonly IAPILog _logger;

        public IntegrationPointDataHubErrorHandlingPipelineModule(IAPILog logger)
        {
            _logger = logger;
        }


        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            _logger.LogError(exceptionContext.Error, "SignalR exception occurred: hub: {hub} method: {method}", invokerContext.MethodDescriptor.Hub.Name, invokerContext.MethodDescriptor.Name);
            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }
}