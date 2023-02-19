using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.FieldsMapping.Metrics
{
    public class MetricBucketNameGenerator : IMetricBucketNameGenerator
    {
        private const string AutoMap = "AutoMap";
        private static readonly Guid DestinationProviderObjectTypeGuid = new Guid("d014f00d-f2c0-4e7a-b335-84fcb6eae980");
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;

        public MetricBucketNameGenerator(IServicesMgr servicesMgr, IAPILog logger)
        {
            _servicesMgr = servicesMgr;
            _logger = logger;
        }

        public async Task<string> GetAutoMapBucketNameAsync(string metricName, Guid destinationProviderGuid, int workspaceID)
        {
            try
            {
                using (IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
                {
                    QueryRequest queryRequest = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = DestinationProviderObjectTypeGuid
                        },
                        IncludeNameInQueryResult = true
                    };
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceID, queryRequest, start: 0, length: 1).ConfigureAwait(false);
                    RelativityObject destinationProviderRDO = queryResult.Objects.Single();

                    return $"{RemoveWhiteSpaces(destinationProviderRDO.Name)}.{AutoMap}.{metricName}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception occurred when querying for name of destination provider with GUID: '{destinationProviderGuid}'." +
                                       " Default bucket name will be used instead.", destinationProviderGuid);
                return $"{AutoMap}.{metricName}";
            }
        }

        private static string RemoveWhiteSpaces(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}
