﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public interface IRelativityStorageService
    {
        Task<IStorageAccess<string>> GetStorageAccessAsync();

        Task<StorageStream> CreateFileOrTruncateExistingAsync(string path);

        Task<StorageStream> OpenFileAsync(OpenFileParameters parameters, CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<string>> ReadAllLinesAsync(string filePath);

        Task<string> GetWorkspaceDirectoryPathAsync(int workspaceId);
    }
}