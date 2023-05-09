using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IFileSizesStatisticsService
    {
        long CalculatePushedFilesSizeForJobHistory(int jobId, DestinationConfiguration destinationConfiguration, SourceConfiguration sourceConfiguration);
    }
}
