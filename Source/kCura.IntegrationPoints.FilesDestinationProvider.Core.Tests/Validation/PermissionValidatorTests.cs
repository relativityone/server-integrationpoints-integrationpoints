using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using System;
using Relativity.API;
using Relativity.DataExchange.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation
{
	[TestFixture, Category("Unit")]
	public class PermissionValidatorTests : PermissionValidatorTestsBase
	{
		[Test]
		public void NoValidationTest()
		{
			// arrange
			var exportSettings = new ExportUsingSavedSearchSettings()
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
				ExportType = "SavedSearch"
			};

			_serializer.Deserialize<ExportUsingSavedSearchSettings>(_validationModel.SourceConfiguration).Returns(exportSettings);
			IAPILog logger = Substitute.For<IAPILog>();
			var permissionValidator = new PermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper, logger);

			// act
			var validationResult = permissionValidator.Validate(_validationModel);

			// assert
			validationResult.Check(true);

			_sourcePermissionRepository.DidNotReceive().UserHasArtifactInstancePermission(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<ArtifactPermission>());
		}

		[TestCase("Folder", true)]
		[TestCase("Folder", false)]
		[TestCase("FolderAndSubfolders", true)]
		[TestCase("FolderAndSubfolders", false)]
		public void ValidateFolderTest(string folderType, bool expected)
		{
			// arrange
			var exportSettings = new ExportUsingSavedSearchSettings()
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
				ExportType = folderType,
				FolderArtifactId = 1000,
			};

			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				(int)ArtifactType.Folder, exportSettings.FolderArtifactId, ArtifactPermission.View).Returns(expected);

			_serializer.Deserialize<ExportUsingSavedSearchSettings>(_validationModel.SourceConfiguration).Returns(exportSettings);

			IAPILog logger = Substitute.For<IAPILog>();
			var permissionValidator = new PermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper, logger);

			// act
			var validationResult = permissionValidator.Validate(_validationModel);

			// assert
			validationResult.Check(expected);

			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission((int)ArtifactType.Folder, exportSettings.FolderArtifactId, ArtifactPermission.View);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValidateProductionTest(bool expected)
		{
			// arrange
			var exportSettings = new ExportUsingSavedSearchSettings()
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
				ExportType = "ProductionSet",
				ProductionId = 2000
			};

			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				(int)ArtifactType.Production, exportSettings.ProductionId, ArtifactPermission.View).Returns(expected);

			_serializer.Deserialize<ExportUsingSavedSearchSettings>(_validationModel.SourceConfiguration).Returns(exportSettings);

			IAPILog logger = Substitute.For<IAPILog>();
			var permissionValidator = new PermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper, logger);

			// act
			var validationResult = permissionValidator.Validate(_validationModel);

			// assert
			validationResult.Check(expected);

			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission((int)ArtifactType.Production, exportSettings.ProductionId, ArtifactPermission.View);
		}

	}
}
