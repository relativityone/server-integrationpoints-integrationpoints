using System.Collections;
using kCura.Apps.Common.Config;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class Config
	{
		private static IDictionary _underlyingSetting;
		protected static IDictionary ConfigSettings
		{
			get { return _underlyingSetting ?? (_underlyingSetting = Manager.Instance.GetConfig("kCura.IntegrationPoints")); }
		}

		public static string WebAPIPath
		{
			get { return ConfigHelper.GetValue(ConfigSettings["WebAPIPath"], string.Empty); }
		}
	}
}
