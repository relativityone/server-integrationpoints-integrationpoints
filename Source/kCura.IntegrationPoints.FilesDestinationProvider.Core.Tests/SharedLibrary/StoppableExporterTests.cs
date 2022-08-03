using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using NSubstitute;
using NUnit.Framework;
using Relativity.DataExchange.Process;
using Relativity.Telemetry.MetricsCollection;
using IExporter = kCura.WinEDDS.IExporter;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
    [TestFixture, Category("Unit")]
    public class StoppableExporterTests : TestBase
    {
        private ProcessContext _context;
        private IExporter _exporter;
        private IJobStopManager _jobStopManager;
        private StoppableExporter _stoppableExporter;

        [SetUp]
        public override void SetUp()
        {
            Client.LazyMetricsClient = new Lazy<IMetricsCollector>(() => Substitute.For<IMetricsCollector>());
            _exporter = Substitute.For<IExporter>();
            _context = new ProcessContext();
            _jobStopManager = Substitute.For<IJobStopManager>();
            _stoppableExporter = new StoppableExporter(_exporter, _context, _jobStopManager);
        }

        [Test]
        public void ItShouldRunExporter()
        {
            _stoppableExporter.ExportSearch();

            _exporter.Received().ExportSearch();
        }

        [Test]
        public void ItShouldCheckForOperationCanceledException()
        {
            _stoppableExporter.ExportSearch();

            _jobStopManager.Received().ThrowIfStopRequested();
        }

        [Test]
        public void ItShouldHaltProcessOnStopRequested()
        {
            var wasCalled = false;
            _context.CancellationRequest += (sender, args) => wasCalled = true;

            _exporter.When(x => x.ExportSearch()).Do(info => _jobStopManager.StopRequestedEvent += Raise.Event<EventHandler<EventArgs>>(EventArgs.Empty));

            _stoppableExporter.ExportSearch();

            Assert.True(wasCalled);
        }

        [Test]
        public void ItShouldNotHaltProcessAfterJobCompletion()
        {
            var wasCalled = false;
            _context.CancellationRequest += (sender, args) => wasCalled = true;

            _stoppableExporter.ExportSearch();

            _jobStopManager.StopRequestedEvent += Raise.Event<EventHandler<EventArgs>>(EventArgs.Empty);

            Assert.False(wasCalled);
        }

        [Test]
        public void ItShouldRaiseEventOnCompletion()
        {
            const int expectedDocumentsCount = 10;

            _exporter.DocumentsExported.Returns(expectedDocumentsCount);

            var actualDocumentsCount = 0;
            var actualErrors = 0;

            _stoppableExporter.OnBatchCompleted += (time, endTime, rows, count) =>
            {
                actualDocumentsCount = rows;
                actualErrors = count;
            };

            _stoppableExporter.ExportSearch();

            Assert.AreEqual(expectedDocumentsCount, actualDocumentsCount);
            Assert.AreEqual(0, actualErrors);
        }
    }
}