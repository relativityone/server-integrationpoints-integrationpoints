using Relativity.Import.V1.Models.Sources;

namespace Relativity.Sync.Extensions
{
    internal static class DataSourceDetailsExtensions
    {
        public static bool IsFinished(this DataSourceDetails dataSource)
        {
            return dataSource.State >= DataSourceState.Canceled;
        }
    }
}
