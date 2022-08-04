using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    [TestFixture, Category("Unit")]
    [Ignore("TODO: Broken test needs to be fixed!")]
    public class ExportProcessRunnerTests : TestBase
    {
        private IExportProcessBuilder _exportProcessBuilder;
        private IExportSettingsBuilder _exportSettingsBuilder;
        private ExportProcessRunner _exportProcessRunner;

        [SetUp]
        public override void SetUp()
        {
            _exportProcessBuilder = Substitute.For<IExportProcessBuilder>();
            _exportSettingsBuilder = Substitute.For<IExportSettingsBuilder>();
            var helper = Substitute.For<IHelper>();
            _exportProcessRunner = new ExportProcessRunner(_exportProcessBuilder, _exportSettingsBuilder, helper);
        }

        [Test]
        public void ItShouldCreateExporterBasedOnSettings()
        {
            var settings = new ExportSettings();

            var job = JobExtensions.CreateJob();

            _exportProcessRunner.StartWith(settings, job);

            _exportProcessBuilder.Received().Create(settings, job);
        }

        [Test]
        public void ItShouldBuildExportSettingsBasedOnInputData()
        {
            var settings = new ExportUsingSavedSearchSettings();
            var fieldMap = new List<FieldMap>();
            var artifactId = 1000;
            var job = JobExtensions.CreateJob();

            _exportProcessRunner.StartWith(settings, fieldMap, artifactId, job);

            _exportSettingsBuilder.Received().Create(settings, fieldMap, artifactId);
            _exportProcessBuilder.Received().Create(Arg.Any<ExportSettings>(), job);
        }

        [Test]
        public void ItShouldRunExporter()
        {
            var exporter = Substitute.For<IExporter>();
            var job = JobExtensions.CreateJob();
            _exportProcessBuilder.Create(new ExportSettings(), job).ReturnsForAnyArgs(exporter);

            _exportProcessRunner.StartWith(new ExportSettings(), job);

            exporter.Received().ExportSearch();
        }

        [Test]
        public void ItShouldRunExporterForInputData()
        {
            var exporter = Substitute.For<IExporter>();
            var job = JobExtensions.CreateJob();
            _exportProcessBuilder.Create(new ExportSettings(), job).ReturnsForAnyArgs(exporter);

            _exportProcessRunner.StartWith(new ExportUsingSavedSearchSettings(), new List<FieldMap>(), 1000, job);

            exporter.Received().ExportSearch();
        }
    }
}