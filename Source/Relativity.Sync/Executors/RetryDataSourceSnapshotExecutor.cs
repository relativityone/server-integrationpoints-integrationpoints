using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal sealed class RetryDataSourceSnapshotExecutor : IExecutor<IRetryDataSourceSnapshotConfiguration>
	{
		private const string _RELATIVITY_NATIVE_TYPE_FIELD_NAME = "RelativityNativeType";
		private const string _SUPPORTED_BY_VIEWER_FIELD_NAME = "SupportedByViewer";

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IFieldManager _fieldManager;
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly INativeFileRepository _nativeFileRepository;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncLog _logger;

		public RetryDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, IFieldManager fieldManager, IJobProgressUpdaterFactory jobProgressUpdaterFactory,
			INativeFileRepository nativeFileRepository, IJobStatisticsContainer jobStatisticsContainer, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_fieldManager = fieldManager;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			_nativeFileRepository = nativeFileRepository;
			_jobStatisticsContainer = jobStatisticsContainer;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IRetryDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Setting {ImportOverwriteMode} from {currentMode} to {appendOverlay} for job retry", nameof(configuration.ImportOverwriteMode), configuration.ImportOverwriteMode, ImportOverwriteMode.AppendOverlay);
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			_logger.LogInformation("{ImportOverwriteMode} successfully to {appendOverlay}", nameof(configuration.ImportOverwriteMode), configuration.ImportOverwriteMode);


			_logger.LogInformation("Initializing export in workspace {workspaceId} with saved search {savedSearchId} and fields {fields}.", configuration.SourceWorkspaceArtifactId,
				configuration.DataSourceArtifactId, configuration.GetFieldMappings());

			_logger.LogVerbose("Including following system fields to export {supportedByViewer}, {nativeType}.", _SUPPORTED_BY_VIEWER_FIELD_NAME, _RELATIVITY_NATIVE_TYPE_FIELD_NAME);

			IEnumerable<FieldInfoDto> documentFields = await _fieldManager.GetDocumentFieldsAsync(token).ConfigureAwait(false);
			IEnumerable<FieldRef> documentFieldRefs = documentFields.Select(f => new FieldRef { Name = f.SourceFieldName });

			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = (int)ArtifactType.Document
				},
				Condition = $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{configuration.JobHistoryToRetryId}])) AND ('Artifact ID' IN SAVEDSEARCH {configuration.DataSourceArtifactId})",
				Fields = documentFieldRefs.ToList()
			};

			ExportInitializationResults results;
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					results = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
					_logger.LogInformation("Retrieved {documentsCount} documents from saved search.", results.RecordCount);

					Task<long> calculateNativesTotalSizeTask = Task.Run(() => _nativeFileRepository.CalculateNativesTotalSizeAsync(configuration.SourceWorkspaceArtifactId, queryRequest), token);
					_jobStatisticsContainer.NativesBytesRequested = calculateNativesTotalSizeTask;
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

			IJobProgressUpdater jobProgressUpdater = _jobProgressUpdaterFactory.CreateJobProgressUpdater();
			await jobProgressUpdater.SetTotalItemsCountAsync((int)results.RecordCount).ConfigureAwait(false);

			return ExecutionResult.Success();
		}
	}
}
