using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
    public interface IIntegrationPointValidator
    {
        ValidationResult Validate(
            IntegrationPointDtoBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int userId);
    }
}
