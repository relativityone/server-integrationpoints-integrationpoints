using System;
using System.Collections;
using kCura.Config;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations
{
	public class ConfigAdapter : IIntegrationPointsConfig
	{
		public string GetWebApiUrl
		{
			get
			{
				IDictionary config = Manager.GetConfig(kCura.IntegrationPoints.Contracts.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION);
				if (config.Contains(kCura.IntegrationPoints.Contracts.Constants.WEB_API_PATH))
				{
					return config[kCura.IntegrationPoints.Contracts.Constants.WEB_API_PATH] as string;
				}
				throw new ConfigurationException(String.Format("Unable to find [{0}:{1}] in Relativity's instance settings.",
					kCura.IntegrationPoints.Contracts.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION, kCura.IntegrationPoints.Contracts.Constants.WEB_API_PATH));
			}
		}
	}
}