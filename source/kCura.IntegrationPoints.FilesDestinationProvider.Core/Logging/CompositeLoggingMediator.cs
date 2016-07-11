using System.Collections.Generic;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public class CompositeLoggingMediator : ILoggingMediator
	{
		private readonly List<ILoggingMediator> _loggingMediators;

		public CompositeLoggingMediator()
		{
			_loggingMediators = new List<ILoggingMediator>();
		}

		public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
			IExporterStatusNotification exporterStatusNotification)
		{
			foreach (var loggingMediator in _loggingMediators)
			{
				loggingMediator.RegisterEventHandlers(userMessageNotification, exporterStatusNotification);
			}
		}

		public void AddLoggingMediator(ILoggingMediator loggingMediator)
		{
			_loggingMediators.Add(loggingMediator);
		}
	}
}