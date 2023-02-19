using System;
using System.Threading.Tasks;
using Relativity.Toggles;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeToggleProviderWithDefaultValue : IToggleProvider
    {
        private readonly TestContext _testCtx;

        public FakeToggleProviderWithDefaultValue(TestContext testCtx)
        {
            _testCtx = testCtx;
        }

        public MissingFeatureBehavior DefaultMissingFeatureBehavior { get; }

        public bool CacheEnabled { get; set; }

        public int CacheTimeoutInSeconds { get; set; }

        public bool IsEnabled<T>() where T : IToggle
        {
            bool? value = _testCtx.ToggleValues.GetValue<T>();
            if (value.HasValue)
            {
                return value.Value;
            }

            return false;
        }

        public Task<bool> IsEnabledAsync<T>() where T : IToggle
        {
            return Task.FromResult(IsEnabled<T>());
        }

        public bool IsEnabledByName(string toggleName)
        {
            switch (toggleName)
            {
                case "Relativity.Sync.Toggles.EnableJobHistoryStatusUpdateToggle":
                    return false;
                default:
                    return false;
            }
        }

        public Task<bool> IsEnabledByNameAsync(string toggleName)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(bool enabled) where T : IToggle
        {
            throw new NotImplementedException();
        }
    }
}
