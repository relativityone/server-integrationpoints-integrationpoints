using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Agent
{
	internal class ConfigurationDeserializer : IConfigurationDeserializer
	{
		public T DeserializeConfiguration<T>(string configurationJson)
		{
			return JsonConvert.DeserializeObject<T>(configurationJson);
		}
	}
}