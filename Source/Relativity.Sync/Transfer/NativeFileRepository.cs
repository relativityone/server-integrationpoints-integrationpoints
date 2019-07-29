using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.WinEDDS.Service.Export;

namespace Relativity.Sync.Transfer
{
	internal sealed class NativeFileRepository : INativeFileRepository
	{
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _SIZE_COLUMN_NAME = "Size";

		private readonly Func<ISearchManager> _searchManagerFactory;
		private readonly ISyncLog _logger;

		public NativeFileRepository(Func<ISearchManager> searchManagerFactory, ISyncLog logger)
		{
			_searchManagerFactory = searchManagerFactory;
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

			using (ISearchManager searchManager = _searchManagerFactory())
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
