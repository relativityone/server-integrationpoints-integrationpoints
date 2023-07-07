﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace kCura.IntegrationPoints.Core.Storage
{
    public class RelativityStorageService : IRelativityStorageService
    {
        private const string _TEAM_ID = "PTCI-2456712";

        private readonly IHelper _helper;
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly ILogger<RelativityStorageService> _logger;

        private IStorageAccess<string> _storageAccess;

        public RelativityStorageService(IHelper helper, IKeplerServiceFactory serviceFactory, ILogger<RelativityStorageService> logger)
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

        public async Task<StorageStream> OpenFileAsync(OpenFileParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            StorageStream stream = await storageAccess.OpenFileAsync(
                parameters.Path,
                parameters.OpenBehavior,
                parameters.ReadWriteMode,
                parameters.OpenFileOptions,
                cancellationToken)
                .ConfigureAwait(false);

            return stream;
        }

        public async Task<IList<string>> ReadAllLinesAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Reading all lines from file: {path}", filePath);

                List<string> lines = new List<string>();

                OpenFileParameters parameters = new OpenFileParameters()
                {
                    Path = filePath,
                    OpenBehavior = OpenBehavior.OpenExisting,
                    ReadWriteMode = ReadWriteMode.ReadOnly
                };

                using (StorageStream storageStream = await OpenFileAsync(parameters).ConfigureAwait(false))
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

        public async Task<DirectoryInfo> PrepareImportDirectoryAsync(int workspaceId, Guid importJobId)
        {
            string workspaceDirectoryPath = await GetWorkspaceDirectoryPathAsync(workspaceId).ConfigureAwait(false);
            DirectoryInfo importDirectory = new DirectoryInfo(Path.Combine(workspaceDirectoryPath, "RIP", "Import", importJobId.ToString()));

            IStorageAccess<string> storageAccess = await GetStorageAccessAsync();
            await storageAccess.CreateDirectoryAsync(importDirectory.FullName).ConfigureAwait(false);

            return importDirectory;
        }

        public async Task DeleteDirectoryRecursiveAsync(string directoryPath)
        {
            IStorageAccess<string> storage = await GetStorageAccessAsync();
            await storage.DeleteDirectoryAsync(directoryPath, new DeleteDirectoryOptions()
            {
                Recursive = true
            }).ConfigureAwait(false);
        }
    }
}