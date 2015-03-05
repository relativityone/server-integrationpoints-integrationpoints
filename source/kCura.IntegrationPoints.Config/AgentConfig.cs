using System.Collections;
using kCura.Apps.Common.Config;

namespace kCura.IntegrationPoints.Config
{
	public class AgentConfig
	{
		private static IDictionary _underlyingSetting;
		protected static IDictionary ConfigSettings
		{
			get { return _underlyingSetting ?? (_underlyingSetting = Manager.Instance.GetConfig("kCura.IntegrationPoints")); }
		}

		private const int BATCH_SIZE_DEFAULT = 1000;
		public static int BatchSize
		{
			get
			{
				var value = ConfigHelper.GetValue<int>(ConfigSettings["BatchSize"], BATCH_SIZE_DEFAULT);
				if (value <= 0)
				{
					value = BATCH_SIZE_DEFAULT;
				}
				return value;
			}
		}
	}
}
