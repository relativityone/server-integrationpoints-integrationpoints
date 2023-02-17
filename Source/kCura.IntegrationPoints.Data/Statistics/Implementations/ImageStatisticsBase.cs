using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity;
using Relativity.API;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public abstract class ImageStatisticsBase
    {
        protected readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;
        protected readonly IHelper _helper;
        protected readonly IAPILog _logger;

        protected ImageStatisticsBase(IRelativityObjectManagerFactory relativityObjectManagerFactory, IHelper helper, IAPILog logger)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
            _helper = helper;
            _logger = logger;
        }

        protected async Task<int> GetArtifactIdOfYesHoiceOnHasImagesAsync(int workspaceId)
        {
            int choiceYesArtifactId;
            int fieldArtifactID;

            IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);
            using (IChoiceQueryManager choiceQueryManager = _helper.GetServicesManager().CreateProxy<IChoiceQueryManager>(ExecutionIdentity.System))
            {
                QueryRequest queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Field },
                    Condition = $"'Name' == '{DocumentFieldsConstants.HasImagesFieldName}'",
                };

                ResultSet<RelativityObject> queryResult = await objectManager.QueryAsync(queryRequest, 0, 1).ConfigureAwait(false);
                fieldArtifactID = queryResult.Items.First().ArtifactID;

                var fieldChoicesList = await choiceQueryManager.QueryAsync(workspaceId, fieldArtifactID);
                var yesChoice = fieldChoicesList.FirstOrDefault(choice => choice.Name == DocumentFieldsConstants.HasImagesYesChoiceName);

                if (yesChoice == null)
                {
                    throw new NotFoundException($"Unable to find choice with '{DocumentFieldsConstants.HasImagesYesChoiceName}' name for '{DocumentFieldsConstants.HasImagesFieldName}' field.");
                }

                choiceYesArtifactId = yesChoice.ArtifactID;
            }

            return choiceYesArtifactId;
        }
    }
}
