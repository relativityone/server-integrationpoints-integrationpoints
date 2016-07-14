using System.Collections;
using kCura.Config;


namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class InstanceSettingRepository : IInstanceSettingRepository
	{
		public string GetConfigurationValue(string section, string name)
		{
			Manager.ClearCache();

			string configValue = null;
			IDictionary config = Manager.GetConfig(section);
			if (config.Contains(name))
			{
				configValue = config[name] as string;
			}

			return configValue;
		}
	}
}
