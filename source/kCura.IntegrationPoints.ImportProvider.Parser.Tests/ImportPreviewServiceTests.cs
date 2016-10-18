using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("ImportProvider")]
    public class ImportPreviewServiceTests
    {
        private IWinEddsLoadFileFactory _loadFileFactory;
        private IPreviewJob _previewJob;
        private IPreviewJobFactory _previewJobFactory;

        [SetUp]
        public void Setup()
        {
            _loadFileFactory = NSubstitute.Substitute.For<IWinEddsLoadFileFactory>();
            _previewJob = NSubstitute.Substitute.For<IPreviewJob>();
            _previewJobFactory = NSubstitute.Substitute.For<IPreviewJobFactory>();
        }

        [Test]
        public void StartPreviewThrowsWhenNoJob()
        {
            ImportPreviewService ips = new ImportPreviewService(_previewJobFactory);
            int badJobId = 1000;

            Assert.Throws<KeyNotFoundException>(() => ips.StartPreviewJob(badJobId));
        }

        [Test]
        public void RetrieveTableThrowsWhenNoJob()
        {
            ImportPreviewService ips = new ImportPreviewService(_previewJobFactory);
            int badJobId = 1000;

            Assert.Throws<KeyNotFoundException>(() => ips.RetrievePreviewTable(badJobId));
        }

        [Test]
        public void CheckProgressThrowsWhenNoJob()
        {
            ImportPreviewService ips = new ImportPreviewService(_previewJobFactory);
            int badJobId = 1000;

            Assert.Throws<KeyNotFoundException>(() => ips.CheckProgress(badJobId));
        }

        [Test]
        public void CreateJobServiceReturnsJobId()
        {
            ImportPreviewService ips = new ImportPreviewService(_previewJobFactory);
            
            int jobId = ips.CreatePreviewJob(new ImportPreviewSettings());

            Assert.AreEqual(1, jobId);
        }
    }
}
