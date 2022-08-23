﻿using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal interface IInstanceSettings
    {
        Task<string> GetWebApiPathAsync(string defaultValue = default(string));

        Task<bool> GetRestrictReferentialFileLinksOnImportAsync(bool defaultValue = default(bool));

        Task<int> GetSyncBatchSizeAsync(int defaultValue = 25000);

        Task<int> GetImportApiBatchSizeAsync(int defaultValue = 1000);

        Task<bool> GetShouldForceADFTransferAsync(bool defaultValue = false);

        Task<int> GetSyncMaxThreadsCountAsync(int defaultValue = 4);

        Task<T> GetAsync<T>(string name, string section, T defaultValue);
    }
}
