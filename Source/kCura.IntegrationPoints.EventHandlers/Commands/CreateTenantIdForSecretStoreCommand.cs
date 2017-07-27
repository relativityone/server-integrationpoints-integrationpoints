using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class CreateTenantIdForSecretStoreCommand : IEHCommand
	{
		private readonly ICreateTenantIdForSecretStore _createTenantIdForSecretStore;
		private readonly ITenantForSecretStoreCreationValidator _tenantForSecretStoreCreationValidator;

		public CreateTenantIdForSecretStoreCommand(ICreateTenantIdForSecretStore createTenantIdForSecretStore,
			ITenantForSecretStoreCreationValidator tenantForSecretStoreCreationValidator)
		{
			_createTenantIdForSecretStore = createTenantIdForSecretStore;
			_tenantForSecretStoreCreationValidator = tenantForSecretStoreCreationValidator;
		}

		public void Execute()
		{
			_createTenantIdForSecretStore.Create();

			//TODO hack for now, as ISecretCatalog doesn't return any information about errors during tenant creation
			if (!_tenantForSecretStoreCreationValidator.Validate())
			{
				throw new CommandExecutionException("Failed to validate tenant creation. Assuming that SecretStore was not initialized.");
			}
		}
	}
}