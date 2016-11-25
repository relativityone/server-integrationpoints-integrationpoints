using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	public interface IRelativityProviderConfiguration
	{
		void UpdateNames(IDictionary<string, object> settings);
	}
}