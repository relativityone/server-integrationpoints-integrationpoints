using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
    public class ChoiceService : IChoiceService
    {
        private readonly IServicesMgr _servicesMgr;

        public ChoiceService(IServicesMgr servicesMgr)
        {
            _servicesMgr = servicesMgr;
        }

        public List<FieldEntry> GetChoiceFields(int workspaceId, int rdoTypeId)
        {
            using (IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Field },
                    Condition = $"'FieldArtifactTypeID' == {rdoTypeId} " +
                                $"AND 'Field Type' IN ['{FieldTypes.SingleChoice}', '{Constants.Fields.MultipleChoice}']",
                    IncludeNameInQueryResult = true,
                    RankSortOrder = SortEnum.Ascending
                };

                QueryResult result = objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue).GetAwaiter().GetResult();

                return result.Objects.Select(x => new FieldEntry
                {
                    DisplayName = x.Name,
                    FieldIdentifier = x.ArtifactID.ToString(),
                    IsRequired = false
                }).ToList();
            }
        }
    }
}
