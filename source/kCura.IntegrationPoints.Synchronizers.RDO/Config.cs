using System;
using System.Collections;
using kCura.Apps.Common.Config;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class Config : IConfig
	{
		private static readonly Lazy<Config> _instance = new Lazy<Config>(() => new Config());
		private static IDictionary _dictionary;

		public static Config Instance
		{
			get { return _instance.Value; }
		}

		protected Config() :
			this(Manager.Instance.GetConfig("kCura.IntegrationPoints"))
		{ }

		internal Config(IDictionary dictionary)
		{
			_dictionary = dictionary;
		}

		public string WebApiPath
		{
			get { return ConfigHelper.GetValue(_dictionary["WebAPIPath"], string.Empty); }
		}

		public bool DisableNativeLocationValidation
		{
			get { return ConfigHelper.GetValue<bool>(_dictionary["DisableNativeLocationValidation"], false); }
		}

		public bool DisableNativeValidation
		{
			get { return ConfigHelper.GetValue<bool>(_dictionary["DisableNativeValidation"], false); }
		}
	}
}
