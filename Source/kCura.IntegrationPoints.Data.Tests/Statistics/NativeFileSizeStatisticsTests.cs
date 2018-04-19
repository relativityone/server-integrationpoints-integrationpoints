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
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
	[TestFixture]
	public class NativeFileSizeStatisticsTests : TestBase
	{
		private const string _SQL_TEXT = "SELECT COALESCE(SUM([Size]),0) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";

		private const int _WORKSPACE_ID = 882826;

		private IAPILog _logger;
		private IHelper _helper;
		private IRelativityObjectManager _relativityObjectManager;

		private NativeFileSizeStatistics _instance;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_relativityObjectManager = Substitute.For<IRelativityObjectManager>();

			var relativityObjectManagerFactory = Substitute.For<IRelativityObjectManagerFactory>();
			relativityObjectManagerFactory.CreateRelativityObjectManager(_WORKSPACE_ID).Returns(_relativityObjectManager);
			_helper.GetLoggerFactory().GetLogger().ForContext<NativeFileSizeStatistics>().Returns(_logger);

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

			var queryResult = MockQueryResult(artifactIds);
			_relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

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

			var queryResult = MockQueryResult(artifactIds);
			_relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

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

			var queryResult = MockQueryResult(artifactIds, ProductionConsts.DocumentFieldGuid);
			_relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

			_helper.GetDBContext(_WORKSPACE_ID).ExecuteSqlStatementAsScalar<long>(
					_SQL_TEXT,
					Arg.Is<SqlParameter>(x => x.ParameterName == "@ArtifactIds" && x.TypeName == "IDs"),
					Arg.Is<SqlParameter>(x => x.ParameterName == "@FileType" && (FileType)x.Value == FileType.Native))
				.Returns(expectedResult);

			var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		private static List<RelativityObject> MockQueryResult(List<int> artifactIds, Guid? guid = null)
		{
			if (guid.HasValue)
			{
				return artifactIds.Select(x => new RelativityObject { ArtifactID = x, FieldValues = CreateFieldValues(guid.Value, x) }).ToList();
			}
			else
			{
				return artifactIds.Select(x => new RelativityObject { ArtifactID = x }).ToList();
			}


		}

		private static List<FieldValuePair> CreateFieldValues(Guid fieldGuid, int value)
		{
			return new List<FieldValuePair>
			{
				new FieldValuePair
				{
					Field = new Field
					{
						Guids = new List<Guid> {fieldGuid }
					},
					Value = new RelativityObjectValue {ArtifactID = value}
				}
			};
		}

	}
}