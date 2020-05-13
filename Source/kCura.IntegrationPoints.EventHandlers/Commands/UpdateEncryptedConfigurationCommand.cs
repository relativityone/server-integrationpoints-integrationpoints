using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Security;
using Relativity.API;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public abstract class UpdateEncryptedConfigurationCommand : UpdateIntegrationPointConfigurationCommandBase
	{
		private const string _SECURED_CONFIGURATION = IntegrationPointFields.SecuredConfiguration;
		private const string _SOURCE_CONFIGURATION = IntegrationPointFields.SourceConfiguration;

		protected readonly IEncryptionManager EncryptionManager;
		protected readonly ISplitJsonObjectService SplitJsonObjectService;

		protected abstract string[] PropertiesToExtract { get; }

		protected override IList<string> FieldsNamesForUpdate => new List<string>
		{
			_SECURED_CONFIGURATION,
			_SOURCE_CONFIGURATION
		};

		protected UpdateEncryptedConfigurationCommand(IEHHelper helper, IRelativityObjectManager relativityObjectManager,
			IEncryptionManager encryptionManager, ISplitJsonObjectService splitJsonObjectService)
			: base(helper, relativityObjectManager)
		{
			EncryptionManager = encryptionManager;
			SplitJsonObjectService = splitJsonObjectService;
		}

		protected override RelativityObjectSlimDto UpdateFields(RelativityObjectSlimDto value)
		{
			//if integrationPoint.SecuredConfiguration is not empty, it means upgrade was made or is not necessary
			string securedConfiguration = value.FieldValues[_SECURED_CONFIGURATION] as string;
			if(!string.IsNullOrEmpty(securedConfiguration))
			{
				return null;
			}

			string sourceConfiguration = value.FieldValues[_SOURCE_CONFIGURATION] as string;
			string decryptedConfiguration = EncryptionManager.Decrypt(sourceConfiguration);
			
			SplittedJsonObject splittedConfiguration = SplitJsonObjectService.Split(decryptedConfiguration, PropertiesToExtract);
			if (splittedConfiguration == null)
			{
				return null;
			}

			value.FieldValues[_SOURCE_CONFIGURATION] = splittedConfiguration.JsonWithoutExtractedProperties;
			value.FieldValues[_SECURED_CONFIGURATION] = splittedConfiguration.JsonWithExtractedProperties;
			
			return value;
		}
	}
}