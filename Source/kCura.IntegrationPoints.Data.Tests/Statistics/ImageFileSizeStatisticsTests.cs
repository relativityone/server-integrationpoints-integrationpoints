using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture, Category("Unit")]
    public class ImageFileSizeStatisticsTests : ImageStatisticsTestBase
    {
        private const string _PRODUCTION_SQL = "SELECT COALESCE(SUM([Size]),0) FROM [{0}] AS PDF JOIN [File] AS F ON F.[FileID] = PDF.[ProducedFileID]";
        private const string _SQL_TEXT = "SELECT COALESCE(SUM([Size]),0) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";
        private ImageFileSizeStatistics _instance;

        public override void SetUp()
        {
            base.SetUp();
            _helper.GetLoggerFactory().GetLogger().ForContext<ImageFileSizeStatistics>().Returns(_logger);
            _instance = new ImageFileSizeStatistics(_helper, _repositoryFactory);
        }

        [Test]
        public void ItShouldReturnResultForFolder()
        {
            long expectedResult = 214;

            int folderId = 767904;
            int viewId = 913638;
            bool includeSubfolders = true;

            List<int> artifactIds = new List<int>
            {
                729,
                414,
                607
            };

            var queryResult = MockQueryResult(artifactIds);
            _relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

            _helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
                    _SQL_TEXT,
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType)x.Value == FileType.Tif))
                .Returns(expectedResult);

            var actualResult = _instance.ForFolder(_WORKSPACE_ID, folderId, viewId, includeSubfolders);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForSavedSearch()
        {
            long expectedResult = 656;

            int savedSearchId = 877574;

            List<int> artifactIds = new List<int>
            {
                462,
                413,
                540
            };

            var queryResult = MockQueryResult(artifactIds);
            _relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

            _helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
                    _SQL_TEXT,
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType)x.Value == FileType.Tif))
                .Returns(expectedResult);

            var actualResult = _instance.ForSavedSearch(_WORKSPACE_ID, savedSearchId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForProduction()
        {
            long expectedResult = 657;

            int productionId = 330796;

            _helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(string.Format(_PRODUCTION_SQL, $"ProductionDocumentFile_{productionId}")).Returns(expectedResult);

            var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        private static List<RelativityObject> MockQueryResult(List<int> artifactIds)
        {
            return artifactIds.Select(x => new RelativityObject { ArtifactID = x }).ToList();
        }
    }
}
