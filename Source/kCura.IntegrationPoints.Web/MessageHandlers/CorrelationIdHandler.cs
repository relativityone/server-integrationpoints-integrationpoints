using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.MessageHandlers
{
	public class CorrelationIdHandler : DelegatingHandler
	{
		private readonly Lazy<IAPILog> _apiLogLocal;
		private readonly ICPHelper _helper;

		public CorrelationIdHandler(ICPHelper helper)
		{
			_helper = helper;
			_apiLogLocal = new Lazy<IAPILog>(() => helper.GetLoggerFactory().GetLogger().ForContext<CorrelationIdHandler>());
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var correlationContext = new WebCorrelationContext
			{
				WebRequestCorrelationId = GetValue(request.GetCorrelationId, "Error while retrieving web request correlation id"),
				UserId = GetValue(() => _helper.GetAuthenticationManager().UserInfo.ArtifactID, "Error while retrieving User Id"),
				WorkspaceId = GetValue(_helper.GetActiveCaseID, "Error while retrieving Workspace Id")
			};

			using (_apiLogLocal.Value.LogContextPushProperties(correlationContext))
			{
				_apiLogLocal.Value.LogDebug($"Integration Point Web Request: {request.RequestUri}");
				return base.SendAsync(request, cancellationToken);
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