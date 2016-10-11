using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class SourceWorkspaceManagerTests
	{
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 8675309;
		private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 2025862;
		private const int _OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID = 311411;
		private const int _OBJECT_TYPE_ARTIFACT_ID = 611911;
		private const int _OBJECT_TYPE_TAB_ID = 963852;
		private const int _DOCUMENT_SOURCE_WORKSPACE_MO_FIELD_ARTIFACT_ID = 22334455;
		private const int _DOCUMENT_SOURCE_WORKSPACE_ARTIFACT_VIEW_FIELD_ID = 44668811;

		private const string _SOURCE_WORKSPACE_NAME = "Killer Queen Review";

		private IRepositoryFactory _repositoryFactory;
		private ISourceWorkspaceRepository _sourceWorkspaceRepository;
		private IArtifactGuidRepository _artifactGuidRepository;
		private IWorkspaceRepository _workspaceRepository;
		private IObjectTypeRepository _objectTypeRepository;
		private ITabRepository _tabRepository;
		private IFieldRepository _fieldRepository;

		private SourceWorkspaceManager _instance;

		[SetUp]
		public void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			_sourceWorkspaceRepository = Substitute.For<ISourceWorkspaceRepository>();
			_repositoryFactory.GetSourceWorkspaceRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID).Returns(_sourceWorkspaceRepository);

			_artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
			_repositoryFactory.GetArtifactGuidRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID).Returns(_artifactGuidRepository);

			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			_repositoryFactory.GetWorkspaceRepository().Returns(_workspaceRepository);

			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_repositoryFactory.GetObjectTypeRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID).Returns(_objectTypeRepository);

			_tabRepository = Substitute.For<ITabRepository>();
			_repositoryFactory.GetTabRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID).Returns(_tabRepository);

			_fieldRepository = Substitute.For<IFieldRepository>();
			_repositoryFactory.GetFieldRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID).Returns(_fieldRepository);

			_instance = new SourceWorkspaceManager(_repositoryFactory);
		}

		[TearDown]
		public void TearDown()
		{
			// Verify repository factory expectations
			// Tab repository excluded
			_repositoryFactory.Received(1).GetSourceWorkspaceRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID);
			_repositoryFactory.Received(1).GetArtifactGuidRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID);
			_repositoryFactory.Received(1).GetWorkspaceRepository();
			_repositoryFactory.Received(1).GetObjectTypeRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID);
			_repositoryFactory.Received(1).GetFieldRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void InitializeWorkspace_PushToSameWorkspace_Test()
		{
			// Arrange
			// Create Object Type
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid)
				.Returns(_OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID);

			// Create Object Fields
			IDictionary<Guid, bool> fieldGuids = new Dictionary<Guid, bool>(2)
			{
				{SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, true},
				{SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid, true}
			};
			_artifactGuidRepository.GuidsExist(Arg.Is<List<Guid>>(x => VerifyListOfGuids(fieldGuids.Keys, x)))
				.Returns(fieldGuids);

			// Create Document Fields
			_artifactGuidRepository.GuidExists(SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid).Returns(true);

			// Create Source Workspace DTO
			WorkspaceDTO workspaceDto = new WorkspaceDTO
			{
				Name = _SOURCE_WORKSPACE_NAME
			};
			_workspaceRepository.Retrieve(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(workspaceDto);
			SourceWorkspaceDTO expectedSourceWorkspaceDto = new SourceWorkspaceDTO
			{
				SourceCaseName = _SOURCE_WORKSPACE_NAME
			};
			_sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(expectedSourceWorkspaceDto);

			// Act
			SourceWorkspaceDTO actualSourceWorkspaceDto = _instance.InitializeWorkspace(_SOURCE_WORKSPACE_ARTIFACT_ID, _DESTINATION_WORKSPACE_ARTIFACT_ID);

			// Assert
			Assert.AreEqual(expectedSourceWorkspaceDto.ArtifactTypeId, _OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID);
			Assert.AreEqual(expectedSourceWorkspaceDto.Name, actualSourceWorkspaceDto.Name);

			_objectTypeRepository.Received(1)
				.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid);

			// Create Object Type
			_repositoryFactory.DidNotReceiveWithAnyArgs().GetTabRepository(Arg.Any<int>());

			_objectTypeRepository.DidNotReceiveWithAnyArgs().RetrieveObjectTypeArtifactId(Arg.Any<string>());
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().CreateObjectType(Arg.Any<int>());
			_artifactGuidRepository.DidNotReceiveWithAnyArgs().InsertArtifactGuidForArtifactId(Arg.Any<int>(), Arg.Any<Guid>());
			_tabRepository.DidNotReceiveWithAnyArgs().RetrieveTabArtifactId(Arg.Any<int>(), Arg.Any<string>());
			_tabRepository.DidNotReceiveWithAnyArgs().Delete(Arg.Any<int>());

			// Create Object Fields
			_fieldRepository.DidNotReceiveWithAnyArgs().RetrieveField(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().CreateObjectTypeFields(Arg.Any<int>(), Arg.Any<IEnumerable<Guid>>());

			// Create Document Fields
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().CreateFieldOnDocument(Arg.Any<int>());
			_fieldRepository.DidNotReceiveWithAnyArgs().RetrieveArtifactViewFieldId(Arg.Any<int>());
			_fieldRepository.DidNotReceiveWithAnyArgs().UpdateFilterType(Arg.Any<int>(), Arg.Any<string>());
			_fieldRepository.DidNotReceiveWithAnyArgs().SetOverlayBehavior(Arg.Any<int>(), Arg.Any<bool>());

			// Create Source Workspace DTO
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().Create(Arg.Any<int>(), Arg.Any<SourceWorkspaceDTO>());
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().Update(Arg.Any<SourceWorkspaceDTO>());
		}

		[Test]
		public void InitializeWorkspace_PushToNewWorkspace_FieldsExistWithoutGuids_FieldsUpdatedWithGuids_Test()
		{
			// Arrange
			// Create Object Type
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x => _OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID);
			_objectTypeRepository.RetrieveObjectTypeArtifactId(
				IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME).Returns(_OBJECT_TYPE_ARTIFACT_ID);

			_tabRepository.RetrieveTabArtifactId(_OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID,
				IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME).Returns(_OBJECT_TYPE_TAB_ID);

			// Create Object Fields
			IDictionary<Guid, bool> fieldGuids = new Dictionary<Guid, bool>(2)
			{
				{SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, false},
				{SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid, false}
			};
			_artifactGuidRepository.GuidsExist(Arg.Is<List<Guid>>(x => VerifyListOfGuids(fieldGuids.Keys, x)))
				.Returns(fieldGuids);

			_fieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
				_OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID, (int) Relativity.Client.FieldType.WholeNumber).Returns(1);
			_fieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
				_OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID, (int) Relativity.Client.FieldType.FixedLengthText).Returns(2);

			// Create Document Fields
			_artifactGuidRepository.GuidExists(SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid).Returns(false);

			_fieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
				(int) Relativity.Client.ArtifactType.Document, (int) Relativity.Client.FieldType.MultipleObject)
				.Returns(_DOCUMENT_SOURCE_WORKSPACE_MO_FIELD_ARTIFACT_ID);

			_fieldRepository.RetrieveArtifactViewFieldId(_DOCUMENT_SOURCE_WORKSPACE_MO_FIELD_ARTIFACT_ID)
				.Returns(_DOCUMENT_SOURCE_WORKSPACE_ARTIFACT_VIEW_FIELD_ID);

			// Create Source Workspace DTO
			WorkspaceDTO workspaceDto = new WorkspaceDTO
			{
				Name = _SOURCE_WORKSPACE_NAME
			};
			_workspaceRepository.Retrieve(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(workspaceDto);
			SourceWorkspaceDTO expectedSourceWorkspaceDto = new SourceWorkspaceDTO
			{
				SourceCaseName = _SOURCE_WORKSPACE_NAME
			};
			_sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(expectedSourceWorkspaceDto);

			// Act
			SourceWorkspaceDTO actualSourceWorkspaceDto = _instance.InitializeWorkspace(_SOURCE_WORKSPACE_ARTIFACT_ID, _DESTINATION_WORKSPACE_ARTIFACT_ID);

			// Assert
			Assert.AreEqual(expectedSourceWorkspaceDto.ArtifactTypeId, _OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID);
			Assert.AreEqual(expectedSourceWorkspaceDto.Name, actualSourceWorkspaceDto.Name);

			// Create Object Type
			_objectTypeRepository.Received(2)
				.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid);
			_objectTypeRepository.Received(1).RetrieveObjectTypeArtifactId(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME);
			_artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(_OBJECT_TYPE_ARTIFACT_ID, SourceWorkspaceDTO.ObjectTypeGuid);

			_repositoryFactory.Received(1).GetTabRepository(_DESTINATION_WORKSPACE_ARTIFACT_ID);
			_tabRepository.Received(1).RetrieveTabArtifactId(_OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME);
			_tabRepository.Received(1).Delete(_OBJECT_TYPE_TAB_ID);

			// Create Object Fields
			_fieldRepository.Received(1).RetrieveField(IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
				_OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID, (int)Relativity.Client.FieldType.WholeNumber);
			_fieldRepository.Received(1).RetrieveField(IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
				_OBJECT_TYPE_DESCRIPTOR_ARTIFACT_TYPE_ID, (int)Relativity.Client.FieldType.FixedLengthText);
			_artifactGuidRepository.Received(1).InsertArtifactGuidsForArtifactIds(Arg.Any<Dictionary<Guid, int>>());
			
			// Create Document Fields
			_artifactGuidRepository.Received(1).GuidExists(SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid);
			_fieldRepository.Received(1)
				.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
					(int) Relativity.Client.ArtifactType.Document, (int) Relativity.Client.FieldType.MultipleObject);
			_fieldRepository.Received(1).RetrieveArtifactViewFieldId(_DOCUMENT_SOURCE_WORKSPACE_MO_FIELD_ARTIFACT_ID);
			_fieldRepository.Received(1).UpdateFilterType(_DOCUMENT_SOURCE_WORKSPACE_ARTIFACT_VIEW_FIELD_ID, "Popup");
			_fieldRepository.Received(1).SetOverlayBehavior(_DOCUMENT_SOURCE_WORKSPACE_MO_FIELD_ARTIFACT_ID, true);
			_artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(_DOCUMENT_SOURCE_WORKSPACE_MO_FIELD_ARTIFACT_ID, SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid);

			// Create Object Type
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().CreateObjectType(Arg.Any<int>());

			// Create Object Fields
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().CreateObjectTypeFields(Arg.Any<int>(), Arg.Any<IEnumerable<Guid>>());

			// Create Document Fields
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().CreateFieldOnDocument(Arg.Any<int>());

			// Create Source Workspace DTO
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().Create(Arg.Any<int>(), Arg.Any<SourceWorkspaceDTO>());
			_sourceWorkspaceRepository.DidNotReceiveWithAnyArgs().Update(Arg.Any<SourceWorkspaceDTO>());
		}

		private bool VerifyListOfGuids(ICollection<Guid> expectedGuids, ICollection<Guid> actualGuids)
		{
			if (expectedGuids == null && actualGuids == null)
			{
				return true;
			}
			if (expectedGuids == null || actualGuids == null)
			{
				return false;
			}
			if (expectedGuids.Count != actualGuids.Count)
			{
				return false;
			}
			if (expectedGuids.Any(expectedGuid => !actualGuids.Contains(expectedGuid)))
			{
				return false;
			}
			return true;
		}
	}
}
