using kCura.Config;
using Relativity.Data.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class RelativityToggleExtensions
	{
		public static bool IsFeatureEnabled<T>(this IToggleProvider provider) where T : AOAGToggle
		{
			if (provider.IsEnabled<AOAGToggle>() && Config.Config.Instance.IsCloudInstance && !Config.Config.Instance.UseEDDSResource)
			{
				return true;
			};
			return false;
		}
	}
}