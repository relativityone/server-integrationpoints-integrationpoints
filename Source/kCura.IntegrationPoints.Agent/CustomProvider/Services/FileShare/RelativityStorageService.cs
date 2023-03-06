using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public class RelativityStorageService : IRelativityStorageService
    {
        private const string _TEAM_ID = "PTCI-2456712";

        private readonly IHelper _helper;
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        private IStorageAccess<string> _storageAccess;

        public RelativityStorageService(IHelper helper, IKeplerServiceFactory serviceFactory, IAPILog logger)
        {
            _helper = helper;
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task<IStorageAccess<string>> GetStorageAccessAsync()
        {
            if (_storageAccess == null)
            {
                _storageAccess = await _helper.GetStorageAccessorAsync(StorageAccessPermissions.GenericReadWrite, new ApplicationDetails(_TEAM_ID)).ConfigureAwait(false);
            }

            return _storageAccess;
        }

        public async Task<StorageStream> CreateFileOrTruncateExistingAsync(string path)
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            StorageStream stream = await storageAccess.CreateFileOrTruncateExistingAsync(path).ConfigureAwait(false);
            return stream;
        }

        public async Task<StorageStream> OpenFileAsync(string path, OpenBehavior openBehavior, ReadWriteMode readWriteMode, OpenFileOptions openFileOptions = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            StorageStream stream = await storageAccess.OpenFileAsync(path, openBehavior, readWriteMode, openFileOptions, cancellationToken).ConfigureAwait(false);
            return stream;
        }

        public async Task<IList<string>> ReadAllLinesAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Reading all lines from file: {path}", filePath);

                List<string> lines = new List<string>();

                using (StorageStream storageStream = await OpenFileAsync(filePath, OpenBehavior.OpenExisting, ReadWriteMode.ReadOnly).ConfigureAwait(false))
                using (TextReader reader = new StreamReader(storageStream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        lines.Add(line);
                    }
                }

                _logger.LogInformation("Successfully read {lines} lines from file: {path}", lines.Count, filePath);

                return lines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read lines from file: {path}", filePath);
                throw;
            }
        }

        public async Task<string> GetWorkspaceDirectoryPathAsync(int workspaceId)
        {
            using (IWorkspaceManager workspaceManager = await _serviceFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
            {
                WorkspaceRef workspace = new WorkspaceRef()
                {
                    ArtifactID = workspaceId
                };

                FileShareResourceServer server = await workspaceManager
                    .GetDefaultWorkspaceFileShareResourceServerAsync(workspace)
                    .ConfigureAwait(false);

                return Path.Combine(server.UNCPath, $"EDDS{workspaceId}");
            }
        }
    }
}
