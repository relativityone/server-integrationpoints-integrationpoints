using Relativity.IntegrationPoints.FieldsMapping.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public interface IFieldsMappingValidator
    {
        Task<FieldMappingValidationResult> ValidateAsync(IEnumerable<FieldMap> map, int sourceWorkspaceID, int destinationWorkspaceID, int sourceArtifactTypeId, int destinationArtifactTypeId);
    }
}
