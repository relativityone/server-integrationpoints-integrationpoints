using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Security;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateFtpConfigurationCommand : UpdateEncryptedConfigurationCommand
	{
		protected override string SourceProviderGuid => Constants.IntegrationPoints.SourceProviders.FTP;

		protected override string[] PropertiesToExtract { get; } = { "username", "password" };

		public UpdateFtpConfigurationCommand(IEHHelper helper, IRelativityObjectManager relativityObjectManager,
			IEncryptionManager encryptionManager, ISplitJsonObjectService splitJsonObjectService)
			: base(helper, relativityObjectManager, encryptionManager, splitJsonObjectService)
		{
		}
	}
}