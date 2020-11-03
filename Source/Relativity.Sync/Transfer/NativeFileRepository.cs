using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.WinEDDS.Service.Export;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class NativeFileRepository : INativeFileRepository
	{
		private const int _BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES = 10000;
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _SIZE_COLUMN_NAME = "Size";

		private readonly ISearchManagerFactory _searchManagerFactory;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public NativeFileRepository(ISearchManagerFactory searchManagerFactory, ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_searchManagerFactory = searchManagerFactory;
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<IEnumerable<INativeFile>> QueryAsync(int workspaceId, ICollection<int> documentIds)
		{
			IEnumerable<INativeFile> empty = Enumerable.Empty<INativeFile>();

			if (documentIds == null || !documentIds.Any())
			{
				return empty;
			}

			_logger.LogInformation("Searching for native files. Documents count: {numberOfDocuments}", documentIds.Count);

			using (ISearchManager searchManager = await _searchManagerFactory.CreateSearchManagerAsync().ConfigureAwait(false))
			{
				string concatenatedArtifactIds = string.Join(",", documentIds);
				ISearchManager searchManagerForLambda = searchManager;
				DataSet dataSet = await Task.Run(() => searchManagerForLambda.RetrieveNativesForSearch(workspaceId, concatenatedArtifactIds)).ConfigureAwait(false);

				if (dataSet == null)
				{
					_logger.LogWarning("SearchManager returned null data set.");
					return empty;
				}

				if (dataSet.Tables.Count == 0)
				{
					_logger.LogWarning("SearchManager returned empty data set.");
					return empty;
				}

				DataTable dataTable = dataSet.Tables[0];
				IList<INativeFile> nativeFiles = GetNativeFiles(dataTable);

				_logger.LogInformation("Found {numberOfNatives} native files.", nativeFiles.Count);

				return nativeFiles;
			}
		}

		/// <summary>
		/// Returns long running task
		/// </summary>
		public async Task<long> CalculateNativesTotalSizeAsync(int workspaceId, QueryRequest request)
		{
			_logger.LogInformation("Initializing calculating total natives size (in chunks of {batchSize})", _BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES);
			
			long nativesTotalSize = 0;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				IEnumerable<IList<int>> documentArtifactIdBatches = (await objectManager.QueryAllAsync(workspaceId, request).ConfigureAwait(false))
					.Select(x => x.ArtifactID)
					.SplitList(_BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES);

				int batchIndex = 1;
				foreach (IList<int> batch in documentArtifactIdBatches)
				{
					_logger.LogInformation("Calculating total natives size for {documentsCount} in chunk {batchIndex}.", batch.Count, batchIndex);

					IEnumerable<INativeFile> nativesInBatch = await QueryAsync(workspaceId, batch).ConfigureAwait(false);
					nativesTotalSize += nativesInBatch.Sum(x => x.Size);

					_logger.LogInformation("Calculated total natives size for {documentsCount} in chunk {batchIndex}.", batch.Count, batchIndex++);
				}
			}

			_logger.LogInformation("Finished calculating total natives size (in chunks of {batchSize} ", _BATCH_SIZE_FOR_NATIVES_SIZE_QUERIES);

			return nativesTotalSize;
		}

		private IList<INativeFile> GetNativeFiles(DataTable dataTable)
		{
			List<INativeFile> natives = new List<INativeFile>(dataTable.Rows.Count);

			foreach (DataRow dataRow in dataTable.Rows)
			{
				INativeFile nativeFile = GetNativeFile(dataRow);
				natives.Add(nativeFile);
			}

			return natives;
		}

		private INativeFile GetNativeFile(DataRow dataRow)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME);

			return new NativeFile(documentArtifactId, location, fileName, size);
		}

		private T GetValue<T>(DataRow row, string columnName)
		{
			object value = null;
			try
			{
				value = row[columnName];
				return (T) value;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving native file info from column \"{columnName}\". Value: \"{value}\" Requested type: \"{type}\"", columnName, value, typeof(T));
				throw;
			}
		}
	}
}
