using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Infrastructure.MessageHandlers
{
	public class CorrelationIdHandler : DelegatingHandler
	{
		private readonly IAPILog _logger;
		private readonly ICPHelper _helper;
		private readonly Func<IWebCorrelationContextProvider> _webCorrelationContextProviderFactory;
		private readonly Func<IWorkspaceIdProvider> _workspaceIdProviderFactory;

		public const string WEB_CORRELATION_ID_HEADER_NAME = "X-Correlation-ID";

		// TODO remove helper dependency
		public CorrelationIdHandler(
			ICPHelper helper, 
			Func<IWebCorrelationContextProvider> webCorrelationContextProviderFactoryFactory, 
			Func<IWorkspaceIdProvider> workspaceIdProviderFactoryFactory
		)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<CorrelationIdHandler>(); // TODO inject logger
			_webCorrelationContextProviderFactory = webCorrelationContextProviderFactoryFactory;
			_workspaceIdProviderFactory = workspaceIdProviderFactoryFactory;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			int userId = GetValue(() => _helper.GetAuthenticationManager().UserInfo.ArtifactID, "Error while retrieving User Id"); // TODO change it

			WebActionContext actionContext = _webCorrelationContextProviderFactory().GetDetails(request.RequestUri.ToString(), userId);
			var correlationContext = new WebCorrelationContext
			{
				WebRequestCorrelationId = GetValue(request.GetCorrelationId, "Error while retrieving web request correlation id"),
				UserId = userId,
				WorkspaceId = GetValue(_workspaceIdProviderFactory().GetWorkspaceId, "Error while retrieving Workspace Id"),
				ActionName = actionContext.ActionName,
				CorrelationId = actionContext.ActionGuid
			};

			using (_logger.LogContextPushProperties(correlationContext))
			{
				_logger.LogDebug($"Integration Point Web Request: {request.RequestUri}");

				string correlationId = correlationContext.CorrelationId?.ToString();
				request.Headers.Add(WEB_CORRELATION_ID_HEADER_NAME, correlationId);
				HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
				response.Headers.Add(WEB_CORRELATION_ID_HEADER_NAME, correlationId);
				return response;
			}
		}

		private T GetValue<T>(Func<T> valueGetter, string errorMessage)
		{
			try
			{
				return valueGetter();
			}
			catch (Exception e)
			{
				_logger.LogError(e, errorMessage);
				throw new CorrelationContextCreationException(errorMessage, e);
			}
		}
	}
}