
using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Validation
{
    public class ValidationContext
    {
        public IntegrationPointModelBase Model { get; set; }
        public SourceProvider SourceProvider { get; set; }
        public DestinationProvider DestinationProvider { get; set; }
        public IntegrationPointType IntegrationPointType { get; set; }
        public Guid ObjectTypeGuid { get; set; }
        public int UserId { get; set; }
    }
}
