using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public class FileShareDiskUsageReporter : IHealthStatisticReporter, IServiceHealthChecker
    {
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        private bool _isFileshareLogged = false;

        public FileShareDiskUsageReporter(IHelper helper, IAPILog logger)
        {
            _helper = helper;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> GetStatisticAsync()
        {
            Dictionary<string, object> fileShareUsage = new Dictionary<string, object>();
            bool isFileShareServiceAccessible = await IsServiceHealthyAsync().ConfigureAwait(false);
            fileShareUsage.Add("IsFileShareServiceAccessible", isFileShareServiceAccessible);

            return fileShareUsage;
        }

        public async Task<bool> IsServiceHealthyAsync()
        {
            List<string> fileShares = await GetFileServerUNCPaths().ConfigureAwait(false);

            if (fileShares.Count < 1)
            {
                return false;
            }

            foreach (string fileShare in fileShares)
            {
                try
                {
                    LogFileShareNameIfNeeded(fileShare);
                    System.IO.Directory.GetFiles(fileShare);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cannot get files list on {fileShareName}", fileShare);
                    return false;
                }
            }

            return true;
        }

        private async Task<List<string>> GetFileServerUNCPaths()
        {
            List<string> serverList = new List<string>();
            try
            {
                using (IFileShareServerManager resourceServer = _helper.GetServicesManager().CreateProxy<IFileShareServerManager>(ExecutionIdentity.System))
                {
                    FileShareQueryResultSet resultSet = await resourceServer.QueryAsync(new Query()).ConfigureAwait(false);
                    foreach (Result<FileShareResourceServer> result in resultSet.Results)
                    {
                        if (!serverList.Contains(result.Artifact.UNCPath))
                        {
                            serverList.Add(result.Artifact.UNCPath);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cannot check instance file server usage.");
            }

            return serverList;
        }

        private void LogFileShareNameIfNeeded(string fileShare)
        {
            // Make sure we're logging file share name only once per job
            if (_isFileshareLogged)
            {
                return;
            }

            _logger.LogInformation("Checking {fileShareName}", fileShare);
            _isFileshareLogged = true;
        }
    }
}
