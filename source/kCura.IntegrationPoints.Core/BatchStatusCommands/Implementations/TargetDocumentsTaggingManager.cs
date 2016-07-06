using System;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class TargetDocumentsTaggingManager : IConsumeScratchTableBatchStatus
	{
		private readonly int _destinationWorkspaceArtifactId;
		private readonly IDocumentRepository _documentRepository;
		private readonly FieldMap[] _fields;
		private readonly string _importConfig;
		private readonly int _jobHistoryArtifactId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly IDataSynchronizer _synchronizer;
		private readonly ISourceJobManager _sourceJobManager;
		private SourceWorkspaceDTO _sourceWorkspaceDto;
		private SourceJobDTO _sourceJobDto;
		private bool _errorOccurDuringJobStart;

		public TargetDocumentsTaggingManager(
			IRepositoryFactory repositoryFactory,
			IDataSynchronizer synchronizer,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			IDocumentRepository documentRepository,
			FieldMap[] fields,
			string importConfig,
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int jobHistoryArtifactId, 
			string uniqueJobId)
		{
			ScratchTableRepository = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId,
				Data.Constants.TEMPORARY_DOC_TABLE_SOURCEWORKSPACE, uniqueJobId);
			_synchronizer = synchronizer;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_sourceJobManager = sourceJobManager;
			_documentRepository = documentRepository;
			_documentRepository.WorkspaceArtifactId = sourceWorkspaceArtifactId;
			_fields = fields;

			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_destinationWorkspaceArtifactId = destinationWorkspaceArtifactId;
			_jobHistoryArtifactId = jobHistoryArtifactId;
			_importConfig = importConfig;
		}

		public IScratchTableRepository ScratchTableRepository { get; }

		public void OnJobStart(Job job)
		{
			try
			{
				_sourceWorkspaceDto = _sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId);
				_sourceJobDto = _sourceJobManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _sourceWorkspaceDto.ArtifactTypeId, _sourceWorkspaceDto.ArtifactId, _jobHistoryArtifactId);
			}
			catch (Exception)
			{
				_errorOccurDuringJobStart = true;
				throw;
			}
		}

		public void OnJobComplete(Job job)
		{
			try
			{
				if (!_errorOccurDuringJobStart)
				{
					FieldMap identifier = _fields.First(f => f.FieldMapType == FieldMapTypeEnum.Identifier);

					DataColumn[] columns = new[]
					{
						new DataColumn(identifier.SourceField.FieldIdentifier),
						new DataColumnWithValue(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD, _sourceWorkspaceDto.Name),
						new DataColumnWithValue(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD , _sourceJobDto.Name)
					};

					int identifierFieldId = Convert.ToInt32(identifier.SourceField.FieldIdentifier);
					using (TempTableReader reader = new TempTableReader(_documentRepository, ScratchTableRepository, columns, identifierFieldId))
					{
						FieldMap[] fieldsToPush = { identifier };
						if (ScratchTableRepository.Count > 0)
						{
							_synchronizer.SyncData(reader, fieldsToPush, _importConfig);
						}
					}
				}
			}
			finally
			{
				ScratchTableRepository.Dispose();
			}
		}
	}
}