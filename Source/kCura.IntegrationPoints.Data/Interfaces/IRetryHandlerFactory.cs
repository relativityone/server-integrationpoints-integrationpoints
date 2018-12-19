namespace kCura.IntegrationPoints.Data.Interfaces
{
	internal interface IRetryHandlerFactory
	{
		IRetryHandler Create(ushort maxNumberOfRetries = 3, ushort exponentialWaitTimeBaseInSeconds = 3);
	}
}
