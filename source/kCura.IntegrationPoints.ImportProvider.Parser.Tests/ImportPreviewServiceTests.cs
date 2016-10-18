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

        [SetUp]
        public void Setup()
        {
            _loadFileFactory = NSubstitute.Substitute.For<IWinEddsLoadFileFactory>();
        }

        [Test]
        public void StartPreviewThrowsWhenNoJob()
        {
            ImportPreviewService ips = new ImportPreviewService(_loadFileFactory);
            int badJobId = 1000;

            Assert.Throws<KeyNotFoundException>(() => ips.StartPreviewJob(badJobId));
        }

        [Test]
        public void RetrieveTableThrowsWhenNoJob()
        {
            ImportPreviewService ips = new ImportPreviewService(_loadFileFactory);
            int badJobId = 1000;

            Assert.Throws<KeyNotFoundException>(() => ips.RetrievePreviewTable(badJobId));
        }

        [Test]
        public void CheckProgressThrowsWhenNoJob()
        {
            ImportPreviewService ips = new ImportPreviewService(_loadFileFactory);
            int badJobId = 1000;

            Assert.Throws<KeyNotFoundException>(() => ips.CheckProgress(badJobId));
        }

    }
}
