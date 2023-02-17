using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class ChoiceRepository : IChoiceRepository
    {
        private const int _CHOICE_ARTIFACT_TYPE_ID = 7;
        private readonly IRelativityObjectManager _objectManager;

        public ChoiceRepository(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
        }

        public async Task<IList<ChoiceWithParentInfoDto>> QueryChoiceWithParentInfoAsync(
            ICollection<ChoiceDto> choicesToQuery,
            ICollection<ChoiceDto> allChoices)
        {
            string condition = BuildChoiceArtifactIDCondition(choicesToQuery);

            var queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _CHOICE_ARTIFACT_TYPE_ID },
                Fields = new FieldRef[0],
                Condition = condition
            };

            List<RelativityObject> choiceObjects = await _objectManager.QueryAsync(queryRequest).ConfigureAwait(false);

            return choiceObjects.Select(choiceObject => new
                {
                    choiceObject.ArtifactID,
                    choicesToQuery.Single(x => x.ArtifactID == choiceObject.ArtifactID).Name,
                    ParentArtifactID = allChoices.All(x => x.ArtifactID != choiceObject.ParentObject.ArtifactID)
                        ? null
                        : (int?) choiceObject.ParentObject.ArtifactID
                })
                .Select(x => new ChoiceWithParentInfoDto(x.ParentArtifactID, x.ArtifactID, x.Name))
                .ToList();
        }

        private static string BuildChoiceArtifactIDCondition(IEnumerable<ChoiceDto> choices)
        {
            IEnumerable<string> singleConditions = choices.Select(x => $"(('Artifact ID' == {x.ArtifactID}))");
            string condition = string.Join(" OR ", singleConditions);
            return condition;
        }
    }
}
