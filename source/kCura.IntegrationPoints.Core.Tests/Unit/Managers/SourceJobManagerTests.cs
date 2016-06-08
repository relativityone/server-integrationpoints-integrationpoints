using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class SourceJobManagerTests
	{
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
		private IFieldRepository _fieldRepo;
		private IObjectTypeRepository _objectTypeRepository;
		private ITabRepository _tabRepository;
		private List<Guid> _objectFieldGuids;

		[SetUp]
		public void Setup()
		{
			_sourceWorkspaceArtifactId = 789456;
			_destinationWorkspaceArtifactId = 744521;
			_sourceWorkspaceArtifactTypeId = 789;
			_sourceWorkspaceRdoInstanceArtifactId = 123;
			_jobHistoryArtifactId = 753159;
			_objectFieldGuids = new List<Guid> { SourceJobDTO.Fields.JobHistoryIdFieldGuid, SourceJobDTO.Fields.JobHistoryNameFieldGuid };

			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			_sourceJobRepo = Substitute.For<ISourceJobRepository>();
			_repositoryFactory.GetSourceJobRepository(_destinationWorkspaceArtifactId).Returns(_sourceJobRepo);

			_sourceWorkspaceJobHistoryRepo = Substitute.For<ISourceWorkspaceJobHistoryRepository>();
			_repositoryFactory.GetSourceWorkspaceJobHistoryRepository(_sourceWorkspaceArtifactId).Returns(_sourceWorkspaceJobHistoryRepo);

			_artifactGuidRepo = Substitute.For<IArtifactGuidRepository>();
			_repositoryFactory.GetArtifactGuidRepository(_destinationWorkspaceArtifactId).Returns(_artifactGuidRepo);

			_fieldRepo = Substitute.For<IFieldRepository>();
			_repositoryFactory.GetFieldRepository(_destinationWorkspaceArtifactId).Returns(_fieldRepo);

			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_repositoryFactory.GetObjectTypeRepository(_destinationWorkspaceArtifactId).Returns(_objectTypeRepository);

			_tabRepository = Substitute.For<ITabRepository>();
			_repositoryFactory.GetTabRepository(_destinationWorkspaceArtifactId).Returns(_tabRepository);

			SourceWorkspaceJobHistoryDTO sourceWorkspaceJobHistory = new SourceWorkspaceJobHistoryDTO() {Name = "MassEditMike"};
			_sourceWorkspaceJobHistoryRepo.Retrieve(_jobHistoryArtifactId).Returns(sourceWorkspaceJobHistory);
			_instance = new SourceJobManager(_repositoryFactory);
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
			_fieldRepo.DidNotReceive().RetrieveField(IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject);
			_sourceJobRepo.DidNotReceive().CreateFieldOnDocument(typeId);
		}

		[Test]
		public void InitializeWorkspace_PushToANewWorkspace_FieldDoesNotExist()
		{
			int artifactId = 741;
			int typeId = 9874;
			var objectFieldToBeCreated = new Dictionary<Guid, int>
			{
				{SourceJobDTO.Fields.JobHistoryIdFieldGuid, 123},
				{SourceJobDTO.Fields.JobHistoryNameFieldGuid, 456}
			};

			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid)
				.Returns(x => { throw new TypeLoadException(); }, x =>  typeId );

			_objectTypeRepository.RetrieveObjectTypeArtifactId(IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_sourceJobRepo.CreateObjectType(_sourceWorkspaceArtifactTypeId).Returns(artifactId);
			_tabRepository.RetrieveTabArtifactId(typeId, IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEJOB_FIELD_NAME).Returns((int?)null);
			_artifactGuidRepo.GuidsExist(Arg.Any<List<Guid>>()).Returns(new Dictionary<Guid, bool>
			{
				{ SourceJobDTO.Fields.JobHistoryIdFieldGuid, false},
				{ SourceJobDTO.Fields.JobHistoryNameFieldGuid, false}
			});
			_fieldRepo.RetrieveField(Arg.Any<String>(), typeId, Arg.Any<int>()).Returns((int?)null);
			_sourceJobRepo.CreateObjectTypeFields(typeId, new [] { SourceJobDTO.Fields.JobHistoryIdFieldGuid, SourceJobDTO.Fields.JobHistoryNameFieldGuid }).Returns(objectFieldToBeCreated);

			_artifactGuidRepo.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid).Returns(true);
			
			// act
			_instance.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceArtifactTypeId, _sourceWorkspaceRdoInstanceArtifactId, _jobHistoryArtifactId);

			//assert
			_sourceJobRepo.Received(1).CreateObjectType(_sourceWorkspaceArtifactTypeId);
			_artifactGuidRepo.Received(1).InsertArtifactGuidForArtifactId(artifactId, SourceJobDTO.ObjectTypeGuid);
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());
			_sourceJobRepo.Received(1).CreateObjectTypeFields(typeId, Arg.Any<List<Guid>>());
			_artifactGuidRepo.Received(1).InsertArtifactGuidsForArtifactIds(Arg.Any<Dictionary<Guid, int>>());
		}
	}
}