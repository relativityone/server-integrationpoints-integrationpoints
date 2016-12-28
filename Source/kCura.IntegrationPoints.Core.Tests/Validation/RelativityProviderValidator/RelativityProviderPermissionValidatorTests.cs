using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class RelativityProviderPermissionValidatorTests : PermissionValidatorTestsBase
	{
		[Test, Combinatorial]
		public void ValidateTest(
			[Values(true, false)] bool exportPermission, 
			[Values(true, false)] bool destinationWorkspacePermission, 
			[Values(true, false)] bool destinationImportPermission, 
			[Values(true, false)] bool destinationRdoPermissions,
			[Values(true, false)] bool sourceDocumentEditPermissions)
		{
			// arrange
			_sourcePermissionRepository.UserCanExport().Returns(exportPermission);
			_destinationPermissionRepository.UserHasPermissionToAccessWorkspace().Returns(destinationWorkspacePermission);
			_destinationPermissionRepository.UserCanImport().Returns(destinationImportPermission);
			_destinationPermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(
					x => x.SequenceEqual(new[] {ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create})))
				.Returns(destinationRdoPermissions);
			_sourcePermissionRepository.UserCanEditDocuments().Returns(sourceDocumentEditPermissions);

			var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper);

			// act
			var validationResult = relativityProviderPermissionValidator.Validate(_validationModel);

			// assert
			bool expected =
				exportPermission &&
				destinationWorkspacePermission &&
				destinationImportPermission &&
				destinationRdoPermissions &&
				sourceDocumentEditPermissions;

			validationResult.Check(expected);

			_sourcePermissionRepository.UserCanExport();
			_destinationPermissionRepository.UserHasPermissionToAccessWorkspace();
			_destinationPermissionRepository.UserCanImport();
			_destinationPermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(
					x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })));
			_sourcePermissionRepository.UserCanEditDocuments();
		}
	}
}
