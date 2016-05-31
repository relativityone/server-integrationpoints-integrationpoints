using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    public class ExportProcessRunnerTests
    {
        private IExportProcessBuilder _exportProcessBuilder;
        private ExportProcessRunner _exportProcessRunner;

        [SetUp]
        public void SetUp()
        {
            _exportProcessBuilder = Substitute.For<IExportProcessBuilder>();
            _exportProcessRunner = new ExportProcessRunner(_exportProcessBuilder);
        }

        [Test]
        public void ItShouldCreateExporterBasedOnSettings()
        {
            var settings = new ExportSettings();

            _exportProcessRunner.StartWith(settings);

            _exportProcessBuilder.Received().Create(settings);
        }

        [Test]
        public void ItShouldRunExporter()
        {
            var exporter = Substitute.For<IExporter>();
            _exportProcessBuilder.Create(new ExportSettings()).ReturnsForAnyArgs(exporter);
            _exportProcessRunner.StartWith(new ExportSettings());

            exporter.Received().Run();
        }
    }
}