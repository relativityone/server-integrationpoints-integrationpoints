namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation
{
	public interface IExternalServiceInstrumentationProvider
	{
		IExternalServiceInstrumentation Create(string serviceType, string serviceName, string operationName);
	}
}
