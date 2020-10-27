using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.WinEDDS;
using Relativity.API.Foundation;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal sealed class ImageRetryDataSourceSnapshotExecutor : IExecutor<IImageRetryDataSourceSnapshotConfiguration>
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;
		private const int _HAS_IMAGES_YES_CHOICE = 1034243;
		
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly IImageFileRepository _imageFileRepository;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncLog _logger;

		public ImageRetryDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, IJobProgressUpdaterFactory jobProgressUpdaterFactory,
			IImageFileRepository imageFileRepository, IJobStatisticsContainer jobStatisticsContainer, IFieldManager fieldManager, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			_imageFileRepository = imageFileRepository;
			_jobStatisticsContainer = jobStatisticsContainer;
			_fieldManager = fieldManager;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IImageRetryDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Setting {ImportOverwriteMode} from {currentMode} to {appendOverlay} for job retry", nameof(configuration.ImportOverwriteMode), configuration.ImportOverwriteMode, ImportOverwriteMode.AppendOverlay);
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			_logger.LogInformation("{ImportOverwriteMode} successfully to {appendOverlay}", nameof(configuration.ImportOverwriteMode), configuration.ImportOverwriteMode);

			_logger.LogInformation("Initializing image export in workspace {workspaceId} with saved search {savedSearchId}.",
				configuration.SourceWorkspaceArtifactId, configuration.DataSourceArtifactId);
			
			FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{configuration.JobHistoryToRetryId}])) " +
							$"AND ('Artifact ID' IN SAVEDSEARCH {configuration.DataSourceArtifactId}) AND ('Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE})",
				Fields = new []{ new FieldRef { Name = identifierField.SourceFieldName } }
			};

			ExportInitializationResults results;
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					results = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
					_logger.LogInformation("Retrieved {documentCount} documents from saved search which have images", results.RecordCount);

					QueryImagesOptions options = new QueryImagesOptions
					{
						ProductionIds = configuration.ProductionIds,
						IncludeOriginalImageIfNotFoundInProductions = configuration.IncludeOriginalImageIfNotFoundInProductions
					};

					Task<ImagesStatistics> calculateImagesTotalSizeTask = Task.Run(() => _imageFileRepository.CalculateImagesStatisticsAsync(configuration.SourceWorkspaceArtifactId, queryRequest, options), token);
					_jobStatisticsContainer.ImagesStatistics = calculateImagesTotalSizeTask;
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "ExportAPI failed to initialize export.");
				return ExecutionResult.Failure("ExportAPI failed to initialize export.", e);
			}

			//ExportInitializationResult provide list of fields with order they will be returned when retrieving metadata
			//however, order is the same as order of fields in QueryRequest when they are provided explicitly
			await configuration.SetSnapshotDataAsync(results.RunID, (int)results.RecordCount).ConfigureAwait(false);

			return ExecutionResult.Success();
		}
	}
}