using System;
using System.Collections;
using kCura.Config;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations
{
	public class ConfigAdapter : IIntegrationPointsConfig
	{
		public string GetWebApiUrl
		{
			// TODO: This is NOT an acceptable solution. We must look into using kCura.Config -- biedrzycki: Feb 16th, 2016
			get
			{
				const string configSection = "kCura.IntegrationPoints";
				const string webApiPath = "WebAPIPath";

				IDictionary config = Manager.GetConfig(configSection);
				if (config.Contains(webApiPath))
				{
					return config[webApiPath] as string;
				}
				throw new ConfigurationException(String.Format("Unable to find [{0}:{1}] in Relativity's instance settings.", configSection, webApiPath));
			}
		}
	}
}