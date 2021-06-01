using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    // public class FakeJobStatisticsService : IJobStatisticsService 
    // {
    //     private readonly RelativityInstanceTest _relativityInstanceTest;
    //     private ImportSettings _importSettings;
    //     private SourceConfiguration _sourceConfiguration;
    //
    //     public FakeJobStatisticsService(RelativityInstanceTest relativityInstanceTest)
    //     {
    //         _relativityInstanceTest = relativityInstanceTest;
    //     }
    //     
    //     public void Subscribe(IBatchReporter reporter, Job job)
    //     {
    //         reporter.
    //     }
    //
    //     public void SetIntegrationPointConfiguration(ImportSettings importSettings, SourceConfiguration sourceConfiguration)
    //     {
    //         _importSettings = importSettings;
    //         _sourceConfiguration = sourceConfiguration;
    //     }
    //
    //     public void Update(Guid identifier, int transferredItem, int erroredCount)
    //     {
    //         var workspace =
    //             _relativityInstanceTest.Workspaces.First(x => x.ArtifactId == _importSettings.CaseArtifactId);
    //         
    //         workspace.JobHistory
    //     }
    // }
}