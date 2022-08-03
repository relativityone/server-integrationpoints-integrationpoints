using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture, Category("Unit")]
    public class ImageStatisticsTestBase : TestBase
    {
        protected const int _WORKSPACE_ID = 218772;

        protected IAPILog _logger;
        protected IHelper _helper;
        protected IRelativityObjectManager _relativityObjectManager;
        protected IRelativityObjectManagerFactory _repositoryFactory;

        public override void SetUp()
        {

            _logger = Substitute.For<IAPILog>();
            _helper = Substitute.For<IHelper>();
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();

            const int hasImagesArtifactId = 111;
            const int yesChoiceArtifactId = 222;

            _relativityObjectManager
                .QueryAsync(Arg.Is<QueryRequest>(q => q.Condition == "'Name' == 'Has Images'"), 0, 1)
                .Returns(new ResultSet<RelativityObject>()
                {
                    Items = new List<RelativityObject>()
                    {
                        new RelativityObject()
                        {
                            ArtifactID = hasImagesArtifactId
                        }
                    }
                });

            IChoiceQueryManager choiceManager = Substitute.For<IChoiceQueryManager>();
            choiceManager.QueryAsync(_WORKSPACE_ID, hasImagesArtifactId).Returns(new List<global::Relativity.Services.ChoiceQuery.Choice>()
            {
                new global::Relativity.Services.ChoiceQuery.Choice()
                {
                    ArtifactID = yesChoiceArtifactId,
                    Name = "Yes"
                }
            });

            _helper.GetServicesManager().CreateProxy<IChoiceQueryManager>(ExecutionIdentity.System).Returns(choiceManager);

            _repositoryFactory = Substitute.For<IRelativityObjectManagerFactory>();
            _repositoryFactory.CreateRelativityObjectManager(_WORKSPACE_ID).Returns(_relativityObjectManager);
        }
    }
}