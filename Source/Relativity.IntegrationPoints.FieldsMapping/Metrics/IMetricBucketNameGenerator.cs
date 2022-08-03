using System;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping.Metrics
{
    public interface IMetricBucketNameGenerator
    {
        Task<string> GetAutoMapBucketNameAsync(string metricName, Guid destinationProviderGuid, int workspaceID);
    }
}