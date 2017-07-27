using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Helpers
{
	public class TenantForSecretStoreCreationValidator : ITenantForSecretStoreCreationValidator
	{
		private readonly IEHContext _context;
		private readonly ISecretManagerFactory _secretManagerFactory;
		private readonly ISecretCatalogFactory _secretCatalogFactory;

		public TenantForSecretStoreCreationValidator(IEHContext context, ISecretManagerFactory secretManagerFactory, ISecretCatalogFactory secretCatalogFactory)
		{
			_context = context;
			_secretManagerFactory = secretManagerFactory;
			_secretCatalogFactory = secretCatalogFactory;
		}

		public bool Validate()
		{
			var secretManager = _secretManagerFactory.Create(_context.Helper.GetActiveCaseID());
			var secretCatalog = _secretCatalogFactory.Create(_context.Helper.GetActiveCaseID());

			var secretIdentifier = secretManager.GenerateIdentifier();
			try
			{
				if (!secretCatalog.WriteSecret(secretIdentifier, new Dictionary<string, string> {{"test", "integration_point"}}))
				{
					return false;
				}
			}
			catch (Exception e)
			{
				_context.Helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to validate tenant ID creation.");
				return false;
			}

			try
			{
				secretCatalog.RevokeSecret(secretIdentifier);
			}
			catch (Exception e)
			{
				_context.Helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to remove test secret during validation.");
				//ignore
			}
			return true;
		}
	}
}