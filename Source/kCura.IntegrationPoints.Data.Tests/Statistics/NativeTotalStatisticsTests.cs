using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture, Category("Unit")]
    public class NativeTotalStatisticsTests : TestBase
    {
        private const int _WORKSPACE_ID = 710262;

        private IAPILog _logger;
        private IHelper _helper;
        private IRelativityObjectManager _relativityObjectManager;

        private NativeTotalStatistics _instance;

        public override void SetUp()
        {
            _logger = Substitute.For<IAPILog>();
            _helper = Substitute.For<IHelper>();
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();

            var relativityObjectManagerFactory = Substitute.For<IRelativityObjectManagerFactory>();
            relativityObjectManagerFactory.CreateRelativityObjectManager(_WORKSPACE_ID).Returns(_relativityObjectManager);
            _helper.GetLoggerFactory().GetLogger().ForContext<NativeTotalStatistics>().Returns(_logger);

            _instance = new NativeTotalStatistics(_helper, relativityObjectManagerFactory);
        }

        [Test]
        public void ItShouldReturnResultForFolder()
        {
            int expectedResult = 215;

            int folderId = 722737;
            int viewId = 972303;
            bool includeSubfolders = true;

            _relativityObjectManager.QueryTotalCount(Arg.Any<QueryRequest>()).Returns(expectedResult);

            var actualResult = _instance.ForFolder(_WORKSPACE_ID, folderId, viewId, includeSubfolders);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForSavedSearch()
        {
            int expectedResult = 267;

            int savedSearchId = 951123;

            _relativityObjectManager.QueryTotalCount(Arg.Any<QueryRequest>()).Returns(expectedResult);

            var actualResult = _instance.ForSavedSearch(_WORKSPACE_ID, savedSearchId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForProduction()
        {
            int expectedResult = 580;

            int productionId = 378814;

            _relativityObjectManager.QueryTotalCount(Arg.Any<QueryRequest>()).Returns(expectedResult);

            var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldLogError()
        {
            _relativityObjectManager.QueryTotalCount(Arg.Any<QueryRequest>()).Throws(new Exception());

            Assert.That(() => _instance.ForFolder(_WORKSPACE_ID, 438, 623, true), Throws.Exception);
            Assert.That(() => _instance.ForProduction(_WORKSPACE_ID, 696), Throws.Exception);
            Assert.That(() => _instance.ForSavedSearch(_WORKSPACE_ID, 929), Throws.Exception);

            _logger.Received(3).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}