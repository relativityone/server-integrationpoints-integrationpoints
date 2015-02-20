using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Config;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class Config
	{
		private static IDictionary _underlyingSetting;
		protected static IDictionary ConfigSettings
		{
			get { return _underlyingSetting ?? (_underlyingSetting = Manager.Instance.GetConfig("kCura.Relativity.IntegrationPoints")); }
		}

		public static string WebAPIPath
		{
			get { return ConfigHelper.GetValue(ConfigSettings["WebAPIPath"], string.Empty); }
		}

	}
}
