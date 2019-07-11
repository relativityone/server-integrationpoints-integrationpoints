using kCura.IntegrationPoints.Common.Handlers;

namespace kCura.IntegrationPoints.Data.Interfaces
{
	public interface IRetryHandlerFactory
	{
		IRetryHandler Create(ushort maxNumberOfRetries = 3, ushort exponentialWaitTimeBaseInSeconds = 3);
	}
}
