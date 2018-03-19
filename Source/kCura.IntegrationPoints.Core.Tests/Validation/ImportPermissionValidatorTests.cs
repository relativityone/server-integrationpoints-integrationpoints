﻿using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	public class ImportPermissionValidatorTests : PermissionValidatorTestsBase
	{
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void ValidateTest(bool sourceImportPermission, bool destinationRdoPermissions)
		{
			// arrange
			_sourcePermissionRepository.UserCanImport().Returns(sourceImportPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermissions(
						Arg.Is(_ARTIFACT_TYPE_ID),
						Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })))
						.Returns(destinationRdoPermissions);

			var importPermissionValidator = new ImportPermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper);

			// act
			var validationResult = importPermissionValidator.Validate(_validationModel);

			// assert
			validationResult.Check(sourceImportPermission && destinationRdoPermissions);

			_sourcePermissionRepository.Received(1).UserCanImport();
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermissions(
						Arg.Is(_ARTIFACT_TYPE_ID),
						Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })));
		}

	}
}
