using System.Diagnostics;
using Microsoft.AspNet.SignalR.Hubs;
using Relativity.API;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.SignalRHubs
{
	public class IntegrationPointDataHubErrorHandlingPipelineModule : HubPipelineModule
	{
		private readonly IAPILog _logger;

		public IntegrationPointDataHubErrorHandlingPipelineModule()
		{
			ICPHelper helper = ConnectionHelper.Helper();
			_logger = helper.GetLoggerFactory().GetLogger();
		}


		protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
		{
			_logger.LogError(exceptionContext.Error, "SignalR exception occurred: hub: {hub} method: {method}", invokerContext.MethodDescriptor.Hub.Name, invokerContext.MethodDescriptor.Name);
			base.OnIncomingError(exceptionContext, invokerContext);
		}
	}
}