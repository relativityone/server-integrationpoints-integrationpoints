using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Installers;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class UpdateJobHistoryDestinationWorkspaceTests : TestBase
    {
        private IJobHistoryService _jobHistoryService;
        private IDestinationParser _destinationParser;

        private UpdateJobHistoryDestinationWorkspace _testInstance;

        [Test]
        public void TestUpgrade()
        {
            string destinationWorkspace1 = "Workspace1 - 123";
            string destinationWorkspace2 = "This Instance - Workspace2 - 456";

            Data.JobHistory jobHistory1 = new Data.JobHistory() { DestinationWorkspace = destinationWorkspace1 };
            Data.JobHistory jobHistory2 = new Data.JobHistory() { DestinationWorkspace = destinationWorkspace2 };

            _jobHistoryService.GetAll().Returns(new Data.JobHistory[] { jobHistory1, jobHistory2});

            _destinationParser.GetElements(destinationWorkspace1).Returns(destinationWorkspace1.Split('-'));
            _destinationParser.GetElements(destinationWorkspace2).Returns(destinationWorkspace2.Split('-'));

            _testInstance.ExecuteInternal();

            _jobHistoryService.Received(1).UpdateRdo(Arg.Is<Data.JobHistory>(j => j.DestinationWorkspace.Equals("This Instance - " + destinationWorkspace1)));
        }

        public override void SetUp()
        {
            _jobHistoryService = Substitute.For<IJobHistoryService>();
            _destinationParser = Substitute.For<IDestinationParser>();

            _testInstance = new UpdateJobHistoryDestinationWorkspace(_jobHistoryService, _destinationParser);
        }
    }
}
