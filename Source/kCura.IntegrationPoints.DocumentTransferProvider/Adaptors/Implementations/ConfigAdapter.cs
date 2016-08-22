using System;
using System.Collections;
using System.Configuration;
using kCura.Config;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations
{
	public class ConfigAdapter : IIntegrationPointsConfig
	{
		public string GetWebApiUrl
		{
			get
			{
				IDictionary config = Manager.GetConfig(Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION);
				if (config.Contains(Domain.Constants.WEB_API_PATH))
				{
					return config[Domain.Constants.WEB_API_PATH] as string;
				}
				throw new ConfigurationErrorsException(String.Format("Unable to find [{0}:{1}] in Relativity's instance settings.",
					Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION, Domain.Constants.WEB_API_PATH));
			}
		}
	}
}