using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Web.Logging;
using Relativity.API;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Web.MessageHandlers
{
	public class CorrelationIdHandler : DelegatingHandler
	{
		private readonly IAPILog _logger;
		private readonly ICPHelper _helper;
		private readonly IWebCorrelationContextProvider _webCorrelationContextProvider;

		public const string WEB_CORRELATION_ID_HEADER_NAME = "X-Correlation-ID";

		// TODO remove helper dependency
		public CorrelationIdHandler(ICPHelper helper, IWebCorrelationContextProvider webCorrelationContextProvider)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<CorrelationIdHandler>(); // TODO inject logger
			_webCorrelationContextProvider = webCorrelationContextProvider;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			int userId = GetValue(() => _helper.GetAuthenticationManager().UserInfo.ArtifactID, "Error while retrieving User Id");

			WebActionContext actionContext = _webCorrelationContextProvider.GetDetails(request.RequestUri.ToString(), userId);
			var correlationContext = new WebCorrelationContext
			{
				WebRequestCorrelationId = GetValue(request.GetCorrelationId, "Error while retrieving web request correlation id"),
				UserId = userId,
				WorkspaceId = GetValue(_helper.GetActiveCaseID, "Error while retrieving Workspace Id"),
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