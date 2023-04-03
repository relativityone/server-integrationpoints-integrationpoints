using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class FileSizesStatisticsService : IFileSizesStatisticsService
    {
        private readonly INativeFileSizeStatistics _nativeFileSizeStatistics;
        private readonly IImageFileSizeStatistics _imageFileSizeStatistics;
        private readonly IErrorFilesSizeStatistics _errorFilesSizeStatistics;

        public FileSizesStatisticsService(INativeFileSizeStatistics nativeFileSizeStatistics, IImageFileSizeStatistics imageFileSizeStatistics, IErrorFilesSizeStatistics errorFilesSizeStatistics)
        {
            _nativeFileSizeStatistics = nativeFileSizeStatistics;
            _imageFileSizeStatistics = imageFileSizeStatistics;
            _errorFilesSizeStatistics = errorFilesSizeStatistics;
        }

        public long CalculatePushedFilesSizeForJobHistory(int jobId, DestinationConfiguration destinationConfiguration, SourceConfiguration sourceConfiguration)
        {
            if (!IsImportWithNatives(destinationConfiguration) || sourceConfiguration == null)
            {
                return 0;
            }

            long filesSize = CalculateAllFilesSize(destinationConfiguration, sourceConfiguration);
            long errorsFileSize = CalculateErroredFilesSize(jobId, sourceConfiguration);

            long copiedFilesFileSize = filesSize - errorsFileSize;
            return copiedFilesFileSize;
        }

        private static bool IsImportWithNatives(DestinationConfiguration destinationConfiguration)
        {
            return (destinationConfiguration?.ImportNativeFile).GetValueOrDefault(false);
        }

        private long CalculateAllFilesSize(DestinationConfiguration destinationConfiguration, SourceConfiguration sourceConfiguration)
        {
            long filesSize = 0;

            switch (sourceConfiguration.TypeOfExport)
            {
                case SourceConfiguration.ExportType.SavedSearch:
                    filesSize = CalculateFileSizeForSavedSearchExport(destinationConfiguration, sourceConfiguration);
                    break;
                case SourceConfiguration.ExportType.ProductionSet:
                    filesSize = CalculateFileSizeForProductionSetExport(sourceConfiguration);
                    break;
            }

            return filesSize;
        }

        private long CalculateFileSizeForProductionSetExport(SourceConfiguration sourceConfiguration)
        {
            return _imageFileSizeStatistics.ForProduction(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.SourceProductionId);
        }

        private long CalculateFileSizeForSavedSearchExport(DestinationConfiguration destinationConfiguration, SourceConfiguration sourceConfiguration)
        {
            return destinationConfiguration.ImageImport
                ? _imageFileSizeStatistics.ForSavedSearch(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.SavedSearchArtifactId)
                : _nativeFileSizeStatistics.ForSavedSearch(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.SavedSearchArtifactId);
        }

        private long CalculateErroredFilesSize(int jobId, SourceConfiguration sourceConfiguration)
        {
            return _errorFilesSizeStatistics.ForJobHistoryOmmitedFiles(sourceConfiguration.SourceWorkspaceArtifactId, jobId);
        }
    }
}
