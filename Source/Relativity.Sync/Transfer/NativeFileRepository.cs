using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
    internal class NativeFileRepository : INativeFileRepository
    {
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IAPILog _logger;
        private readonly SyncJobParameters _parameters;

        public NativeFileRepository(ISourceServiceFactoryForUser serviceFactoryForUser, IAPILog logger, SyncJobParameters parameters)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _logger = logger;
            _parameters = parameters;
        }

        public async Task<IEnumerable<INativeFile>> QueryAsync(int workspaceId, ICollection<int> documentIds)
        {
            IEnumerable<INativeFile> empty = Enumerable.Empty<INativeFile>();

            if (documentIds == null || !documentIds.Any())
            {
                return empty;
            }

            _logger.LogInformation("Searching for native files. Documents count: {numberOfDocuments}", documentIds.Count);

            using (ISearchService searchService = await _serviceFactoryForUser.CreateProxyAsync<ISearchService>().ConfigureAwait(false))
            {
                string concatenatedArtifactIds = string.Join(",", documentIds);
                DataSetWrapper dataSetWrapper = await searchService.RetrieveNativesForSearchAsync(
                    workspaceId, concatenatedArtifactIds, _parameters.WorkflowId).ConfigureAwait(false);

                if (dataSetWrapper == null)
                {
                    _logger.LogWarning("SearchService returned null data set.");
                    return empty;
                }

                DataSet dataSet = dataSetWrapper.Unwrap();
                if (dataSet.Tables.Count == 0)
                {
                    _logger.LogWarning("SearchService returned empty data set.");
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
            int documentArtifactId = GetValue<int>(dataRow, "DocumentArtifactID");
            string location = GetValue<string>(dataRow, "Location");
            string fileName = GetValue<string>(dataRow, "Filename");
            long size = GetValue<long>(dataRow, "Size");

            return new NativeFile(documentArtifactId, location, fileName, size);
        }

        private T GetValue<T>(DataRow row, string columnName)
        {
            object value = null;
            try
            {
                value = row[columnName];
                return (T)value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving native file info from column \"{columnName}\". Value: \"{value}\" Requested type: \"{type}\"", columnName, value, typeof(T));
                throw;
            }
        }
    }
}
