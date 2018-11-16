using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
	[TestFixture]
	public class DeleteIntegrationPointsTests : TestBase
	{
		private IIntegrationPointQuery _integrationPointQuery;
		private IDeleteHistoryService _deleteHistoryService;
		private IRelativityObjectManager _relativityObjectManager;

		[SetUp]
		public override void SetUp()
		{
			_relativityObjectManager = Substitute.For<IRelativityObjectManager>();

			_deleteHistoryService = Substitute.For<IDeleteHistoryService>();
			_integrationPointQuery = Substitute.For<IIntegrationPointQuery>();
		}

		[Test]
		public void ItShouldThrowExceptionWhenIntegrationPointQueryIsNullTest()
		{
			// arrange
			var deleteIntegrationPoints = new DeleteIntegrationPoints(null, _deleteHistoryService, _relativityObjectManager);
			var ids = new List<int> { 1, 2, 3 };

			// act & assert
			Assert.Throws<NullReferenceException>(() => deleteIntegrationPoints.DeleteIPsWithSourceProvider(ids));
		}

		[Test]
		public void ItShouldThrowExceptionWhenDeleteHistoryServiceIsNullTest()
		{
			// arrange
			var deleteIntegrationPoints = new DeleteIntegrationPoints(_integrationPointQuery, null, _relativityObjectManager);
			var ids = new List<int> { 1, 2, 3 };

			// act & assert
			Assert.Throws<NullReferenceException>(() => deleteIntegrationPoints.DeleteIPsWithSourceProvider(ids));
		}

		[TestCase(new int[] { })]
		[TestCase(new[] { 1 })]
		[TestCase(new[] { 1, 6, 3 })]
		public void ItShouldCallWithProperArgumentIntegrationPointQueryTest(int[] sourceProvider)
		{
			// arrange
			var deleteIntegrationPoints = new DeleteIntegrationPoints(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

			// act
			deleteIntegrationPoints.DeleteIPsWithSourceProvider(sourceProvider.ToList());

			// assert
			_integrationPointQuery.Received(1).GetIntegrationPoints(Arg.Is<List<int>>(x => x.SequenceEqual(sourceProvider)));
		}

		[TestCase(new int[0], new int[0])]
		[TestCase(new[] { 1, 2, 3 }, new[] { 5, 7, 6 })]
		public void ItShouldCallWithProperArgumentsDeleteHistoryServiceTest(int[] sourceProvider, int[] artifactIds)
		{
			// arrange
			var integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
			_integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);
			var deleteIntegrationPoints = new DeleteIntegrationPoints(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

			// act
			deleteIntegrationPoints.DeleteIPsWithSourceProvider(sourceProvider.ToList());

			// assert
			_deleteHistoryService.Received(1).DeleteHistoriesAssociatedWithIPs(Arg.Is<List<int>>(x => x.SequenceEqual(artifactIds)), _relativityObjectManager);
		}

		[TestCase(new int[0], new int[0])]
		[TestCase(new[] { 1, 2, 3 }, new[] { 5, 7, 6 })]
		public void ItShouldCallWithProperArgumentsIntegrationPointLibraryTest(int[] sourceProvider, int[] artifactIds)
		{
			// arrange
			var integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
			_integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);
			var deleteIntegrationPoints = new DeleteIntegrationPoints(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

			// act
			deleteIntegrationPoints.DeleteIPsWithSourceProvider(sourceProvider.ToList());

			// assert
			foreach (var ip in integrationPoints)
			{
				_relativityObjectManager.Received(1)
					.Delete<Data.IntegrationPoint>(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == ip.ArtifactId));
			}
		}

		[Test]
		public void ItShouldCallWithProperArgumentsIntegrationPointLibraryWhenCalledTwoTimesTest()
		{
			// arrange
			var sourceProvider = new List<int> { 1 };
			var deleteIntegrationPoints = new DeleteIntegrationPoints(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

			var artifactIds = new[] { 7 };
			var integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
			_integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);

			deleteIntegrationPoints.DeleteIPsWithSourceProvider(sourceProvider);

			var secondArtifactIds = new[] { 8 };
			var secondintegrationPoints = GetMockIntegrationsPoints(secondArtifactIds).ToList();
			_integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(secondintegrationPoints);

			// act
			deleteIntegrationPoints.DeleteIPsWithSourceProvider(sourceProvider);

			// assert
			foreach (var ip in integrationPoints)
			{
				_relativityObjectManager.Received(1).Delete<Data.IntegrationPoint>(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == ip.ArtifactId));
			}
			foreach (var ip in secondintegrationPoints)
			{
				_relativityObjectManager.Received(1).Delete<Data.IntegrationPoint>(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == ip.ArtifactId));
			}
		}

		private IEnumerable<Data.IntegrationPoint> GetMockIntegrationsPoints(IEnumerable<int> artifactIds)
		{
			return artifactIds.Select(artifactId => new Data.IntegrationPoint { ArtifactId = artifactId });
		}
	}
}
