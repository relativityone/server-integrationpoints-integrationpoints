using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings
{
    public interface IInstanceSettings
    {
        Task<int> GetCustomProviderBatchSizeAsync();

        Task<TimeSpan> GetCustomProviderProgressUpdateIntervalAsync();

        Task<T> GetAsync<T>(string name, string section, T defaultValue);
    }
}
