using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class DeleteHistoryServiceTest
	{
		[Test]
		public void deleteHistoriesAssociatedWithIPs_integrationPoint_setJobHistoryNull()
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
			deleteHistoryService.DeleteHistoriesAssociatedWithIPs(new List<int>() { 1, 2 });

			//ACT

			//do call

			//ASSERT
			service.IntegrationPointLibrary.Received().Update(Arg.Is<IEnumerable<Data.IntegrationPoint>>(x => x.All(y => !y.JobHistory.Any()))); //
		}

		[Test]
		public void deleteHistoriesAssociatedWithIPs_integrationPoint_correctIdsSentToRemove()
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
					JobHistory = new int[] {4,5, 6}
				}
			};
			service.IntegrationPointLibrary.Query(Arg.Any<Query<RDO>>()).Returns(integrationPoint);
			deleteHistoryService.DeleteHistoriesAssociatedWithIPs(new List<int>() { 1, 2 });

			//ACT

			//do call

			//ASSERT
			var allJobHistory = new int[] { 1, 2, 3, 4, 5, 6 };
			service.JobHistoryLibrary.Received().Delete(Arg.Is<IEnumerable<int>>(x => x.All(allJobHistory.Contains)));
		}
	}
}