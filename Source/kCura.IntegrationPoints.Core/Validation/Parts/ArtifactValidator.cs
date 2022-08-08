using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ArtifactValidator : BasePartsValidator<int>
    {
        private readonly IArtifactService _artifactService;
        private readonly int _workspaceArtifactId;
        private readonly string _artifactTypeName;

        public ArtifactValidator(IArtifactService artifactService, int workspaceArtifactId, string artifactTypeName)
        {
            _artifactService = artifactService;
            _workspaceArtifactId = workspaceArtifactId;
            _artifactTypeName = artifactTypeName;
        }

        public override ValidationResult Validate(int value)
        {
            var result = new ValidationResult();

            RelativityObject artifact = _artifactService.GetArtifact(_workspaceArtifactId, _artifactTypeName, value);

            if (artifact == null)
            {
                result.Add($"{_artifactTypeName} {IntegrationPointProviderValidationMessages.ARTIFACT_NOT_EXIST}");
            }

            return result;
        }
    }
}