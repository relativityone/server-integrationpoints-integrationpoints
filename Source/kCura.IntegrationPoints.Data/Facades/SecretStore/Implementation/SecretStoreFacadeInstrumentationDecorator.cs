using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation
{
    internal class SecretStoreFacadeInstrumentationDecorator : ISecretStoreFacade
    {
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
        private readonly ISecretStoreFacade _secretStore;

        public SecretStoreFacadeInstrumentationDecorator(
            ISecretStoreFacade secretStore,
            IExternalServiceInstrumentationProvider instrumentationProvider)
        {
            _secretStore = secretStore;
            _instrumentationProvider = instrumentationProvider;
        }

        public Task<Secret> GetAsync(string path)
        {
            return Execute(x => x.GetAsync(path));
        }

        public Task SetAsync(string path, Secret secret)
        {
            return Execute(x => x.SetAsync(path, secret));
        }

        public Task DeleteAsync(string path)
        {
            return Execute(x => x.DeleteAsync(path));
        }

        private async Task<T> Execute<T>(Func<ISecretStoreFacade, Task<T>> f)
        {
            IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
            try
            {
                return await f(_secretStore).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                startedInstrumentation.Failed(ex);
                throw;
            }
            finally
            {
                startedInstrumentation.Completed();
            }
        }

        private async Task Execute(Func<ISecretStoreFacade, Task> f)
        {
            IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
            try
            {
                await f(_secretStore).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                startedInstrumentation.Failed(ex);
                throw;
            }
            finally
            {
                startedInstrumentation.Completed();
            }
        }

        private IExternalServiceInstrumentationStarted StartInstrumentation([CallerMemberName] string operationName = "")
        {
            IExternalServiceInstrumentation instrumentation =
                _instrumentationProvider.Create(
                    ExternalServiceTypes.SECRET_STORE,
                    nameof(ISecretStore),
                    operationName
                );
            return instrumentation.Started();
        }
    }
}
