using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
	[TestFixture]
	public class DocumentTotalStatisticsTests : TestBase
	{
		private const int _WORKSPACE_ID = 237273;

		private IAPILog _logger;
		private IHelper _helper;
		private IRdoRepository _rdoRepository;

		private DocumentTotalStatistics _instance;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_rdoRepository = Substitute.For<IRdoRepository>();

			var repositoryFactory = Substitute.For<IRepositoryFactory>();
			repositoryFactory.GetRdoRepository(_WORKSPACE_ID).Returns(_rdoRepository);
			_helper.GetLoggerFactory().GetLogger().ForContext<DocumentTotalStatistics>().Returns(_logger);

			_instance = new DocumentTotalStatistics(_helper, repositoryFactory);
		}

		[Test]
		public void ItShouldReturnResultForFolder()
		{
			int expectedResult = 960;

			int folderId = 794276;
			int viewId = 453234;
			bool includeSubfolders = true;

			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(new QueryResultSet<RDO>
			{
				TotalCount = expectedResult
			});

			var actualResult = _instance.ForFolder(_WORKSPACE_ID, folderId, viewId, includeSubfolders);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldReturnResultForSavedSearch()
		{
			int expectedResult = 439;

			int savedSearchId = 883508;

			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(new QueryResultSet<RDO>
			{
				TotalCount = expectedResult
			});

			var actualResult = _instance.ForSavedSearch(_WORKSPACE_ID, savedSearchId);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldReturnResultForProduction()
		{
			int expectedResult = 979;

			int productionId = 446524;

			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(new QueryResultSet<RDO>
			{
				TotalCount = expectedResult
			});

			var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldLogError()
		{
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Throws(new Exception());

			Assert.That(() => _instance.ForFolder(_WORKSPACE_ID, 759, 464, true), Throws.Exception);
			Assert.That(() => _instance.ForProduction(_WORKSPACE_ID, 523), Throws.Exception);
			Assert.That(() => _instance.ForSavedSearch(_WORKSPACE_ID, 434), Throws.Exception);

			_logger.Received(3).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}