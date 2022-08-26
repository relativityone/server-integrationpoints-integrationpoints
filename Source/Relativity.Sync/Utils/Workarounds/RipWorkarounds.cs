using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Utils.Workarounds
{
    internal class RipWorkarounds : IRipWorkarounds
    {
        private readonly ISourceServiceFactoryForAdmin _serviceFactory;
        private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
        private readonly IAPILog _logger;

        public RipWorkarounds(ISourceServiceFactoryForAdmin serviceFactory, IRdoGuidConfiguration rdoGuidConfiguration, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _logger = logger.ForContext<RipWorkarounds>();
        }

        public async Task TryUpdateIntegrationPointAsync(int workspaceId, int jobHistoryId, bool? hasErrors, DateTime lastRuntime)
        {
            Guid ripJobHistoryTypeGuid = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
            Guid hasErrorsFieldGuid = new Guid("a9853e55-0ba0-43d8-a766-747a61471981");
            Guid lastRuntimeFieldGuid = new Guid("90d58af1-f79f-40ae-85fc-7e42f84dbcc1");

            if (_rdoGuidConfiguration.JobHistory.TypeGuid != ripJobHistoryTypeGuid || !hasErrors.HasValue)
            {
                return;
            }

            try
            {
                using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest queryRequest = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = ripJobHistoryTypeGuid
                        },
                        Condition = $"'Artifact ID' == {jobHistoryId}",
                        Fields = new[]
                        {
                            new FieldRef()
                            {
                                Name = "Integration Point"
                            }
                        }
                    };
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, 0, 1).ConfigureAwait(false);
                    RelativityObject jobHistoryObject = queryResult.Objects.Single();
                    List<RelativityObjectValue> integrationPointFieldValues = (List<RelativityObjectValue>)jobHistoryObject.FieldValues.Single().Value;
                    int integrationPointArtifactId = integrationPointFieldValues.Single().ArtifactID;

                    UpdateRequest updateRequest = new UpdateRequest()
                    {
                        Object = new RelativityObjectRef()
                        {
                            ArtifactID = integrationPointArtifactId
                        },
                        FieldValues = new[]
                        {
                            new FieldRefValuePair()
                            {
                                Field = new FieldRef()
                                {
                                    Guid = hasErrorsFieldGuid
                                },
                                Value = hasErrors.Value
                            },
                            new FieldRefValuePair()
                            {
                                Field = new FieldRef()
                                {
                                    Guid = lastRuntimeFieldGuid
                                },
                                Value = lastRuntime
                            }
                        }
                    };
                    await objectManager.UpdateAsync(workspaceId, updateRequest).ConfigureAwait(false);
                    _logger.LogInformation("Integration Point ID {integrationPointArtifactId} has been successfully updated", integrationPointArtifactId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Integration Point. Has Errors: {hasErrors} Last Runtime: {lastRuntime}", hasErrors, lastRuntime);
            }
        }
    }
}
