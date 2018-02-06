using System;
using System.Collections.Generic;
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
	[TestFixture]
	public class ImageTotalStatisticsTests : TestBase
	{
		private const int _WORKSPACE_ID = 237273;

		private IAPILog _logger;
		private IHelper _helper;
		private IRelativityObjectManager _rdoRepository;

		private ImageTotalStatistics _instance;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_rdoRepository = Substitute.For<IRelativityObjectManager>();

			var repositoryFactory = Substitute.For<IRelativityObjectManagerFactory>();
			repositoryFactory.CreateRelativityObjectManager(_WORKSPACE_ID).Returns(_rdoRepository);
			_helper.GetLoggerFactory().GetLogger().ForContext<ImageTotalStatistics>().Returns(_logger);

			_instance = new ImageTotalStatistics(_helper, repositoryFactory);
		}

		[Test]
		public void ItShouldReturnResultForFolder()
		{
			int expectedResult = 554;

			int folderId = 114438;
			int viewId = 398415;
			bool includeSubfolders = true;

			var queryResult = MockQueryResult(expectedResult, DocumentFieldsConstants.RelativityImageCount);
			_rdoRepository.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

			var actualResult = _instance.ForFolder(_WORKSPACE_ID, folderId, viewId, includeSubfolders);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldReturnResultForSavedSearch()
		{
			int expectedResult = 879;

			int savedSearchId = 768974;

			var queryResult = MockQueryResult(expectedResult, DocumentFieldsConstants.RelativityImageCount);
			_rdoRepository.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

			var actualResult = _instance.ForSavedSearch(_WORKSPACE_ID, savedSearchId);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldReturnResultForProduction()
		{
			int expectedResult = 424;

			int productionId = 998788;

			var queryResult = MockQueryResult(expectedResult, ProductionConsts.ImageCountFieldGuid);
			_rdoRepository.Query(Arg.Any<QueryRequest>()).Returns(queryResult);

			var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		private static List<RelativityObject> MockQueryResult(int expectedResult, Guid fieldGuid)
		{
			return new List<RelativityObject>
			{
				new RelativityObject
				{
					FieldValues = new List<FieldValuePair>
					{
						new FieldValuePair
						{
							Field = new Field
							{
								Guids = new List<Guid> { fieldGuid }
							},
							Value = expectedResult
						}
					}
				}
			};
		}

		[Test]
		public void ItShouldLogError()
		{
			_rdoRepository.Query(Arg.Any<QueryRequest>()).Throws(new Exception());

			Assert.That(() => _instance.ForFolder(_WORKSPACE_ID, 157, 237, true), Throws.Exception);
			Assert.That(() => _instance.ForProduction(_WORKSPACE_ID, 465), Throws.Exception);
			Assert.That(() => _instance.ForSavedSearch(_WORKSPACE_ID, 740), Throws.Exception);

			_logger.Received(3).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}
