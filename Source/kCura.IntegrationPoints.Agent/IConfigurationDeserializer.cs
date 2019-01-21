namespace kCura.IntegrationPoints.Agent
{
	internal interface IConfigurationDeserializer
	{
		T DeserializeConfiguration<T>(string configurationJson);
	}
}