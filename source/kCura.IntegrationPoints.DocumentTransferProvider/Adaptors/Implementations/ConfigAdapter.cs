using System;
using System.Collections;
using kCura.Config;
using kCura.IntegrationPoints.DocumentTransferProvider.Shared;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations
{
	public class ConfigAdapter : IIntegrationPointsConfig
	{
		public string GetWebApiUrl
		{
			get
			{
				IDictionary config = Manager.GetConfig(Constants.CONFIG_SECTION);
				if (config.Contains(Constants.WEB_API_PATH))
				{
					return config[Constants.WEB_API_PATH] as string;
				}
				throw new ConfigurationException(String.Format("Unable to find [{0}:{1}] in Relativity's instance settings.", Constants.CONFIG_SECTION, Constants.WEB_API_PATH));
			}
		}
	}
}