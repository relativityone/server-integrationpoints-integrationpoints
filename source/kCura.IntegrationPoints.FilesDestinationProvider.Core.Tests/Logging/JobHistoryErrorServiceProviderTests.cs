using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
    public class JobHistoryErrorServiceProviderTests
    {
        [Test]
        public void ItShouldReturnSameInstanceEveryTime()
        {
            var jobHistoryErrorService = new JobHistoryErrorService(Substitute.For<ICaseServiceContext>());

            var jobHistoryErrorServiceProvider = new JobHistoryErrorServiceProvider(jobHistoryErrorService);

            Assert.AreSame(jobHistoryErrorService, jobHistoryErrorServiceProvider.JobHistoryErrorService);
            Assert.AreSame(jobHistoryErrorService, jobHistoryErrorServiceProvider.JobHistoryErrorService);
        }
    }
}