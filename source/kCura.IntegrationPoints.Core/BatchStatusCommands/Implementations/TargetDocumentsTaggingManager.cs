using System;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Readers;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
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
		private readonly ITempDocTableHelper _tempTableHelper;
		private SourceWorkspaceDTO _sourceWorkspaceDto;
		private SourceJobDTO _sourceJobDto;
		private IScratchTableRepository _scratchTableRepository;
		private bool _errorOccurDuringJobStart;

		public TargetDocumentsTaggingManager(ITempDocTableHelper tempTableHelper,
			IDataSynchronizer synchronizer,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			IDocumentRepository documentRepository,
			FieldMap[] fields,
			string importConfig,
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int jobHistoryArtifactId)
		{
			_tempTableHelper = tempTableHelper;
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

		public void JobComplete(Job job)
		{
			try
			{
				if (!_errorOccurDuringJobStart)
				{
					FieldMap[] identifiers = _fields.Where(f => f.FieldMapType == FieldMapTypeEnum.Identifier).ToArray();
					FieldMap identifier = identifiers[0];

					DataColumn[] columns = new[]
					{
						new DataColumn(identifier.SourceField.FieldIdentifier),
						new DataColumnWithValue(IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD, _sourceWorkspaceDto.Name),
						new DataColumnWithValue(IntegrationPoints.Contracts.Constants.SPECIAL_JOBHISTORY_FIELD , _sourceJobDto.Name)
					};

					int identifierFieldId = Convert.ToInt32(identifier.SourceField.FieldIdentifier);
					TempTableReader reader = new TempTableReader(_documentRepository, ScratchTableRepository, columns, identifierFieldId);
					FieldMap[] fieldsToPush = { identifier };
					if (ScratchTableRepository.Count > 0)
					{
						_synchronizer.SyncData(reader, fieldsToPush, _importConfig);
					}
				}
			}
			finally
			{
				ScratchTableRepository.Dispose();
			}
		}

		public IScratchTableRepository ScratchTableRepository
		{
			get
			{
				if (_scratchTableRepository == null)
				{
					_scratchTableRepository = new ScratchTableRepository(Data.Constants.TEMPORARY_DOC_TABLE_SOURCEWORKSPACE, _tempTableHelper);
				}
				return _scratchTableRepository;
			}
		}

		public void JobStarted(Job job)
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
	}
}