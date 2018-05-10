using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class RelativityProviderPermissionValidatorTests : PermissionValidatorTestsBase
	{
		private IHelper _helper;
		private IHelperFactory _helperFactory;
		private IContextContainerFactory _contextContainerFactory;
		private IManagerFactory _managerFactory;
		private IPermissionManager _sourceWorkspacePermissionManager;
		private IPermissionManager _targetWorkspacePermissionManager;
		private IWorkspaceManager _destinationWorkspaceManager;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_helper = Substitute.For<IHelper>();
			_helperFactory = Substitute.For<IHelperFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_sourceWorkspacePermissionManager = Substitute.For<IPermissionManager>();
			_targetWorkspacePermissionManager = Substitute.For<IPermissionManager>();
			_destinationWorkspaceManager = Substitute.For<IWorkspaceManager>();

			var sourceContextContainer = Substitute.For<IContextContainer>();
			var targetContextContainer = Substitute.For<IContextContainer>();
			var targetHelper = Substitute.For<IHelper>();
			_helperFactory.CreateTargetHelper(_helper, Arg.Any<int?>(), Arg.Any<string>()).Returns(targetHelper);

			_contextContainerFactory.CreateContextContainer(_helper).Returns(sourceContextContainer);
			_contextContainerFactory.CreateContextContainer(targetHelper).Returns(targetContextContainer);

			_managerFactory.CreatePermissionManager(sourceContextContainer).Returns(_sourceWorkspacePermissionManager);
			_managerFactory.CreatePermissionManager(targetContextContainer).Returns(_targetWorkspacePermissionManager);
			_managerFactory.CreateWorkspaceManager(Arg.Any<IContextContainer>()).Returns(_destinationWorkspaceManager);
		}

		[Test, Combinatorial]
		public void ItShouldValidatePermissions_WhenDestinationWorkspaceExists(
			[Values(true, false)] bool exportPermission,
			[Values(true, false)] bool destinationWorkspacePermission,
			[Values(true, false)] bool destinationImportPermission,
			[Values(true, false)] bool destinationRdoPermissions,
			[Values(true, false)] bool sourceDocumentEditPermissions,
			[Values(null, 1000)] int federatedInstanceId)
		{
			// arrange
			_destinationWorkspaceManager.WorkspaceExists(_DESTINATION_WORKSPACE_ID).Returns(true);
			_sourceWorkspacePermissionManager.UserCanExport(_SOURCE_WORKSPACE_ID).Returns(exportPermission);
			_targetWorkspacePermissionManager.UserHasPermissionToAccessWorkspace(_DESTINATION_WORKSPACE_ID).Returns(destinationWorkspacePermission);
			_targetWorkspacePermissionManager.UserCanImport(_DESTINATION_WORKSPACE_ID).Returns(destinationImportPermission);
			_targetWorkspacePermissionManager.UserHasArtifactTypePermissions(
				_DESTINATION_WORKSPACE_ID,
				_ARTIFACT_TYPE_ID,
				Arg.Is<ArtifactPermission[]>(
					x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })))
				.Returns(destinationRdoPermissions);
			_sourceWorkspacePermissionManager.UserCanEditDocuments(_SOURCE_WORKSPACE_ID).Returns(sourceDocumentEditPermissions);

			_serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
				.Returns(new SourceConfiguration()
				{
					SavedSearchArtifactId = _SAVED_SEARCH_ID,
					SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
					TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
					FederatedInstanceArtifactId = federatedInstanceId
				});

			var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper,
				_helper, _helperFactory, _contextContainerFactory, _managerFactory);

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

			_sourceWorkspacePermissionManager.Received(1).UserCanExport(_SOURCE_WORKSPACE_ID);
			_targetWorkspacePermissionManager.Received(1).UserHasPermissionToAccessWorkspace(_DESTINATION_WORKSPACE_ID);
			if (destinationWorkspacePermission)
			{
				_targetWorkspacePermissionManager.Received(1).UserCanImport(_DESTINATION_WORKSPACE_ID);
				_targetWorkspacePermissionManager.Received(1).UserHasArtifactTypePermissions(
					_DESTINATION_WORKSPACE_ID,
					_ARTIFACT_TYPE_ID,
					Arg.Is<ArtifactPermission[]>(
						x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })));
			}
			_sourceWorkspacePermissionManager.Received(1).UserCanEditDocuments(_SOURCE_WORKSPACE_ID);
		}

		[Test]
		public void ItShouldNotValidateDestinationWorkspacePermissionWhenWorkspaceDoesNotExist()
		{
			bool destinationWorkspaceExists = false;
			bool exportPermission = true;
			bool sourceDocumentEditPermissions = true;
			int federatedInstanceId = 1000;
			// arrange
			_destinationWorkspaceManager.WorkspaceExists(_DESTINATION_WORKSPACE_ID).Returns(destinationWorkspaceExists);

			_sourceWorkspacePermissionManager.UserCanExport(_SOURCE_WORKSPACE_ID).Returns(exportPermission);
			_sourceWorkspacePermissionManager.UserCanEditDocuments(_SOURCE_WORKSPACE_ID).Returns(sourceDocumentEditPermissions);

			_serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
				.Returns(new SourceConfiguration()
				{
					SavedSearchArtifactId = _SAVED_SEARCH_ID,
					SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
					TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
					FederatedInstanceArtifactId = federatedInstanceId
				});

			var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper,
				_helper, _helperFactory, _contextContainerFactory, _managerFactory);

			// act
			var validationResult = relativityProviderPermissionValidator.Validate(_validationModel);

			// assert
			bool expected = destinationWorkspaceExists;

			validationResult.Check(expected);

			_sourceWorkspacePermissionManager.Received(1).UserCanExport(_SOURCE_WORKSPACE_ID);
			_sourceWorkspacePermissionManager.Received(1).UserCanEditDocuments(_SOURCE_WORKSPACE_ID);
		}
	}
}
