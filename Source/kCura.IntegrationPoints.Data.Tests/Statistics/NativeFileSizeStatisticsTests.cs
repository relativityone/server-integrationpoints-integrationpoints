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
	public class NativeFileSizeStatisticsTests : TestBase
	{
		private const string _SQL_TEXT = "SELECT SUM([Size]) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";

		private const int _WORKSPACE_ID = 882826;

		private IAPILog _logger;
		private IHelper _helper;
		private IRdoRepository _rdoRepository;

		private NativeFileSizeStatistics _instance;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_rdoRepository = Substitute.For<IRdoRepository>();

			var repositoryFactory = Substitute.For<IRepositoryFactory>();
			repositoryFactory.GetRdoRepository(_WORKSPACE_ID).Returns(_rdoRepository);
			_helper.GetLoggerFactory().GetLogger().ForContext<NativeFileSizeStatistics>().Returns(_logger);

			_instance = new NativeFileSizeStatistics(_helper, repositoryFactory);
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

			var queryResult = MockQueryResult(artifactIds);
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(queryResult);

			_helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<int>(
					_SQL_TEXT,
					Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
					Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType) x.Value == FileType.Native))
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

			var queryResult = MockQueryResult(artifactIds);
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(queryResult);

			_helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<int>(
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

			var queryResult = MockQueryResult(artifactIds, ProductionConsts.DocumentFieldGuid);
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(queryResult);

			_helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<int>(
					_SQL_TEXT,
					Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
					Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType)x.Value == FileType.Native))
				.Returns(expectedResult);

			var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldLogError()
		{
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Throws(new Exception());

			Assert.That(() => _instance.ForFolder(_WORKSPACE_ID, 670, 692, true), Throws.Exception);
			Assert.That(() => _instance.ForProduction(_WORKSPACE_ID, 234), Throws.Exception);
			Assert.That(() => _instance.ForSavedSearch(_WORKSPACE_ID, 498), Throws.Exception);

			_logger.Received(3).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		private static QueryResultSet<RDO> MockQueryResult(List<int> artifactIds, Guid? guid = null)
		{
			if (guid.HasValue)
			{
				var queryResult = new QueryResultSet<RDO>
				{
					Results = new List<Result<RDO>>(artifactIds.Select(x =>
					{
						var rdo = new Result<RDO>
						{
							Artifact = new RDO(x)
						};
						rdo.Artifact.Fields.Add(new FieldValue(guid.Value, new RDO(x)));
						return rdo;
					}))
				};
				return queryResult;
			}
			else
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
}