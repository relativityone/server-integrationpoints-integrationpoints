using System;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Injection;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class TargetDocumentsTaggingManager : IConsumeScratchTableBatchStatus
	{
		private readonly int _destinationWorkspaceArtifactId;
		private readonly int? _federatedInstanceArtifactId;
		private readonly IDocumentRepository _documentRepository;
		private readonly FieldMap[] _fields;
		private readonly string _importConfig;
		private readonly int _jobHistoryArtifactId;
		private readonly IAPILog _logger;
		private readonly ISourceJobManager _sourceJobManager;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly IDataSynchronizer _synchronizer;
		private bool _errorOccurDuringJobStart;
		private SourceJobDTO _sourceJobDto;
		private SourceWorkspaceDTO _sourceWorkspaceDto;

		public TargetDocumentsTaggingManager(
			IRepositoryFactory repositoryFactory,
			IDataSynchronizer synchronizer,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			IDocumentRepository documentRepository,
			IHelper helper,
			FieldMap[] fields,
			string importConfig,
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int? federatedInstanceArtifactId,
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
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<TargetDocumentsTaggingManager>();

			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_destinationWorkspaceArtifactId = destinationWorkspaceArtifactId;
			_federatedInstanceArtifactId = federatedInstanceArtifactId;
			_jobHistoryArtifactId = jobHistoryArtifactId;
			_importConfig = importConfig;
		}

		public IScratchTableRepository ScratchTableRepository { get; }

		public void OnJobStart(Job job)
		{
			try
			{
				_sourceWorkspaceDto = _sourceWorkspaceManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _federatedInstanceArtifactId);
				_sourceJobDto = _sourceJobManager.InitializeWorkspace(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _sourceWorkspaceDto.ArtifactTypeId,
					_sourceWorkspaceDto.ArtifactId, _jobHistoryArtifactId);
			}
			catch (Exception e)
			{
				LogErrorDuringJobStart(e);
				_errorOccurDuringJobStart = true;
				throw;
			}
		}

		public void OnJobComplete(Job job)
		{
			InjectionManager.Instance.Evaluate(InjectionPoints.BEFORE_TAGGING_STARTS_ONJOBCOMPLETE.Id);
			try
			{
				if (!_errorOccurDuringJobStart)
				{
					FieldMap identifier = _fields.First(f => f.FieldMapType == FieldMapTypeEnum.Identifier);

					DataColumn[] columns =
					{
						new DataColumn(identifier.SourceField.FieldIdentifier),
						new DataColumnWithValue(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD, _sourceWorkspaceDto.Name),
						new DataColumnWithValue(IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD, _sourceJobDto.Name)
					};

					int identifierFieldId = Convert.ToInt32(identifier.SourceField.FieldIdentifier);
					using (TempTableReader reader = new TempTableReader(_documentRepository, ScratchTableRepository, columns, identifierFieldId))
					{
						FieldMap[] fieldsToPush = {identifier};
						var documentTransferContext = new DefaultTransferContext(reader);
						if (ScratchTableRepository.Count > 0)
						{
							_synchronizer.SyncData(documentTransferContext, fieldsToPush, _importConfig);
						}
					}
				}
			}
			catch (Exception e)
			{
				LogErrorDuringJobComplete(e);
				throw;
			}
			finally
			{
				if (_federatedInstanceArtifactId == null)
				{
					ScratchTableRepository.Dispose();
				}
			}
		}

		#region Logging

		private void LogErrorDuringJobStart(Exception e)
		{
			_logger.LogError(e, $"Error occurred during job starting in {nameof(TargetDocumentsTaggingManager)}");
		}

		private void LogErrorDuringJobComplete(Exception e)
		{
			_logger.LogError(e, $"Error occurred during job completion in {nameof(TargetDocumentsTaggingManager)}");
		}

		#endregion
	}
}