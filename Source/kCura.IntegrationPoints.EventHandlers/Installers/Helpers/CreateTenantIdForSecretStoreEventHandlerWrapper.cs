using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Helpers
{
	public class CreateTenantIdForSecretStoreEventHandlerWrapper
	{
		internal IEHHelper Helper;
		private ICreateTenantIdForSecretStore _createTenantIdForSecretStore;

		internal ICreateTenantIdForSecretStore CreateTenantIdForSecretStore
		{
			get { return _createTenantIdForSecretStore ?? (_createTenantIdForSecretStore = CreateTenantIdForSecretStoreFactory.Create(Helper.GetActiveCaseID())); }
			set { _createTenantIdForSecretStore = value; }
		}


		public Response Execute()
		{
			var response = new Response
			{
				Success = true,
				Message = "SecretStore successfully initialized."
			};
			try
			{
				CreateTenantIdForSecretStore.Create();
				ValidateTenantCreation();
			}
			catch (Exception e)
			{
				LogError(e);
				response.Success = false;
				response.Message = "Failed to initialize SecretStore.";
				response.Exception = e;
			}
			return response;
		}

		private void ValidateTenantCreation()
		{
			//TODO hack for now, as ISecretCatalog doesn't return any information about errors during tenant creation
			var secretManager = new SecretManager(Helper.GetActiveCaseID());
			var tenantExists =
				Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>($"SELECT COUNT(*) FROM [SQLSecretStore] WHERE [TenantID] = '{secretManager.GetTenantID()}'") > 0;
			if (!tenantExists)
			{
				throw new Exception("Failed to validate tenant creation. Assuming that SecretStore was not initialized.");
			}
		}

		private void LogError(Exception e)
		{
			var logger = Helper.GetLoggerFactory().GetLogger().ForContext<CreateTenantIdForSecretStoreEventHandlerWrapper>();
			logger.LogError(e, "Failed to create TenantID in SecretStore.");
		}
	}
}