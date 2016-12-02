using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
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

			var artifact = _artifactService.GetArtifacts(_workspaceArtifactId, _artifactTypeName)
				.FirstOrDefault(x => x.ArtifactID.Equals(value));

			if (artifact == null)
			{
				result.Add($"{_artifactTypeName} {FileDestinationProviderValidationMessages.ARTIFACT_NOT_EXIST}");
			}

			return result;
		}
	}
}