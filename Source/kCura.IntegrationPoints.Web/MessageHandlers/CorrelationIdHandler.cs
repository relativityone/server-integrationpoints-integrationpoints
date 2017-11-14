using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.MessageHandlers
{
	public class CorrelationIdHandler : DelegatingHandler
	{
		private readonly Lazy<IAPILog> _apiLogLocal;

		public CorrelationIdHandler(ICPHelper helper)
		{
			_apiLogLocal = new Lazy<IAPILog>( () => helper.GetLoggerFactory().GetLogger().ForContext<CorrelationIdHandler>());
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Guid webRequestId = request.GetCorrelationId();
			using (_apiLogLocal.Value.LogContextPushProperty("WebRequestId", webRequestId))
			{
				_apiLogLocal.Value.LogDebug($"Integration Point Web Request: {request.RequestUri}");
				return base.SendAsync(request, cancellationToken);
			}
		}
	}
}