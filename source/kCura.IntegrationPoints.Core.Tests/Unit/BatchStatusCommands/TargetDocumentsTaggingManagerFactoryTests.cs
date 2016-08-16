﻿using Castle.Core.Internal;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.BatchStatusCommands
{
	[TestFixture]
	public class TargetDocumentsTaggingManagerFactoryTests
	{
		private const string _SOURCE_CONFIG = "source config";
		private const string _DEST_CONFIG = "destination config";
		private const string _NEW_DEST_CONFIG = "new destination config";
		private const int _JOBHISTORY_ARTIFACT_ID = 321;
		private const string _UNIQUE_JOBID = "very unique";

		private IRepositoryFactory _repositoryFactory;
		private ISourceWorkspaceManager _sourceWorkspaceManager;
		private ISourceJobManager _sourceJobManager;
		private IDocumentRepository _documentRepository;
		private ISynchronizerFactory _synchronizerFactory;
		private ISerializer _serializer;
		private FieldMap[] _fields;
		private TargetDocumentsTaggingManagerFactory _instance;
		private ImportSettings _settings;
		private IDataSynchronizer _dataSynchronizer;

		[SetUp]
		public void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
			_sourceJobManager = Substitute.For<ISourceJobManager>();
			_documentRepository = Substitute.For<IDocumentRepository>();
			_synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_serializer = Substitute.For<ISerializer>();
			_dataSynchronizer = Substitute.For<IDataSynchronizer>();
			_fields = new FieldMap[0];
			_settings = new ImportSettings();
			_serializer.Deserialize<ImportSettings>(_DEST_CONFIG).Returns(_settings);
			_serializer.Serialize(_settings).Returns(_NEW_DEST_CONFIG);
		}

		[Test]
		public void Init_OverrideImportSettings()
		{
			// ARRANGE & ACT
			_instance = new TargetDocumentsTaggingManagerFactory
			(
				_repositoryFactory,
				_sourceWorkspaceManager,
				_sourceJobManager,
				_documentRepository,
				_synchronizerFactory,
				_serializer,
				_fields,
				_SOURCE_CONFIG,
				_DEST_CONFIG,
				_JOBHISTORY_ARTIFACT_ID,
				_UNIQUE_JOBID
			);

			// ASSERT
			Assert.AreEqual(ImportOverwriteModeEnum.OverlayOnly, _settings.ImportOverwriteMode);
			Assert.AreEqual(ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE, _settings.FieldOverlayBehavior);
			Assert.IsFalse(_settings.CopyFilesToDocumentRepository);
			Assert.IsNull(_settings.FileNameColumn);
			Assert.IsNull(_settings.NativeFilePathSourceFieldName);
			Assert.IsNull(_settings.FolderPathSourceFieldName);
			Assert.That(_settings.Provider, Is.Null.Or.Empty);
			Assert.IsFalse(_settings.ImportNativeFile);
		}

		[Test]
		public void BuildDocumentsTagger_GoldFlow()
		{
			// ARRANGE
			ExportSettings exportSettings = new ExportUsingSavedSearchSettings()
			{
				SourceWorkspaceArtifactId = 1,
				TargetWorkspaceArtifactId = 2
			};
			_serializer.Deserialize<ExportSettings>(_SOURCE_CONFIG).Returns(exportSettings);
			_synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _NEW_DEST_CONFIG).Returns(_dataSynchronizer);
			_instance = new TargetDocumentsTaggingManagerFactory
			(
				_repositoryFactory,
				_sourceWorkspaceManager,
				_sourceJobManager,
				_documentRepository,
				_synchronizerFactory,
				_serializer,
				_fields,
				_SOURCE_CONFIG,
				_DEST_CONFIG,
				_JOBHISTORY_ARTIFACT_ID,
				_UNIQUE_JOBID
			);

			// ACT
			IConsumeScratchTableBatchStatus tagger = _instance.BuildDocumentsTagger();

			// ASSERT
			Assert.IsNotNull(tagger);
			Assert.IsTrue(tagger is TargetDocumentsTaggingManager);
		}
	}
}