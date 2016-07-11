using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public interface ILoggingMediator
	{
		void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
			IExporterStatusNotification exporterStatusNotification);
	}
}