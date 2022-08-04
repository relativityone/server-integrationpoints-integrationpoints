using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Removes all secrets with associated RIP from deleted workspace")]
    [RunOnce(true)]
    [Guid("725134DF-AA9A-4AAE-80FF-4CC047AF8C15")]
    public class SecretStorePreWorkspaceDeleteEventHandler : PreWorkspaceDeleteEventHandlerBase
    {
        private const string _SUCCESS_MESSAGE = "Secret Store successfully cleaned up.";
        private const string _FAILED_MESSAGE = "Failed to clean up Secret Store.";

        public override Response Execute()
        {
            IAPILog logger = Helper.GetLoggerFactory().GetLogger();
            int workspaceID = Helper.GetActiveCaseID();
            var secretsRepository = new SecretsRepository(
                SecretStoreFacadeFactory_Deprecated.Create(Helper.GetSecretStore, logger),
                logger
            );

            try
            {
                secretsRepository
                    .DeleteAllRipSecretsFromWorkspaceAsync(workspaceID)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                logger.ForContext<SecretStorePreWorkspaceDeleteEventHandler>()
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