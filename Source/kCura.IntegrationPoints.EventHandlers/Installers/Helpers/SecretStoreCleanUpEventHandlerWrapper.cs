using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Helpers
{
	public class SecretStoreCleanUpEventHandlerWrapper
	{
		internal IEHHelper Helper;
		private ISecretStoreCleanUp _secretStoreCleanUp;
		private const string _SUCCESS_MESSAGE = "Secret Store successfully cleaned up.";
		private const string _FAILED_MESSAGE = "Failed to clean up Secret Store.";

		internal ISecretStoreCleanUp SecretStoreCleanUp
		{
			get { return _secretStoreCleanUp ?? (_secretStoreCleanUp = SecretStoreCleanUpFactory.Create(Helper.GetActiveCaseID())); }
			set { _secretStoreCleanUp = value; }
		}

		public Response Execute()
		{
			var response = new Response
			{
				Success = true,
				Message = _SUCCESS_MESSAGE
			};
			try
			{
				SecretStoreCleanUp.CleanUpSecretStore();
			}
			catch (Exception e)
			{
				LogSecretStoreCleanUpError(e);
				response.Success = false;
				response.Message = _FAILED_MESSAGE;
				response.Exception = e;
			}
			return response;
		}

		private void LogSecretStoreCleanUpError(Exception e)
		{
			var logger = Helper.GetLoggerFactory().GetLogger().ForContext<SecretStoreCleanUpEventHandlerWrapper>();
			logger.LogError(e, _FAILED_MESSAGE);
		}
	}
}