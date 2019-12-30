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
	internal sealed class DataSourceSnapshotExecutor : IExecutor<IDataSourceSnapshotConfiguration>
	{
		private const int _BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES = 10000;
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;
		private const string _RELATIVITY_NATIVE_TYPE_FIELD_NAME = "RelativityNativeType";
		private const string _SUPPORTED_BY_VIEWER_FIELD_NAME = "SupportedByViewer";

		private readonly IFieldManager _fieldManager;
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly INativeFileRepository _nativeFileRepository;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public DataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, IFieldManager fieldManager, IJobProgressUpdaterFactory jobProgressUpdaterFactory,
			INativeFileRepository nativeFileRepository, IJobStatisticsContainer jobStatisticsContainer, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_fieldManager = fieldManager;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			_nativeFileRepository = nativeFileRepository;
			_jobStatisticsContainer = jobStatisticsContainer;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Initializing export in workspace {workspaceId} with saved search {savedSearchId} and fields {fields}.", configuration.SourceWorkspaceArtifactId,
				configuration.DataSourceArtifactId, configuration.GetFieldMappings());

			_logger.LogVerbose("Including following system fields to export {supportedByViewer}, {nativeType}.", _SUPPORTED_BY_VIEWER_FIELD_NAME, _RELATIVITY_NATIVE_TYPE_FIELD_NAME);

			IEnumerable<FieldInfoDto> documentFields = await _fieldManager.GetDocumentFieldsAsync(token).ConfigureAwait(false);
			IEnumerable<FieldRef> documentFieldRefs = documentFields.Select(f => new FieldRef {Name = f.SourceFieldName});

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"(('ArtifactId' IN SAVEDSEARCH {configuration.DataSourceArtifactId}))",
				Fields = documentFieldRefs.ToList()
			};

			ExportInitializationResults results;
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					results = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
					Task<long> calculateNativesTotalSizeTask = Task.Run(() => CalculateNativesTotalSizeAsync(configuration.SourceWorkspaceArtifactId, queryRequest, (int)results.RecordCount), token);
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
			await jobProgressUpdater.SetTotalItemsCountAsync((int) results.RecordCount).ConfigureAwait(false);

			return ExecutionResult.Success();
		}

		private async Task<long> CalculateNativesTotalSizeAsync(int workspaceId, QueryRequest request, int recordCount)
		{
			long nativesTotalSize = 0;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				List<int> allDocumentsArtifactIds = await GetAllDocumentsArtifactIds(workspaceId, objectManager, request, recordCount).ConfigureAwait(false);
				IEnumerable<IList<int>> documentArtifactIdBatches = allDocumentsArtifactIds.SplitList(_BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES);

				foreach (IList<int> batch in documentArtifactIdBatches)
				{
					IEnumerable<INativeFile> nativesInBatch = await _nativeFileRepository.QueryAsync(workspaceId, batch).ConfigureAwait(false);
					nativesTotalSize += nativesInBatch.Sum(x => x.Size);
				}
			}
			return nativesTotalSize;
		}

		private static async Task<List<int>> GetAllDocumentsArtifactIds(int workspaceId, IObjectManager objectManager, QueryRequest request, int recordCount)
		{
			var allDocumentsArtifactIds = new List<int>(recordCount);
			for (int start = 1; start <= recordCount; start += _BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES)
			{
				int[] batch = (await objectManager.QueryAsync(workspaceId, request, start, _BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES).ConfigureAwait(false))
					.Objects.Select(x => x.ArtifactID).ToArray();
				allDocumentsArtifactIds.AddRange(batch);
			}
			return allDocumentsArtifactIds;
		}
	}
}