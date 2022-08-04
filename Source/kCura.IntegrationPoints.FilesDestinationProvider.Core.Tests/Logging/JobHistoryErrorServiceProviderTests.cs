using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
    [TestFixture, Category("Unit")]
    public class JobHistoryErrorServiceProviderTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
            
        }

        [Test]
        public void ItShouldReturnSameInstanceEveryTime()
        {
            var jobHistoryErrorService = new JobHistoryErrorService(
                Substitute.For<ICaseServiceContext>(), 
                Substitute.For<IHelper>(), 
                Substitute.For<IIntegrationPointRepository>()
            );

            var jobHistoryErrorServiceProvider = new JobHistoryErrorServiceProvider(jobHistoryErrorService);

            Assert.AreSame(jobHistoryErrorService, jobHistoryErrorServiceProvider.JobHistoryErrorService);
            Assert.AreSame(jobHistoryErrorService, jobHistoryErrorServiceProvider.JobHistoryErrorService);
        }
    }
}