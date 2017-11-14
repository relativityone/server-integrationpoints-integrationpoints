using System;

namespace kCura.IntegrationPoints.Core.Logging
{
	public class WebCorrelationContext : BaseCorrelationContext
	{
		public Guid? CorrelationId { get; set; }
		public Guid? WebRequestCorrelationId { get; set; }
	}
}
