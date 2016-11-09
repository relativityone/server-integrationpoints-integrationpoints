using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class DeleteHistoryServiceTest : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void DeleteHistoriesAssociatedWithIPs_IntegrationPoint_SetJobHistoryNull()
		{
			//ARRANGE
			var service = NSubstitute.Substitute.For<IRSAPIService>();
			var deleteError = NSubstitute.Substitute.For<IDeleteHistoryErrorService>();

			var deleteHistoryService = new DeleteHistoryService(service, deleteError);

			var integrationPoint = new List<Data.IntegrationPoint>()
			{
				new Data.IntegrationPoint()
				{
					JobHistory = new int[] {1, 2, 3}
				},
					new Data.IntegrationPoint()
				{
					JobHistory = new int[] {1, 2, 3}
				}
			};
			deleteError.DeleteErrorAssociatedWithHistories(Arg.Any<List<int>>());

			service.IntegrationPointLibrary.Query(Arg.Any<Query<RDO>>()).Returns(integrationPoint);

			//ACT
			deleteHistoryService.DeleteHistoriesAssociatedWithIPs(new List<int>() { 1, 2 });

			//ASSERT
			service.IntegrationPointLibrary.Received().Update(Arg.Is<IEnumerable<Data.IntegrationPoint>>(x => x.All(y => !y.JobHistory.Any())));
		}
	}
}