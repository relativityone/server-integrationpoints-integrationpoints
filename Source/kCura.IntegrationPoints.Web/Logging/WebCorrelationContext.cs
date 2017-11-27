using System;
using kCura.IntegrationPoints.Core.Logging;

namespace kCura.IntegrationPoints.Web.Logging
{
	public class WebCorrelationContext : BaseCorrelationContext
	{
		public Guid? CorrelationId { get; set; }
		public Guid? WebRequestCorrelationId { get; set; }
	}
}
