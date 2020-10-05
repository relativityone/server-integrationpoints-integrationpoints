using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal class DocumentSynchronizationExecutor : SynchronizationExecutorBase<IDocumentSynchronizationConfiguration>
	{
		public DocumentSynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory, IDocumentTagRepository documentsTagRepository,
			IFieldManager fieldManager, IFieldMappings fieldMappings, IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration, IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			ISyncLog logger) : base(importJobFactory, batchRepository, jobProgressHandlerFactory, documentsTagRepository, fieldManager,
			fieldMappings, jobStatisticsContainer, jobCleanupConfiguration, automatedWorkflowTriggerConfiguration, logger)
		{
		}

		protected override Task<IImportJob> CreateImportJobAsync(IDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			return _importJobFactory.CreateNativeImportJobAsync(configuration, batch, token);
		}

		protected override void UpdateImportSettings(IDocumentSynchronizationConfiguration configuration)
		{
			base.UpdateImportSettings(configuration);

			IList<FieldInfoDto> specialFields = _fieldManager.GetNativeSpecialFields().ToList();
			if (configuration.DestinationFolderStructureBehavior != DestinationFolderStructureBehavior.None)
			{
				configuration.FolderPathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.FolderPath);
			}

			configuration.FileSizeColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileSize);
			configuration.NativeFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileLocation);
			configuration.FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileFilename);
			configuration.OiFileTypeColumnName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.RelativityNativeType);
			configuration.SupportedByViewerColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.SupportedByViewer);
		}
	}
}
