using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    public class ExportProcessRunnerTests
    {
        private IExportProcessBuilder _exportProcessBuilder;
	    private IExportSettingsBuilder _exportSettingsBuilder;
        private ExportProcessRunner _exportProcessRunner;

        [SetUp]
        public void SetUp()
        {
            _exportProcessBuilder = Substitute.For<IExportProcessBuilder>();
	        _exportSettingsBuilder = Substitute.For<IExportSettingsBuilder>();
            _exportProcessRunner = new ExportProcessRunner(_exportProcessBuilder, _exportSettingsBuilder);
        }

        [Test]
        public void ItShouldCreateExporterBasedOnSettings()
        {
            var settings = new ExportSettings();

            _exportProcessRunner.StartWith(settings);

            _exportProcessBuilder.Received().Create(settings);
        }

		[Test]
		public void ItShouldBuildExportSettingsBasedOnInputData()
		{
			var settings = new ExportUsingSavedSearchSettings();
			var fieldMap = new List<FieldMap>();
			var artifactId = 1000;

			_exportProcessRunner.StartWith(settings, fieldMap, artifactId);

			_exportSettingsBuilder.Received().Create(settings, fieldMap, artifactId);
			_exportProcessBuilder.Received().Create(Arg.Any<ExportSettings>());
		}

		[Test]
        public void ItShouldRunExporter()
        {
            var exporter = Substitute.For<IExporter>();
            _exportProcessBuilder.Create(new ExportSettings()).ReturnsForAnyArgs(exporter);

            _exportProcessRunner.StartWith(new ExportSettings());

            exporter.Received().Run();
        }

		[Test]
		public void ItShouldRunExporterForInputData()
		{
			var exporter = Substitute.For<IExporter>();
			_exportProcessBuilder.Create(new ExportSettings()).ReturnsForAnyArgs(exporter);

			_exportProcessRunner.StartWith(new ExportUsingSavedSearchSettings(), new List<FieldMap>(), 1000);

			exporter.Received().Run();
		}
	}
}