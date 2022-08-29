using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.Utility;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class DiskUsageReporter : IDiskUsageReporter
    {
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public DiskUsageReporter(IHelper helper, IAPILog logger)
        {
            _helper = helper;
            _logger = logger;
        }

        public Dictionary<string, object> GetFileShareUsage()
        {
            Dictionary<string, object> fileShareUsage = new Dictionary<string, object>();
            List<string> fileShares = GetFileServerUNCPaths().GetAwaiter().GetResult();
            foreach (var fileShare in fileShares)
            {
                DriveSpace systemDiscDrive = new DriveSpace(fileShare);
                long systemDiscFreeSpaceGb = systemDiscDrive.TotalFreeSpace / (1024 * 1024 * 1024);
                fileShareUsage.Add(fileShare, systemDiscFreeSpaceGb);
            }

            return fileShareUsage;
        }

        private async Task<List<string>> GetFileServerUNCPaths()
        {
            List<string> serverList = new List<string>();
            using (var resourceServer = _helper.GetServicesManager()
                       .CreateProxy<IFileShareServerManager>(ExecutionIdentity.System))
            {
                var resultSet = await resourceServer.QueryAsync(new Query()).ConfigureAwait(false);
                foreach (Result<FileShareResourceServer> result in resultSet.Results)
                {
                    Uri resultUri = new Uri(result.Artifact.UNCPath);
                    if (!serverList.Contains(resultUri.Host))
                    {
                        _logger.LogInformation("Name: {FileServerName} UNC: {fileServerUNC}", result.Artifact.Name,
                            result.Artifact.UNCPath);
                        serverList.Add(resultUri.Host);
                    }
                }
            }

            return serverList;
        }

    }
}