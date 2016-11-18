using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.BatchStatusCommands
{
	[TestFixture]
	public class TargetDocumentsTaggingManagerTests : TestBase
	{
		private IRepositoryFactory _repositoryFactory;
		private IScratchTableRepository _scratchTableRepository;
		private IDataSynchronizer _synchronizer;
		private ISourceWorkspaceManager _sourceWorkspaceManager;
		private ISourceJobManager _sourceJobManager;
		private IDocumentRepository _documentRepo;
		private IHelper _helper;
		private string _importConfig;
		private int _sourceWorkspaceArtifactId;
		private int _destinationWorkspaceArtifactId;
		private int _jobHistoryArtifactId;

		private FieldMap[] _fieldMaps;
		private TargetDocumentsTaggingManager _instance;
		private Job _job = null;

		private const string _scratchTableName = "IntegrationPoint_Relativity_SourceWorkspace";

		private readonly string _uniqueJobId = "1_JobIdGuid";

		readonly SourceWorkspaceDTO _sourceWorkspaceDto = new SourceWorkspaceDTO()
		{
			Name = "source workspace",
			ArtifactTypeId = 410,
			ArtifactId = 987
		};

		[OneTimeSetUp]
		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_scratchTableRepository = Substitute.For<IScratchTableRepository>();
			_synchronizer = Substitute.For<IDataSynchronizer>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
			_sourceJobManager = Substitute.For<ISourceJobManager>();
			_documentRepo = Substitute.For<IDocumentRepository>();
			_helper = Substitute.For<IHelper>();

			_importConfig = String.Empty;
			_sourceWorkspaceArtifactId = 100;
			_destinationWorkspaceArtifactId = 200;
			_jobHistoryArtifactId = 300;
			_fieldMaps = new FieldMap[]
			{
				new FieldMap()
				{
					DestinationField = new FieldEntry(),
					SourceField = new FieldEntry()
				},
				new FieldMap()
				{
					DestinationField = new FieldEntry()
					{
						DisplayName = "destination id",
						FieldIdentifier = "123456"
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					SourceField = new FieldEntry()
					{
						DisplayName = "source id",
						FieldIdentifier = "789456"
					}
					
				}
			};

			_repositoryFactory.GetScratchTableRepository(_sourceWorkspaceArtifactId, _scratchTableName, Arg.Any<string>()).ReturnsForAnyArgs(_scratchTableRepository);

			_instance = new TargetDocumentsTaggingManager(_repositoryFactory, _synchronizer, _sourceWorkspaceManager, _sourceJobManager, 
				_documentRepo, _helper, _fieldMaps, _importConfig, _sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, 
				_jobHistoryArtifactId, _uniqueJobId);
		}

		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void OnJobStart_CreateSourceWorkspaceAndJobHistory()
		{
			// arrange
			_sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId).Returns(_sourceWorkspaceDto);

			//act
			_instance.OnJobStart(_job);

			//assert
			_sourceWorkspaceManager.Received().InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId);
			_sourceJobManager.Received().InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceDto.ArtifactTypeId,
				_sourceWorkspaceDto.ArtifactId,
				_jobHistoryArtifactId);
		}

		[Test]
		public void OnJobStart_SourceWorkspaceManagerFails()
		{
			// arrange
			_sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId).Throws(new Exception());

			//act
			Assert.Throws<Exception>(() => _instance.OnJobStart(_job));

			//assert
			Assert.DoesNotThrow(() => _instance.OnJobComplete(_job));
		}

		[Test]
		public void OnJobComplete_ImportTaggingFieldsWhenThereAreDocumentsToTag()
		{
			//arrange
			SourceJobDTO job = new SourceJobDTO()
			{
				Name = "whatever"
			};

			_sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId).Returns(_sourceWorkspaceDto);
			_sourceJobManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceDto.ArtifactTypeId,
				_sourceWorkspaceDto.ArtifactId,
				_jobHistoryArtifactId).Returns(job);

			_scratchTableRepository.Count.Returns(1);

			//act
			_instance.OnJobStart(_job);
			_instance.OnJobComplete(_job);

			//assert
			_synchronizer.Received(1).SyncData(Arg.Any<TempTableReader>(), Arg.Any<FieldMap[]>(), _importConfig);
		}

		[Test]
		public void OnJobComplete_DoesNotImportTaggingFieldsWhenThereIsNoDocumentToTag()
		{
			//arrange
			SourceJobDTO job = new SourceJobDTO()
			{
				Name = "whatever"
			};

			_sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId).Returns(_sourceWorkspaceDto);
			_sourceJobManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceDto.ArtifactTypeId,
				_sourceWorkspaceDto.ArtifactId,
				_jobHistoryArtifactId).Returns(job);

			//act
			_instance.OnJobStart(_job);
			_instance.OnJobComplete(_job);

			//assert
			_synchronizer.DidNotReceiveWithAnyArgs().SyncData(Arg.Any<TempTableReader>(), Arg.Any<FieldMap[]>(), _importConfig);
		}
	}
}