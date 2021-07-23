using System.Collections;
using System.Collections.Generic;
using kCura.IntegrationPoints.Config;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeInstanceSettingsProvider : IInstanceSettingsProvider
	{
		public IDictionary GetInstanceSettings()
		{
			return new Dictionary<string, string>();
		}

		public T GetValue<T>(object input)
		{
			return default(T);
		}

		public T GetValue<T>(object input, T defaultValue)
		{
			return defaultValue;
		}
	}
}