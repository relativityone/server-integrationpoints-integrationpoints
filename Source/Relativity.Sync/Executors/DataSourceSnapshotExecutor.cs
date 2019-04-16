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
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class DataSourceSnapshotExecutor : IExecutor<IDataSourceSnapshotConfiguration>
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;

		private const string _SUPPORTED_BY_VIEWER_FIELD_NAME = "SupportedByViewer";
		private const string _RELATIVITY_NATIVE_TYPE_FIELD_NAME = "RelativityNativeType";

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public DataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Initializing export in workspace {workspaceId} with saved search {savedSearchId} and fields {fields}.", configuration.SourceWorkspaceArtifactId,
				configuration.DataSourceArtifactId, configuration.FieldMappings);

			_logger.LogVerbose("Including following system fields to export {supportedByViewer}, {nativeType}.", _SUPPORTED_BY_VIEWER_FIELD_NAME, _RELATIVITY_NATIVE_TYPE_FIELD_NAME);

			IEnumerable<FieldRef> fields = PrepareFieldsList(configuration);

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"(('ArtifactId' IN SAVEDSEARCH {configuration.DataSourceArtifactId}))",
				Fields = fields.ToList()
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
				return ExecutionResult.Failure("ExportAPI failed to initialize export.", e);
			}

			//ExportInitializationResult provide list of fields with order they will be returned when retrieving metadata
			//however, order is the same as order of fields in QueryRequest when they are provided explicitly
			await configuration.SetSnapshotDataAsync(results.RunID, results.RecordCount).ConfigureAwait(false);
			return ExecutionResult.Success();
		}

		private IEnumerable<FieldRef> PrepareFieldsList(IDataSourceSnapshotConfiguration configuration)
		{
			foreach (FieldMap fieldMap in configuration.FieldMappings)
			{
				yield return new FieldRef
				{
					ArtifactID = fieldMap.SourceField.FieldIdentifier
				};
			}

			yield return new FieldRef
			{
				Name = _SUPPORTED_BY_VIEWER_FIELD_NAME
			};
			yield return new FieldRef
			{
				Name = _RELATIVITY_NATIVE_TYPE_FIELD_NAME
			};
			if (configuration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				_logger.LogVerbose("Including field {artifactId} used to retrieving destination folder structure.", configuration.FolderPathSourceFieldArtifactId);
				yield return new FieldRef
				{
					ArtifactID = configuration.FolderPathSourceFieldArtifactId
				};
			}
		}
	}
}