using Relativity.Import.V1.Models.Sources;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Utils
{
    internal static class DataSourceDetailsExtensions
    {
        public static bool IsFinished(this DataSourceDetails dataSource)
        {
            return dataSource.State == DataSourceState.Canceled
                || dataSource.State == DataSourceState.Failed
                || dataSource.State == DataSourceState.CompletedWithItemErrors
                || dataSource.State == DataSourceState.Completed;
        }
    }
}
