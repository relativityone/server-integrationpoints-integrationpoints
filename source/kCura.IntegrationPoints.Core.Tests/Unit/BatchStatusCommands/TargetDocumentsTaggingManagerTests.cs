using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.BatchStatusCommands
{
	[TestFixture]
	public class TargetDocumentsTaggingManagerTests
	{
		private ITempDocTableHelper _tempTableHelper;
		private IDataSynchronizer _synchronizer;
		private ISourceWorkspaceManager _sourceWorkspaceManager;
		private ISourceJobManager _sourceJobManager;
		private IDocumentRepository _documentRepo;
		private string _importConfig;
		private int _sourceWorkspaceArtifactId;
		private int _destinationWorkspaceArtifactId;
		private int _jobHistoryArtifactId;

		private FieldMap[] _fieldMaps;
		private TargetDocumentsTaggingManager _instance;
		private Job _job;

		readonly SourceWorkspaceDTO _sourceWorkspaceDto = new SourceWorkspaceDTO()
		{
			Name = "source workspace",
			ArtifactTypeId = 410,
			ArtifactId = 987
		};

		[TestFixtureSetUp]
		public void Setup()
		{
			_tempTableHelper = NSubstitute.Substitute.For<ITempDocTableHelper>();
			_synchronizer = NSubstitute.Substitute.For<IDataSynchronizer>();
			_sourceWorkspaceManager = NSubstitute.Substitute.For<ISourceWorkspaceManager>();
			_sourceJobManager = NSubstitute.Substitute.For<ISourceJobManager>();
			_documentRepo = NSubstitute.Substitute.For<IDocumentRepository>();

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
			_instance = new TargetDocumentsTaggingManager(_tempTableHelper, _synchronizer, 
				_sourceWorkspaceManager, _sourceJobManager, 
				_documentRepo, _fieldMaps, _importConfig,
				_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, 
				_jobHistoryArtifactId);
		}

		[Test]
		public void JobStarted_CreateSourceWorkspaceAndJobHistory()
		{
			// arrange
			_sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId).Returns(_sourceWorkspaceDto);

			//act
			_instance.JobStarted(_job);

			//assert
			_sourceWorkspaceManager.Received().InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId);
			_sourceJobManager.Received().InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId,
				_sourceWorkspaceDto.ArtifactTypeId,
				_sourceWorkspaceDto.ArtifactId,
				_jobHistoryArtifactId);
		}

		[Test]
		public void JobStarted_SourceWorkspaceManagerFails()
		{
			// arrange
			_sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId).Throws(new Exception());

			//act
			Assert.Throws<Exception>(() => _instance.JobStarted(_job));

			//assert
			Assert.DoesNotThrow(() => _instance.JobComplete(_job));
		}

		[Test]
		public void JobComplete_ImportTaggingFieldsWhenThereAreDocumentsToTag()
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

			_instance.ScratchTableRepository.AddArtifactIdsIntoTempTable(new List<int>() { 1, 2 });

			//act
			_instance.JobStarted(_job);
			_instance.JobComplete(_job);

			//assert
			_synchronizer.Received().SyncData(Arg.Any<TempTableReader>(), Arg.Any<FieldMap[]>(), _importConfig);
		}

		[Test]
		public void JobComplete_DoesNotImportTaggingFieldsWhenThereIsNoDocumentToTag()
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
			_instance.JobStarted(_job);
			_instance.JobComplete(_job);

			//assert
			_synchronizer.DidNotReceiveWithAnyArgs().SyncData(Arg.Any<TempTableReader>(), Arg.Any<FieldMap[]>(), _importConfig);
		}
	}
}