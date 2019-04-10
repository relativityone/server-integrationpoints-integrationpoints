using kCura.EventHandler;
using kCura.IntegrationPoints.Services;
using kCura.IntegrationPoints.SourceProviderInstaller.Internals;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
    /// <summary>
    /// Occurs immediately before the execution of a Pre Uninstall event handler.
    /// </summary>
    public delegate void PreUninstallPreExecuteEvent();

    /// <summary>
    /// Occurs after the source provider is removed from the database table.
    /// </summary>
    /// <param name="isUninstalled"></param>
    /// <param name="ex"></param>
    public delegate void PreUninstallPostExecuteEvent(bool isUninstalled, Exception ex);

    /// <summary>
    /// Removes a data source provider when the user uninstalls the application from a workspace.
    /// </summary>
    public abstract class IntegrationPointSourceProviderUninstaller : PreUninstallEventHandler
    {
        private const int _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER = 3;
        private const int _SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS = 3000;
        private const string _SUCCESS_MESSAGE = "Source Provider uninstalled successfully.";
        private const string _FAILURE_MESSAGE = "Uninstalling source provider failed.";

        private readonly Lazy<IAPILog> _logggerLazy;

        /// <summary>
        /// Occurs before the removal of the data source provider.
        /// </summary>
        public event PreUninstallPreExecuteEvent RaisePreUninstallPreExecuteEvent;

        /// <summary>
        /// Occurs after the removal of all the data source providers in the current application.
        /// </summary>
        public event PreUninstallPostExecuteEvent RaisePreUninstallPostExecuteEvent;

        private IAPILog Logger => _logggerLazy.Value;

        /// <summary>
        /// Creates a new instance of the data source uninstall provider.
        /// </summary>
        protected IntegrationPointSourceProviderUninstaller()
        {
            _logggerLazy = new Lazy<IAPILog>(
                () => Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointSourceProviderInstaller>()
            );
        }

        /// <summary>
        /// Runs when the event handler is called during the removal of the data source provider.
        /// </summary>
        /// <returns>An object of type Response, which frequently contains a message.</returns>
        public sealed override Response Execute()
        {
            Exception thrownException = null;

            try
            {
                OnRaisePreUninstallPreExecuteEvent();
                UninstallProviderResponse uninstallProviderResponse = UninstallSourceProvider().GetAwaiter().GetResult();

                return new Response
                {
                    Success = uninstallProviderResponse.Success,
                    Message = uninstallProviderResponse.Success ? _SUCCESS_MESSAGE : uninstallProviderResponse.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                thrownException = ex;
                return new Response
                {
                    Success = false,
                    Message = _FAILURE_MESSAGE,
                    Exception = ex
                };
            }
            finally
            {
                bool isSuccess = thrownException == null;
                OnRaisePreUninstallPostExecuteEvent(isSuccess, thrownException);
            }
        }

        private async Task<UninstallProviderResponse> UninstallSourceProvider()
        {
            var request = new UninstallProviderRequest
            {
                ApplicationID = ApplicationArtifactId,
                WorkspaceID = Helper.GetActiveCaseID()
            };

            return await SendUninstallProviderRequestWithRetriesAsync(request).ConfigureAwait(false);
        }

        /// <summary>
        /// We cannot use Polly, because it would require adding external dependency to our SDK
        /// </summary>
        private Task<UninstallProviderResponse> SendUninstallProviderRequestWithRetriesAsync(UninstallProviderRequest request, int attemptNumber = 1)
        {
            IServicesMgr servicesManager = Helper.GetServicesManager();
            var retryHelper = new KeplerRequestHelper(Logger, servicesManager, _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER, _SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS);

            return retryHelper
                .ExecuteWithRetriesAsync<IProviderManager, UninstallProviderRequest, UninstallProviderResponse>(
                    (providerManager, r) => providerManager.UninstallProviderAsync(r),
                    request
                 );
        }

        /// <summary>
        /// Raises the RaisePreUninstallPreExecuteEvent.
        /// </summary>
        protected void OnRaisePreUninstallPreExecuteEvent()
        {
            RaisePreUninstallPreExecuteEvent?.Invoke();
        }

        /// <summary>
        /// Raises the RaisePreUninstallPostExecuteEvent.
        /// </summary>
        protected void OnRaisePreUninstallPostExecuteEvent(bool isUninstalled, Exception ex)
        {
            RaisePreUninstallPostExecuteEvent?.Invoke(isUninstalled, ex);
        }
    }
}
