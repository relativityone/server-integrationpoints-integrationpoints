﻿using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Managers
{
	[TestFixture]
	[Category("Integration Tests")]
	public class TargetDocumentsTaggingManagerTests : RelativityProviderTemplate
	{
		private IRepositoryFactory _repositoryFactory;
		private IDocumentRepository _documentRepository;
		private SourceWorkspaceManager _sourceWorkspaceManager;
		private SourceJobManager _sourceJobManager;
		private ISynchronizerFactory _synchronizerFactory;
		private IJobHistoryService _jobHistoryService;
		private IFieldRepository _fieldRepository;
		private FieldMap[] _fieldMaps;
		private ISerializer _serializer;
		private const int _ADMIN_USER_ID = 9;
		private const string _RELATIVITY_SOURCE_CASE = "Relativity Source Case";
		private const string _RELATIVITY_SOURCE_JOB = "Relativity Source Job";

		public TargetDocumentsTaggingManagerTests() : base("TargetDocumentsTaggingManagerSource", null)
		{
		}

		public override void SuiteSetup()
		{
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_serializer = Container.Resolve<ISerializer>();
			_sourceWorkspaceManager = new SourceWorkspaceManager(_repositoryFactory);
			_sourceJobManager = new SourceJobManager(_repositoryFactory);
			_synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldRepository = _repositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId);
			_fieldMaps = GetDefaultFieldMap();
		}

		[Test]
		[TestCase(499, "UnderBatch")]
		[TestCase(500, "EqualBatch")]
		[TestCase(502, "OverBatch")]
		public void TargetWorkspaceDocumentTagging_GoldFlow(int numberOfDocuments, string documentIdentifier)
		{
			//Act
			string expectedRelativitySourceCase = $"TargetDocumentsTaggingManagerSource - {SourceWorkspaceArtifactId}";
			DataTable dataTable = Import.GetImportTable(documentIdentifier, numberOfDocuments);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, dataTable);
			int[] documentArtifactIds = _documentRepository.RetrieveDocumentByIdentifierPrefixAsync(Fields.GetDocumentIdentifierFieldName(_fieldRepository), documentIdentifier).ConfigureAwait(false).GetAwaiter().GetResult();

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};
			IntegrationModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Data.IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationModelCreated.ArtifactID);
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, Guid.NewGuid(), DateTime.Now);

			TargetDocumentsTaggingManagerFactory targetDocumentsTaggingManagerFactory = new TargetDocumentsTaggingManagerFactory(_repositoryFactory, _sourceWorkspaceManager, _sourceJobManager, _documentRepository, _synchronizerFactory, _serializer, _fieldMaps, integrationModelCreated.SourceConfiguration, integrationModelCreated.Destination, jobHistory.ArtifactId, jobHistory.BatchInstance);
			IConsumeScratchTableBatchStatus targetDocumentsTaggingManager = targetDocumentsTaggingManagerFactory.BuildDocumentsTagger();
			targetDocumentsTaggingManager.ScratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIds);

			//Act
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationModelCreated.ArtifactID, _ADMIN_USER_ID, 1);
			targetDocumentsTaggingManager.OnJobStart(job);
			targetDocumentsTaggingManager.OnJobComplete(job);

			//Assert
			VerifyRelativitySourceJobAndSourceCase(documentArtifactIds, jobHistory.Name, expectedRelativitySourceCase);
		}

		private void VerifyRelativitySourceJobAndSourceCase(int[] documentArtifactIds, string expectedSourceJob, string expectedSourceCase)
		{
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);

			int relativitySourceCaseFieldArtifactId = _fieldRepository.RetrieveField(_RELATIVITY_SOURCE_CASE, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).GetValueOrDefault();
			int relativitySourceJobdArtifactId = _fieldRepository.RetrieveField(_RELATIVITY_SOURCE_JOB, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject).GetValueOrDefault();

			ArtifactDTO[] documentArtifacts =
				_documentRepository.RetrieveDocumentsAsync(documentArtifactIds,
					new HashSet<int>() { relativitySourceJobdArtifactId, relativitySourceCaseFieldArtifactId })
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				if (artifact.Fields[0].Value == null || !artifact.Fields[0].Value.ToString().Contains(expectedSourceJob))
				{
					throw new Exception($"Failed to correctly tag Document field 'Relativity Source Job'. Expected value: {expectedSourceJob}. Actual: {artifact.Fields[1].Value}.");
				}

				if (artifact.Fields[1].Value == null || !artifact.Fields[1].Value.ToString().Contains(expectedSourceCase))
				{
					throw new Exception($"Failed to correctly tag Document field 'Relativity Source Case'. Expected value: {expectedSourceCase}. Actual: {artifact.Fields[0].Value}.");
				}
			}
		}
	}
}