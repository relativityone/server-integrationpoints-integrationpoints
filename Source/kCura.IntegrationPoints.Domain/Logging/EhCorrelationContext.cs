
using System;

namespace kCura.IntegrationPoints.Domain.Logging
{
    public class EhCorrelationContext : BaseCorrelationContext
    {
        public Guid CorrelationId { get; set; }

        public Guid InstallerGuid { get; set; }
    }
}
