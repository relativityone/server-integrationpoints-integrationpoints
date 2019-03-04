using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;
using Relativity.API;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Web.Infrastructure.MessageHandlers
{
	public class CorrelationIdHandler : DelegatingHandler
	{
		private readonly IAPILog _logger;
		private readonly Func<IWebCorrelationContextProvider> _webCorrelationContextProviderFactory;
		private readonly Func<IWorkspaceContext> _workspaceContextFactory;
		private readonly Func<IUserContext> _userContextFactory;

		public const string WEB_CORRELATION_ID_HEADER_NAME = "X-Correlation-ID";

		public CorrelationIdHandler(
			IAPILog logger,
			Func<IWebCorrelationContextProvider> webCorrelationContextProviderFactoryFactory,
			Func<IWorkspaceContext> workspaceContextFactory,
			Func<IUserContext> userContextFactory
		)
		{
			_logger = logger.ForContext<CorrelationIdHandler>();
			_webCorrelationContextProviderFactory = webCorrelationContextProviderFactoryFactory;
			_workspaceContextFactory = workspaceContextFactory;
			_userContextFactory = userContextFactory;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			WebCorrelationContext correlationContext = GetWebActionContext(request);

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

		private WebCorrelationContext GetWebActionContext(HttpRequestMessage request)
		{
			IUserContext userContext = _userContextFactory();
			IWorkspaceContext workspaceContext = _workspaceContextFactory();
			IWebCorrelationContextProvider webCorrelationContextProvider = _webCorrelationContextProviderFactory();

			return GetWebActionContext(request, userContext, workspaceContext, webCorrelationContextProvider);
		}

		private WebCorrelationContext GetWebActionContext(
			HttpRequestMessage request, 
			IUserContext userContext, 
			IWorkspaceContext workspaceContext, 
			IWebCorrelationContextProvider webCorrelationContextProvider)
		{
			int userId = GetValueOrThrowException(userContext.GetUserID, "Error while retrieving User Id");

			WebActionContext actionContext = webCorrelationContextProvider.GetDetails(request.RequestUri.ToString(), userId);
			var correlationContext = new WebCorrelationContext
			{
				WebRequestCorrelationId = GetValueOrThrowException(request.GetCorrelationId, "Error while retrieving web request correlation id"),
				UserId = userId,
				WorkspaceId = GetValueOrThrowException(workspaceContext.GetWorkspaceId, "Error while retrieving Workspace Id"),
				ActionName = actionContext.ActionName,
				CorrelationId = actionContext.ActionGuid
			};
			return correlationContext;
		}

		private T GetValueOrThrowException<T>(Func<T> valueGetter, string errorMessage)
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