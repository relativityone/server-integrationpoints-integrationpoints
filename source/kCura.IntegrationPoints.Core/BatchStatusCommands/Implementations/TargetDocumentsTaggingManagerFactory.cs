using System;
using System.Linq;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class TargetDocumentsTaggingManagerFactory
	{
		private readonly ITempDocTableHelper _tempTableHelper;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly ISourceJobManager _sourceJobManager;
		private readonly IDocumentRepository _documentRepository;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly FieldMap[] _fields;
		private readonly string _sourceConfig;
		private readonly string _destinationConfig;
		private readonly int _jobHistoryArtifactId;

		public TargetDocumentsTaggingManagerFactory(
			ITempDocTableHelper tempTableHelper,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			IDocumentRepository documentRepository,
			ISynchronizerFactory synchronizerFactory,
			FieldMap[] fields,
			string sourceConfig,
			string destinationConfig,
			int jobHistoryArtifactId)
		{
			_tempTableHelper = tempTableHelper;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_sourceJobManager = sourceJobManager;
			_documentRepository = documentRepository;
			_synchronizerFactory = synchronizerFactory;
			_fields = fields;
			_sourceConfig = sourceConfig;
			_jobHistoryArtifactId = jobHistoryArtifactId;

			// specify settings to tag
			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(destinationConfig);
			importSettings.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
			importSettings.FieldOverlayBehavior = "Merge Values";
			importSettings.CopyFilesToDocumentRepository = false;
			importSettings.FileNameColumn = null;
			importSettings.NativeFilePathSourceFieldName = null;
			importSettings.FolderPathSourceFieldName = null;
			importSettings.Provider = String.Empty;
			importSettings.ImportNativeFile = false;
			_destinationConfig = JsonConvert.SerializeObject(importSettings);
		}

		public IConsumeScratchTableBatchStatus BuildDocumentsTagger()
		{
			var settings = JsonConvert.DeserializeObject<ExportSettings>(_sourceConfig);
			var synchronizer = GetSynchronizerForDocumentTagging(_destinationConfig);

			IConsumeScratchTableBatchStatus tagger = new TargetDocumentsTaggingManager(
				_tempTableHelper,
				synchronizer,
				_sourceWorkspaceManager,
				_sourceJobManager,
				_documentRepository,
				_fields.ToArray(),
				_destinationConfig,
				settings.SourceWorkspaceArtifactId,
				settings.TargetWorkspaceArtifactId,
				_jobHistoryArtifactId);

			return tagger;
		}

		internal IDataSynchronizer GetSynchronizerForDocumentTagging(string configuration)
		{
			IDataSynchronizer synchronizer = _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _destinationConfig);
			return synchronizer;
		}
	}
}