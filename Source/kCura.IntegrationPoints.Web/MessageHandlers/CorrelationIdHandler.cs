using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Logging.Web;
using kCura.IntegrationPoints.Web.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.MessageHandlers
{
	public class CorrelationIdHandler : DelegatingHandler
	{
		private readonly Lazy<IAPILog> _apiLogLocal;
		private readonly ICPHelper _helper;
		private readonly IWebCorrelationContextProvider _webCorrelationContextProvider;

		public CorrelationIdHandler(ICPHelper helper, IWebCorrelationContextProvider webCorrelationContextProvider)
		{
			_helper = helper;
			_apiLogLocal = new Lazy<IAPILog>(() => helper.GetLoggerFactory().GetLogger().ForContext<CorrelationIdHandler>());
			_webCorrelationContextProvider = webCorrelationContextProvider;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			int userId = GetValue(() => _helper.GetAuthenticationManager().UserInfo.ArtifactID, "Error while retrieving User Id");

			var actionContext = _webCorrelationContextProvider.GetDetails(request.RequestUri.ToString(), userId);
			var correlationContext = new WebCorrelationContext
			{
				WebRequestCorrelationId = GetValue(request.GetCorrelationId, "Error while retrieving web request correlation id"),
				UserId = userId,
				WorkspaceId = GetValue(_helper.GetActiveCaseID, "Error while retrieving Workspace Id"),
				ActionName = actionContext.ActionName,
				CorrelationId = actionContext.ActionGuid
			};
			
			using (_apiLogLocal.Value.LogContextPushProperties(correlationContext))
			{
				_apiLogLocal.Value.LogDebug($"Integration Point Web Request: {request.RequestUri}");
				return base.SendAsync(request, cancellationToken).ContinueWith(task =>
				{
					HttpResponseMessage response = task.Result;
					response.Headers.Add("X-Correlation-ID", correlationContext.CorrelationId?.ToString());
					return response;
				}, cancellationToken);
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
				_apiLogLocal.Value.LogError(e, errorMessage);
				throw new CorrelationContextCreationException(errorMessage, e);
			}
		}
	}
}