using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class DataSourceSnapshotExecutor : IExecutor<IDataSourceSnapshotConfiguration>
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public DataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Initializing export in workpsace {workspaceId} with saved search {savedSearchId} and fields {fields}.", configuration.SourceWorkspaceArtifactId,
				configuration.DataSourceArtifactId, configuration.FieldMappings);

			_logger.LogVerbose("Including following system fields to export {fields}.");


			// proper list of fields will be created in next PR
			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"(('ArtifactId' IN SAVEDSEARCH {configuration.DataSourceArtifactId}))",
				Fields = new[]
				{
					new FieldRef
					{
						Name = "Extracted Text"
					},
					new FieldRef
					{
						Name = "Control Number"
					},
					new FieldRef
					{
						Name = "Supported By Viewer"
					}
				}
			};

			ExportInitializationResults results;
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					results = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "ExportAPI failed to initialize export.");
				throw;
			}

			//ExportInitializationResult provide list of fields with order they will be returned when retrieving metadata
			//however, order is the same as order of fields in QueryRequest when they are provided explicitly
			await configuration.SetSnapshotDataAsync(results.RunID, results.RecordCount).ConfigureAwait(false);
			return ExecutionResult.Success();
		}
	}
}