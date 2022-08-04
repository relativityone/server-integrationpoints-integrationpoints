using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Removes all secrets associated with RIP")]
    [RunOnce(true)]
    [Guid("CFB90708-63CB-4C9C-A0AE-BBCFCDE1E07E")]
    public class SecretStoreUninstaller : PreUninstallEventHandler
    {
        private const string _SUCCESS_MESSAGE = "Secret Store successfully cleaned up.";
        private const string _FAILED_MESSAGE = "Failed to clean up Secret Store.";

        public override Response Execute()
        {
            IAPILog logger = Helper.GetLoggerFactory().GetLogger();
            var secretsRepository = new SecretsRepository(
                SecretStoreFacadeFactory_Deprecated.Create(Helper.GetSecretStore, logger),
                logger
            );

            try
            {
                secretsRepository
                    .DeleteAllRipSecretsFromAllWorkspacesAsync()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                logger.ForContext<SecretStoreUninstaller>()
                    .LogError(e, _FAILED_MESSAGE);
                return new Response
                {
                    Success = false,
                    Message = _FAILED_MESSAGE,
                    Exception = e
                };
            }
            return new Response
            {
                Success = true,
                Message = _SUCCESS_MESSAGE
            };
        }
    }
}