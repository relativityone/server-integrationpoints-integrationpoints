using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class SourceJobManagerTests : TestBase
	{
		private const string expectError = "Unable to create Relativity Source Job object. Please contact the system administrator.";
		private SourceJobManager _instance;
		private IRepositoryFactory _repositoryFactory;
		private int _sourceWorkspaceArtifactId;
		private int _destinationWorkspaceArtifactId;
		private int _sourceWorkspaceArtifactTypeId;
		private int _sourceWorkspaceRdoInstanceArtifactId;
		private int _jobHistoryArtifactId;
		private ISourceJobRepository _sourceJobRepo;
		private ISourceWorkspaceJobHistoryRepository _sourceWorkspaceJobHistoryRepo;
		private IArtifactGuidRepository _artifactGuidRepo;
		private IExtendedFieldRepository _extendedFieldRepository;
		private IObjectTypeRepository _objectTypeRepository;
		private ITabRepository _tabRepository;
		private List<Guid> _objectFieldGuids;
		private SourceWorkspaceJobHistoryDTO _sourceWorkspaceJobHistory;
		private Dictionary<Guid, int> _objectFieldToBeCreated;

		[SetUp]
		public override void SetUp()
		{
			_sourceWorkspaceArtifactId = 789456;
			_destinationWorkspaceArtifactId = 744521;
			_sourceWorkspaceArtifactTypeId = 789;
			_sourceWorkspaceRdoInstanceArtifactId = 123;
			_jobHistoryArtifactId = 753159;
			_objectFieldGuids = new List<Guid> { SourceJobDTO.Fields.JobHistoryIdFieldGuid, SourceJobDTO.Fields.JobHistoryNameFieldGuid };

			_objectFieldToBeCreated = new Dictionary<Guid, int>
			{
				{SourceJobDTO.Fields.JobHistoryIdFieldGuid, 123},
				{SourceJobDTO.Fields.JobHistoryNameFieldGuid, 456}
			};

			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			_sourceJobRepo = Substitute.For<ISourceJobRepository>();
			_repositoryFactory.GetSourceJobRepository(_destinationWorkspaceArtifactId).Returns(_sourceJobRepo);

			_sourceWorkspaceJobHistoryRepo = Substitute.For<ISourceWorkspaceJobHistoryRepository>();
			_repositoryFactory.GetSourceWorkspaceJobHistoryRepository(_sourceWorkspaceArtifactId).Returns(_sourceWorkspaceJobHistoryRepo);

			_artifactGuidRepo = Substitute.For<IArtifactGuidRepository>();
			_repositoryFactory.GetArtifactGuidRepository(_destinationWorkspaceArtifactId).Returns(_artifactGuidRepo);

			_extendedFieldRepository = Substitute.For<IExtendedFieldRepository>();
			_repositoryFactory.GetExtendedFieldRepository(_destinationWorkspaceArtifactId).Returns(_extendedFieldRepository);

			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_repositoryFactory.GetObjectTypeRepository(_destinationWorkspaceArtifactId).Returns(_objectTypeRepository);

			_tabRepository = Substitute.For<ITabRepository>();
			_repositoryFactory.GetTabRepository(_destinationWorkspaceArtifactId).Returns(_tabRepository);

			_sourceWorkspaceJobHistory = new SourceWorkspaceJobHistoryDTO() { Name = "MassEditMike" };
			_sourceWorkspaceJobHistoryRepo.Retrieve(_jobHistoryArtifactId).Returns(_sourceWorkspaceJobHistory);
			_instance = new SourceJobManager(_repositoryFactory);

			Assert.IsTrue(_instance is DestinationWorkspaceFieldManagerBase, "if the base class changed, the expectation in these tests are probably wrong.");
		}

		[Test]
		public void InitializeWorkspace_PushToTheSameWorkspace()
		{
			// arrange
			int typeId = 852;
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid).Returns(typeId);
			_artifactGuidRepo.GuidsExist(_objectFieldGuids).Returns(new Dictionary<Guid, bool>());
			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(true);

			// act
			_instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId);

			// assert
			_sourceJobRepo.Received(1).Create(typeId, Arg.Any<SourceJobDTO>());
			_artifactGuidRepo.DidNotReceive().InsertArtifactGuidForArtifactId(Arg.Any<int>(), SourceJobDTO.ObjectTypeGuid);
			_sourceJobRepo.DidNotReceive().CreateObjectTypeFields(Arg.Any<int>(), Arg.Any<IEnumerable<Guid>>());
			_artifactGuidRepo.DidNotReceive().InsertArtifactGuidsForArtifactIds(Arg.Any<IDictionary<Guid, int>>());
			_extendedFieldRepository.DidNotReceive().RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject);
			_sourceJobRepo.DidNotReceive().CreateFieldOnDocument(typeId);
		}

		[Test]
		public void InitializeWorkspace_PushToANewWorkspace()
		{
			int artifactId = 741;
			int typeId = 9874;
			int filterType = 666;
			int documentFieldArtifactId = 6666;
			int sourceJobArtifactId = 98765646;

			// throw exception the first time. returns a type id after the object type is created.
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x => typeId);
			// no type found in the workspace
			_objectTypeRepository.RetrieveObjectTypeArtifactId(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_sourceJobRepo.CreateObjectType(_sourceWorkspaceArtifactTypeId).Returns(artifactId);
			// no tab created after creating an object type
			_tabRepository.RetrieveTabArtifactId(typeId, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);

			// object fields are not found when searching by guid
			_artifactGuidRepo.GuidsExist(Arg.Any<List<Guid>>()).Returns(new Dictionary<Guid, bool>
			{
				{ SourceJobDTO.Fields.JobHistoryIdFieldGuid, false},
				{ SourceJobDTO.Fields.JobHistoryNameFieldGuid, false}
			});

			// object fields are not found when searching by name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.WholeNumber).Returns((int?)null);
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.FixedLengthText).Returns((int?)null);

			// defined the behavior when we try to create fields
			_sourceJobRepo.CreateObjectTypeFields(typeId,
				Arg.Is<IEnumerable<Guid>>(x => x.Count() == 2 && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)))
				.Returns(_objectFieldToBeCreated);

			// field in document object does not exist
			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(false);

			// can't find when searching by name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).Returns((int?)null);

			_sourceJobRepo.CreateFieldOnDocument(typeId).Returns(documentFieldArtifactId);
			_extendedFieldRepository.RetrieveArtifactViewFieldId(documentFieldArtifactId).Returns(filterType);

			_sourceJobRepo.Create(typeId, Arg.Any<SourceJobDTO>()).Returns(sourceJobArtifactId);

			// act
			SourceJobDTO sourceJobDto = _instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId);

			//assert
			_sourceJobRepo.Received(1).CreateObjectType(_sourceWorkspaceArtifactTypeId);
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(artifactId, SourceJobDTO.ObjectTypeGuid);

			// expect to not having to remove the tab
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());

			// expect to create 2 fields with the specified guids
			_sourceJobRepo.Received(1).CreateObjectTypeFields(typeId, Arg.Is<List<Guid>>(x => x.Count == 2
			   && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)));

			// expect to associate fields with guids
			_artifactGuidRepo.Received(1).InsertArtifactGuidsForArtifactIds(Arg.Is<Dictionary<Guid, Int32>>(x =>
				x.Count == 2
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x[SourceJobDTO.Fields.JobHistoryIdFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryIdFieldGuid]
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryNameFieldGuid) && x[SourceJobDTO.Fields.JobHistoryNameFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryNameFieldGuid]
			));

			// expect to set filter
			_extendedFieldRepository.Received(1).UpdateFilterType(Arg.Any<int>(), Arg.Any<string>());
			// expect to set overlay behavior
			_extendedFieldRepository.Received(1).SetOverlayBehavior(documentFieldArtifactId, true);
			// expect to associate the field
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(documentFieldArtifactId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			// expect to create an instance of a source job
			_sourceJobRepo.Received(1).Create(typeId, Arg.Any<SourceJobDTO>());

			ValidateSourceJob(sourceJobDto, sourceJobArtifactId);
		}

		[Test]
		public void InitializeWorkspace_CreateObjectFields_RetrieveFieldFails()
		{
			int artifactId = 741;
			int typeId = 9874;
			int filterType = 666;
			int documentFieldArtifactId = 6666;
			int sourceJobArtifactId = 98765646;

			// throw exception the first time. returns a type id after the object type is created.
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x => typeId);
			// no type found in the workspace
			_objectTypeRepository.RetrieveObjectTypeArtifactId(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_sourceJobRepo.CreateObjectType(_sourceWorkspaceArtifactTypeId).Returns(artifactId);
			// no tab created after creating an object type
			_tabRepository.RetrieveTabArtifactId(typeId, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);

			// object fields are not found when searching by guid
			_artifactGuidRepo.GuidsExist(Arg.Any<List<Guid>>()).Returns(new Dictionary<Guid, bool>
			{
				{ SourceJobDTO.Fields.JobHistoryIdFieldGuid, false},
				{ SourceJobDTO.Fields.JobHistoryNameFieldGuid, false}
			});

			// fails to retrieve field
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, typeId,
				(int) Relativity.Client.FieldType.WholeNumber).Throws<Exception>();
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.FixedLengthText).Returns((int?)null);

			// defined the behavior when we try to create fields
			_sourceJobRepo.CreateObjectTypeFields(typeId,
				Arg.Is<IEnumerable<Guid>>(x => x.Count() == 2 && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)))
				.Returns(_objectFieldToBeCreated);

			// field in document object does not exist
			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(false);

			// can't find when searching by name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).Returns((int?)null);

			_sourceJobRepo.CreateFieldOnDocument(typeId).Returns(documentFieldArtifactId);
			_extendedFieldRepository.RetrieveArtifactViewFieldId(documentFieldArtifactId).Returns(filterType);

			_sourceJobRepo.Create(typeId, Arg.Any<SourceJobDTO>()).Returns(sourceJobArtifactId);

			// act
			Assert.Throws<Exception>(() => _instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId), expectError);

			//assert
			_sourceJobRepo.Received(1).CreateObjectType(_sourceWorkspaceArtifactTypeId);
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(artifactId, SourceJobDTO.ObjectTypeGuid);

			// expect to not having to remove the tab
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());
			// expect not to create obj fields with the specified guids
			_sourceJobRepo.DidNotReceive().CreateObjectTypeFields(typeId, Arg.Any<List<Guid>>());
			// expect not to associate obj fields with guids
			_artifactGuidRepo.DidNotReceive().InsertArtifactGuidsForArtifactIds(Arg.Any<Dictionary<Guid, Int32>>());
			// expect not to search for document field
			_artifactGuidRepo.DidNotReceive().GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			// expect not to set filter
			_extendedFieldRepository.DidNotReceive().UpdateFilterType(Arg.Any<int>(), Arg.Any<string>());
			// expect not to set overlay behavior
			_extendedFieldRepository.DidNotReceive().SetOverlayBehavior(documentFieldArtifactId, true);
			// expect not to associate the field
			_artifactGuidRepo.DidNotReceive().InsertArtifactGuidForArtifactId(documentFieldArtifactId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			// expect not to create an instance of a source job
			_sourceJobRepo.DidNotReceive().Create(typeId, Arg.Any<SourceJobDTO>());

		}

		[Test]
		public void InitializeWorkspace_CreateDocumentsFields_PushToAWorkspaceThatAlreadyHasAFieldOnTheDocumentsObject()
		{
			int artifactId = 741;
			int typeId = 9874;
			int filterType = 666;
			int documentFieldArtifactId = 6666;
			int sourceJobArtifactId = 98765646;

			// throw exception the first time. returns a type id after the object type is created.
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x => typeId);
			// no type found in the workspace
			_objectTypeRepository.RetrieveObjectTypeArtifactId(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_sourceJobRepo.CreateObjectType(_sourceWorkspaceArtifactTypeId).Returns(artifactId);
			// no tab created after creating an object type
			_tabRepository.RetrieveTabArtifactId(typeId, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);

			// object fields are not found when searching by guid
			_artifactGuidRepo.GuidsExist(Arg.Any<List<Guid>>()).Returns(new Dictionary<Guid, bool>
			{
				{ SourceJobDTO.Fields.JobHistoryIdFieldGuid, false},
				{ SourceJobDTO.Fields.JobHistoryNameFieldGuid, false}
			});

			// object fields are not found when searching by name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.WholeNumber).Returns((int?)null);
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.FixedLengthText).Returns((int?)null);

			// defined the behavior when we try to create fields
			_sourceJobRepo.CreateObjectTypeFields(typeId,
				Arg.Is<IEnumerable<Guid>>(x => x.Count() == 2 && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)))
				.Returns(_objectFieldToBeCreated);

			// field in document object does not exist
			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(false);

			// found a field with the same name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).Returns(documentFieldArtifactId);
			_extendedFieldRepository.RetrieveArtifactViewFieldId(documentFieldArtifactId).Returns(filterType);

			_sourceJobRepo.Create(typeId, Arg.Any<SourceJobDTO>()).Returns(sourceJobArtifactId);

			// act
			SourceJobDTO sourceJobDto = _instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId);

			//assert
			_sourceJobRepo.Received(1).CreateObjectType(_sourceWorkspaceArtifactTypeId);
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(artifactId, SourceJobDTO.ObjectTypeGuid);

			// expect to not having to remove the tab
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());

			// expect to create 2 fields with the specified guids
			_sourceJobRepo.Received(1).CreateObjectTypeFields(typeId, Arg.Is<List<Guid>>(x => x.Count == 2
			   && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)));

			// expect to associate fields with guids
			_artifactGuidRepo.Received(1).InsertArtifactGuidsForArtifactIds(Arg.Is<Dictionary<Guid, Int32>>(x =>
				x.Count == 2
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x[SourceJobDTO.Fields.JobHistoryIdFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryIdFieldGuid]
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryNameFieldGuid) && x[SourceJobDTO.Fields.JobHistoryNameFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryNameFieldGuid]
			));

			// expect not to create a new field
			_sourceJobRepo.DidNotReceive().CreateFieldOnDocument(Arg.Any<int>());

			// expect to set filter
			_extendedFieldRepository.Received(1).UpdateFilterType(filterType, "Popup");
			// expect to set overlay behavior
			_extendedFieldRepository.Received(1).SetOverlayBehavior(documentFieldArtifactId, true);
			// expect to associate the field
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(documentFieldArtifactId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			// expect to create an instance of a source job
			_sourceJobRepo.Received(1).Create(typeId, Arg.Any<SourceJobDTO>());

			ValidateSourceJob(sourceJobDto, sourceJobArtifactId);
		}

		[Test]
		public void InitializeWorkspace_CreateDocumentsFields_RetrieveArtifactViewFieldIdDoesNotReturnArtifactFieldViewId()
		{
			int artifactId = 741;
			int typeId = 9874;
			int documentFieldArtifactId = 6666;

			// throw exception the first time. returns a type id after the object type is created.
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x => typeId);
			// no type found in the workspace
			_objectTypeRepository.RetrieveObjectTypeArtifactId(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_sourceJobRepo.CreateObjectType(_sourceWorkspaceArtifactTypeId).Returns(artifactId);
			// no tab created after creating an object type
			_tabRepository.RetrieveTabArtifactId(typeId, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);

			// object fields are not found when searching by guid
			_artifactGuidRepo.GuidsExist(Arg.Any<List<Guid>>()).Returns(new Dictionary<Guid, bool>
			{
				{ SourceJobDTO.Fields.JobHistoryIdFieldGuid, false},
				{ SourceJobDTO.Fields.JobHistoryNameFieldGuid, false}
			});

			// error occurs when trying to retrieve field's artifact id
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, typeId,(int) Relativity.Client.FieldType.WholeNumber).Throws(new Exception());
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.FixedLengthText).Returns((int?)null);

			// defined the behavior when we try to create fields
			_sourceJobRepo.CreateObjectTypeFields(typeId,
				Arg.Is<IEnumerable<Guid>>(x => x.Count() == 2 && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)))
				.Returns(_objectFieldToBeCreated);

			// field in document object does not exist
			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(false);

			// found a field with the same name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).Returns(documentFieldArtifactId);

			// can't find artifact view field id
			_extendedFieldRepository.RetrieveArtifactViewFieldId(documentFieldArtifactId).Returns((int?)null);

			// act
			Assert.Throws<Exception>(() => _instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId), expectError);

			//assert
			_sourceJobRepo.Received(1).CreateObjectType(_sourceWorkspaceArtifactTypeId);
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(artifactId, SourceJobDTO.ObjectTypeGuid);

			// expect to not having to remove the tab
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());

			// expect not to create object fields
			_sourceJobRepo.DidNotReceive().CreateObjectTypeFields(typeId, Arg.Any<List<Guid>>());
			// expect not to associate object fields with guids
			_artifactGuidRepo.DidNotReceive().InsertArtifactGuidsForArtifactIds(Arg.Any<Dictionary<Guid, Int32>>());
			// expect not to create a new field
			_sourceJobRepo.DidNotReceive().CreateFieldOnDocument(Arg.Any<int>());
			// expect not to set filter
			_extendedFieldRepository.DidNotReceive().UpdateFilterType(Arg.Any<int>(), Arg.Any<string>());
			// expect not to associate the field
			_artifactGuidRepo.DidNotReceive().InsertArtifactGuidForArtifactId(documentFieldArtifactId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			// expect not to create an instance of a source job
			_sourceJobRepo.DidNotReceive().Create(typeId, Arg.Any<SourceJobDTO>());
		}

		[Test]
		public void InitializeWorkspace_CreateDocumentsFields_SetOverlayBehaviorFails()
		{
			int artifactId = 741;
			int typeId = 9874;
			int filterType = 666;
			int documentFieldArtifactId = 6666;

			// throw exception the first time. returns a type id after the object type is created.
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x => typeId);
			// no type found in the workspace
			_objectTypeRepository.RetrieveObjectTypeArtifactId(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_sourceJobRepo.CreateObjectType(_sourceWorkspaceArtifactTypeId).Returns(artifactId);
			// no tab created after creating an object type
			_tabRepository.RetrieveTabArtifactId(typeId, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);

			// object fields are not found when searching by guid
			_artifactGuidRepo.GuidsExist(Arg.Any<List<Guid>>()).Returns(new Dictionary<Guid, bool>
			{
				{ SourceJobDTO.Fields.JobHistoryIdFieldGuid, false},
				{ SourceJobDTO.Fields.JobHistoryNameFieldGuid, false}
			});

			// object fields are not found when searching by name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.WholeNumber).Returns((int?)null);
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.FixedLengthText).Returns((int?)null);

			// defined the behavior when we try to create fields
			_sourceJobRepo.CreateObjectTypeFields(typeId,
				Arg.Is<IEnumerable<Guid>>(x => x.Count() == 2 && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)))
				.Returns(_objectFieldToBeCreated);

			// field in document object does not exist
			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(false);

			// found a field with the same name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).Returns(documentFieldArtifactId);

			// can't find artifact view field id
			_extendedFieldRepository.RetrieveArtifactViewFieldId(documentFieldArtifactId).Returns(filterType);

			_extendedFieldRepository.When(x => x.SetOverlayBehavior(documentFieldArtifactId, true)).Throw<Exception>();

			// act
			Assert.Throws<Exception>(() => _instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId), expectError);

			//assert
			_sourceJobRepo.Received(1).CreateObjectType(_sourceWorkspaceArtifactTypeId);
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(artifactId, SourceJobDTO.ObjectTypeGuid);

			// expect to not having to remove the tab
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());

			// expect to create 2 fields with the specified guids
			_sourceJobRepo.Received(1).CreateObjectTypeFields(typeId, Arg.Is<List<Guid>>(x => x.Count == 2
			   && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)));

			// expect to associate fields with guids
			_artifactGuidRepo.Received(1).InsertArtifactGuidsForArtifactIds(Arg.Is<Dictionary<Guid, Int32>>(x =>
				x.Count == 2
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x[SourceJobDTO.Fields.JobHistoryIdFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryIdFieldGuid]
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryNameFieldGuid) && x[SourceJobDTO.Fields.JobHistoryNameFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryNameFieldGuid]
			));

			// expect not to create a new field
			_sourceJobRepo.DidNotReceive().CreateFieldOnDocument(Arg.Any<int>());
			// expect to set filter
			_extendedFieldRepository.Received(1).UpdateFilterType(filterType, "Popup");
			// expect not to associate the field
			_artifactGuidRepo.DidNotReceive().InsertArtifactGuidForArtifactId(documentFieldArtifactId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			// expect not to create an instance of a source job
			_sourceJobRepo.DidNotReceive().Create(typeId, Arg.Any<SourceJobDTO>());
		}

		[Test]
		public void InitializeWorkspace_CreateDocumentsFields_InsertArtifactGuidForArtifactIdOnDocumentsObjectFieldFails()
		{
			int artifactId = 741;
			int typeId = 9874;
			int filterType = 666;
			int documentFieldArtifactId = 6666;

			// throw exception the first time. returns a type id after the object type is created.
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x => typeId);
			// no type found in the workspace
			_objectTypeRepository.RetrieveObjectTypeArtifactId(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_sourceJobRepo.CreateObjectType(_sourceWorkspaceArtifactTypeId).Returns(artifactId);
			// no tab created after creating an object type
			_tabRepository.RetrieveTabArtifactId(typeId, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);

			// object fields are not found when searching by guid
			_artifactGuidRepo.GuidsExist(Arg.Any<List<Guid>>()).Returns(new Dictionary<Guid, bool>
			{
				{ SourceJobDTO.Fields.JobHistoryIdFieldGuid, false},
				{ SourceJobDTO.Fields.JobHistoryNameFieldGuid, false}
			});

			// object fields are not found when searching by name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.WholeNumber).Returns((int?)null);
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, typeId, (int)Relativity.Client.FieldType.FixedLengthText).Returns((int?)null);

			// defined the behavior when we try to create fields
			_sourceJobRepo.CreateObjectTypeFields(typeId,
				Arg.Is<IEnumerable<Guid>>(x => x.Count() == 2 && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)))
				.Returns(_objectFieldToBeCreated);

			// field in document object does not exist
			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(false);

			// found a field with the same name
			_extendedFieldRepository.RetrieveField(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).Returns(documentFieldArtifactId);

			
			_extendedFieldRepository.RetrieveArtifactViewFieldId(documentFieldArtifactId).Returns(filterType);

			_artifactGuidRepo.When( x => x.InsertArtifactGuidForArtifactId(documentFieldArtifactId,SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid)).Throw<Exception>();

			// act
			Assert.Throws<Exception>(() => _instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId), expectError);

			//assert
			_sourceJobRepo.Received(1).CreateObjectType(_sourceWorkspaceArtifactTypeId);
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(artifactId, SourceJobDTO.ObjectTypeGuid);

			// expect to not having to remove the tab
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());

			// expect to create 2 fields with the specified guids
			_sourceJobRepo.Received(1).CreateObjectTypeFields(typeId, Arg.Is<List<Guid>>(x => x.Count == 2
			   && x.Contains(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x.Contains(SourceJobDTO.Fields.JobHistoryNameFieldGuid)));

			// expect to associate fields with guids
			_artifactGuidRepo.Received(1).InsertArtifactGuidsForArtifactIds(Arg.Is<Dictionary<Guid, Int32>>(x =>
				x.Count == 2
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryIdFieldGuid) && x[SourceJobDTO.Fields.JobHistoryIdFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryIdFieldGuid]
				&& x.ContainsKey(SourceJobDTO.Fields.JobHistoryNameFieldGuid) && x[SourceJobDTO.Fields.JobHistoryNameFieldGuid] == _objectFieldToBeCreated[SourceJobDTO.Fields.JobHistoryNameFieldGuid]
			));

			// expect to create a new field
			_sourceJobRepo.DidNotReceive().CreateFieldOnDocument(Arg.Any<int>());
			// expect to set filter
			_extendedFieldRepository.Received(1).UpdateFilterType(filterType, "Popup");
			// expect to associate the field
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(documentFieldArtifactId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			// expect not to create an instance of a source job
			_sourceJobRepo.DidNotReceive().Create(typeId, Arg.Any<SourceJobDTO>());
		}

		private void ValidateSourceJob(SourceJobDTO sourceJob, int sourceJobArtifactId)
		{
			Assert.IsNotNull(sourceJob);
			string expectedName = Utils.GetFormatForWorkspaceOrJobDisplay(_sourceWorkspaceJobHistory.Name, _jobHistoryArtifactId);
			Assert.AreEqual(expectedName, sourceJob.Name);
			Assert.AreEqual(_sourceWorkspaceRdoInstanceArtifactId, sourceJob.SourceWorkspaceArtifactId);
			Assert.AreEqual(_jobHistoryArtifactId, sourceJob.JobHistoryArtifactId);
			Assert.AreEqual(_sourceWorkspaceJobHistory.Name, sourceJob.JobHistoryName);
			Assert.AreEqual(sourceJobArtifactId, sourceJob.ArtifactId);
		}
	}
}