using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Windows.Process;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
	public class StoppableExporterTests
	{
		private Controller _controller;
		private IExporter _exporter;
		private IJobStopManager _jobStopManager;
		private StoppableExporter _stoppableExporter;

		[SetUp]
		public void SetUp()
		{
			_exporter = Substitute.For<IExporter>();
			_controller = new Controller();
			_jobStopManager = Substitute.For<IJobStopManager>();
			_stoppableExporter = new StoppableExporter(_exporter, _controller, _jobStopManager);
		}

		[Test]
		public void ItShouldRunExporter()
		{
			_stoppableExporter.Run();

			_exporter.Received().Run();
		}

		[Test]
		public void ItShouldCheckForOperationCanceledException()
		{
			_stoppableExporter.Run();

			_jobStopManager.Received().ThrowIfStopRequested();
		}

		[Test]
		public void ItShouldHaltProcessOnStopRequested()
		{
			var wasCalled = false;
			_controller.HaltProcessEvent += id => wasCalled = true;

			_exporter.When(x => x.Run()).Do(info => _jobStopManager.StopRequestedEvent += Raise.Event<EventHandler<EventArgs>>(EventArgs.Empty));

			_stoppableExporter.Run();

			Assert.True(wasCalled);
		}
	}
}