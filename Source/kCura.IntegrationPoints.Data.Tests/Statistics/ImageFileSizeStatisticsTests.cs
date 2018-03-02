using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
	[TestFixture]
	public class ImageFileSizeStatisticsTests : TestBase
	{
		private const string _PRODUCTION_SQL = "SELECT COALESCE(SUM([Size]),0) FROM [{0}] AS PDF JOIN [File] AS F ON F.[FileID] = PDF.[ProducedFileID]";
		private const string _SQL_TEXT = "SELECT COALESCE(SUM([Size]),0) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";

		private const int _WORKSPACE_ID = 218772;

		private IAPILog _logger;
		private IHelper _helper;
		private IRdoRepository _rdoRepository;

		private ImageFileSizeStatistics _instance;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_rdoRepository = Substitute.For<IRdoRepository>();

			var repositoryFactory = Substitute.For<IRepositoryFactory>();
			repositoryFactory.GetRdoRepository(_WORKSPACE_ID).Returns(_rdoRepository);
			_helper.GetLoggerFactory().GetLogger().ForContext<ImageFileSizeStatistics>().Returns(_logger);

			_instance = new ImageFileSizeStatistics(_helper, repositoryFactory);
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
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(queryResult);

			_helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
					_SQL_TEXT,
					Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
					Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType) x.Value == FileType.Tif))
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
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(queryResult);

			_helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
					_SQL_TEXT,
					Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
					Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType) x.Value == FileType.Tif))
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

		private static QueryResultSet<RDO> MockQueryResult(List<int> artifactIds)
		{
			var queryResult = new QueryResultSet<RDO>
			{
				Results = new List<Result<RDO>>(artifactIds.Select(x => new Result<RDO>
				{
					Artifact = new RDO(x)
				}))
			};
			return queryResult;
		}
	}
}