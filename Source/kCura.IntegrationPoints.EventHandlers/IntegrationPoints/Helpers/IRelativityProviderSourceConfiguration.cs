using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	public interface IRelativityProviderSourceConfiguration
	{
		void UpdateNames(IDictionary<string, object> settings);
	}
}