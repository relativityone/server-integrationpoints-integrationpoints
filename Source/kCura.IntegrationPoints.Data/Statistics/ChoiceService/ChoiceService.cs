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

namespace kCura.IntegrationPoints.Data.Statistics
{
    internal class ChoiceService : IChoiceService
    {
        protected readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;
        protected readonly IHelper _helper;
        protected readonly IAPILog _logger;

        public ChoiceService(IRelativityObjectManagerFactory relativityObjectManagerFactory, IHelper helper, IAPILog logger)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
            _helper = helper;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<int> GetGuidOfYesChoiceOnHasImagesAsync(int workspaceArtifactId)
        {
            IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId);
            using (IChoiceQueryManager choiceQueryManager = _helper.GetServicesManager().CreateProxy<IChoiceQueryManager>(ExecutionIdentity.System))
            {
                var query = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                    Condition = "'Name' == 'Has Images'",
                };

                ResultSet<RelativityObject> queryResult = await objectManager.QueryAsync(query, 0, 1).ConfigureAwait(false);
                int fieldArtifactID = queryResult.Items.First().ArtifactID;

                var fieldChoicesList = await choiceQueryManager.QueryAsync(workspaceArtifactId, fieldArtifactID);
                var yesChoice = fieldChoicesList.FirstOrDefault(choice => choice.Name == "Yes");

                if(yesChoice == null)
                {
                    _logger.LogError("Unable to find choice with \"Yes\" name for \"Has Images\" field - this system field is in invalid state");
                    throw new NotFoundException("Unable to find choice with \"Yes\" name for \"Has Images\" field - this system field is in invalid state");
                }

                return yesChoice.ArtifactID;
            }
        }
    }
}
