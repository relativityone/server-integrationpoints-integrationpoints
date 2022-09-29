using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
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
        [Test]
        public void ItShouldReturnSameInstanceEveryTime()
        {
            JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(
                Substitute.For<IRelativityObjectManager>(),
                Substitute.For<IHelper>(),
                Substitute.For<IIntegrationPointRepository>()
            );

            JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider = new JobHistoryErrorServiceProvider(jobHistoryErrorService);

            Assert.AreSame(jobHistoryErrorService, jobHistoryErrorServiceProvider.JobHistoryErrorService);
            Assert.AreSame(jobHistoryErrorService, jobHistoryErrorServiceProvider.JobHistoryErrorService);
        }
    }
}