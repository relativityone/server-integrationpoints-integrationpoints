using System.Threading.Tasks;
using kCura.IntegrationPoints.Config;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.Data.Toggle
{
	public interface IExtendedRelativityToggle : IToggleProvider
	{
		/// <summary>
		/// Relativity Configuration
		/// </summary>
		IConfig Configuration { get; set; }

		/// <summary>
		/// Determine whether the AOAG feature is enabled
		/// </summary>
		/// <returns></returns>
		bool IsAOAGFeatureEnabled();
	}

	public class ExtendedRelativityToggle : IExtendedRelativityToggle
	{
		private IConfig _config;

		private SqlServerToggleProvider Provider { get; }

		public IConfig Configuration { get { return _config ?? Config.Config.Instance; } set { _config = value; } }

		public ExtendedRelativityToggle(SqlServerToggleProvider toggleProvider)
		{
			Provider = toggleProvider;
		}

		public bool IsAOAGFeatureEnabled()
		{
			// Do not check on the toggle of this feature !!! - SAMO 5/31/2016.
			// the default value of the toggle itself is off, which will make the check be invalid when it gets removed.
			if ((Configuration.IsCloudInstance || !Configuration.UseEDDSResource))
			{
				return true;
			};
			return false;
		}

		public bool IsEnabled<T>() where T : IToggle
		{
			return Provider.IsEnabled<T>();
		}

		public async Task<bool> IsEnabledAsync<T>() where T : IToggle
		{
			return await Provider.IsEnabledAsync<T>();
		}

		public bool IsEnabledByName(string toggleName)
		{
			return Provider.IsEnabledByName(toggleName);
		}

		public async Task<bool> IsEnabledByNameAsync(string toggleName)
		{
			return await Provider.IsEnabledByNameAsync(toggleName);
		}

		public async Task SetAsync<T>(bool enabled) where T : IToggle
		{
			await Provider.SetAsync<T>(enabled);
		}

		public MissingFeatureBehavior DefaultMissingFeatureBehavior => Provider.DefaultMissingFeatureBehavior;

		public bool CacheEnabled { get { return Provider.CacheEnabled; } set { Provider.CacheEnabled = value; } }
		public int CacheTimeoutInSeconds { get { return Provider.CacheTimeoutInSeconds; } set { Provider.CacheTimeoutInSeconds = value; } }
	}
}