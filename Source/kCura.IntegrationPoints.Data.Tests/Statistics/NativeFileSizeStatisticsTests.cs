using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture, Category("Unit")]
    public class NativeFileSizeStatisticsTests : TestBase
    {
        private const string _SQL_TEXT = "SELECT COALESCE(SUM([Size]),0) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";
        private const int _WORKSPACE_ID = 882826;
        private IAPILog _logger;
        private IHelper _helper;
        private IRelativityObjectManager _relativityObjectManager;
        private NativeFileSizeStatistics _instance;
        private IExportQueryResult _exportResult;

        public override void SetUp()
        {
            _logger = Substitute.For<IAPILog>();
            _helper = Substitute.For<IHelper>();
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();

            var relativityObjectManagerFactory = Substitute.For<IRelativityObjectManagerFactory>();
            relativityObjectManagerFactory.CreateRelativityObjectManager(_WORKSPACE_ID).Returns(_relativityObjectManager);
            _helper.GetLoggerFactory().GetLogger().ForContext<NativeFileSizeStatistics>().Returns(_logger);

            _exportResult = Substitute.For<IExportQueryResult>();

            _exportResult.ExportResult.Returns(new ExportInitializationResults { FieldData = new List<FieldMetadata>() });

            _relativityObjectManager.QueryWithExportAsync(Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<ExecutionIdentity>())
                .Returns(_exportResult);

            _instance = new NativeFileSizeStatistics(_helper, relativityObjectManagerFactory);
        }

        [Test]
        public void ItShouldReturnResultForFolder()
        {
            int expectedResult = 435;

            int folderId = 267232;
            int viewId = 204502;
            bool includeSubfolders = true;

            List<int> artifactIds = new List<int>
            {
                474,
                856,
                594
            };

            MockQueryResult(artifactIds);

            _helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
                    _SQL_TEXT,
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType)x.Value == FileType.Native))
                .Returns(expectedResult);

            var actualResult = _instance.ForFolder(_WORKSPACE_ID, folderId, viewId, includeSubfolders);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForSavedSearch()
        {
            int expectedResult = 669;

            int savedSearchId = 733381;

            List<int> artifactIds = new List<int>
            {
                268,
                348,
                679
            };

            MockQueryResult(artifactIds);

            _helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
                    _SQL_TEXT,
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType)x.Value == FileType.Native))
                .Returns(expectedResult);

            var actualResult = _instance.ForSavedSearch(_WORKSPACE_ID, savedSearchId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForProduction()
        {
            int expectedResult = 746;

            int productionId = 808193;

            List<int> artifactIds = new List<int>
            {
                152,
                534,
                907
            };

            MockQueryResult(artifactIds, ProductionConsts.DocumentFieldGuid);

            _helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
                    _SQL_TEXT,
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
                    Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType)x.Value == FileType.Native))
                .Returns(expectedResult);

            var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        private void MockQueryResult(List<int> artifactIds, Guid? guid = null)
        {
            var relativityObjectSlims = artifactIds.Select(x => new RelativityObjectSlim { ArtifactID = x }).ToList();

            if (guid.HasValue)
            {
                _exportResult.ExportResult.Returns(new ExportInitializationResults()
                {
                    FieldData = new List<FieldMetadata>
                    {
                        new FieldMetadata
                        {
                            Guids = new List<Guid> {guid.Value}
                        }
                    }
                });

                foreach (var objectSlim in relativityObjectSlims)
                {
                    objectSlim.Values = new List<object> { new RelativityObjectValue{ArtifactID = objectSlim.ArtifactID }};
                }
            }

            _exportResult.GetAllResultsAsync().Returns(relativityObjectSlims);
        }
    }
}
