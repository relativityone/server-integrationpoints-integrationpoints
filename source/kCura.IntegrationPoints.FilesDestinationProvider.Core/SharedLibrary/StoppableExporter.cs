using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.Windows.Process;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class StoppableExporter : ExporterEventsWrapper
	{
		private readonly Controller _controller;
		private readonly IExporter _exporter;
		private readonly IJobStopManager _jobStopManager;

		public StoppableExporter(IExporter exporter, Controller controller, IJobStopManager jobStopManager) : base(exporter)
		{
			_exporter = exporter;
			_controller = controller;
			_jobStopManager = jobStopManager;
		}

		public override IUserNotification InteractionManager
		{
			get { return _exporter.InteractionManager; }
			set { _exporter.InteractionManager = value; }
		}

		public override void Run()
		{
			try
			{
				_jobStopManager.StopRequestedEvent += OnStopRequested;
				_exporter.Run();
				_jobStopManager.ThrowIfStopRequested();
			}
			finally
			{
				_jobStopManager.StopRequestedEvent -= OnStopRequested;
			}
		}

		private void OnStopRequested(object sender, EventArgs eventArgs)
		{
			_controller.HaltProcess(Guid.Empty);
		}
	}
}