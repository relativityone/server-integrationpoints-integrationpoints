using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("Unit"), Category("ImportProvider")]
    public class ImportPreviewServiceTests : TestBase
    {
        private IPreviewJob _previewJob;
        private IPreviewJobFactory _previewJobFactory;

        [SetUp]
        public override void SetUp()
        {
            _previewJob = Substitute.For<IPreviewJob>();
            _previewJobFactory = Substitute.For<IPreviewJobFactory>();
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

        [Test]
        public void CheckProgressDisposesJobIfFailed()
        {
            //mock ImportJob to return with IsFailed as true
            _previewJob.IsFailed.ReturnsForAnyArgs(true);
            _previewJobFactory.GetPreviewJob(new ImportPreviewSettings()).ReturnsForAnyArgs(_previewJob);
            ImportPreviewService ips = new ImportPreviewService(_previewJobFactory);

            //Create Preview Job
            int jobId = ips.CreatePreviewJob(new ImportPreviewSettings());

            //Assert that the ID exists and IsJobComplete will not throw
            Assert.DoesNotThrow(() => ips.IsJobComplete(jobId));

            //Check for Progress, Assert that IsFailed is false.
            //becuase this method sees that IsFailed is true it will dispose of the job
            Assert.IsTrue(ips.CheckProgress(jobId).IsFailed);

            //Confirm that the job was disposed and no longer exists in dictionary
            Assert.Throws<KeyNotFoundException>(() => ips.IsJobComplete(jobId));
        }
    }
}
