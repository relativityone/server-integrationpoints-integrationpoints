﻿using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal class ImageSynchronizationExecutor : SynchronizationExecutorBase<IImageSynchronizationConfiguration>
	{
		public ImageSynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory, IDocumentTagRepository documentsTagRepository,
			IFieldManager fieldManager, IFieldMappings fieldMappings, IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration, IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			ISyncLog logger)
			: base(importJobFactory, batchRepository, jobProgressHandlerFactory, documentsTagRepository, fieldManager,
			fieldMappings, jobStatisticsContainer, jobCleanupConfiguration, automatedWorkflowTriggerConfiguration, logger)
		{
		}

		protected override Task<IImportJob> CreateImportJobAsync(IImageSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			return _importJobFactory.CreateImageImportJobAsync(configuration, batch, token);
		}

		protected override void UpdateImportSettings(IImageSynchronizationConfiguration configuration)
		{
			base.UpdateImportSettings(configuration);

			IList<FieldInfoDto> specialFields = _fieldManager.GetImageSpecialFields().ToList();
			configuration.ImageFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.ImageFileLocation);
		}
	}
}
