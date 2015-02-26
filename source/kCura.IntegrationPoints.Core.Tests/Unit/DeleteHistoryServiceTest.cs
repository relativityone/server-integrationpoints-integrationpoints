using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.Core;
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

			var deleteHistoryService = new DeleteHistoryService(service);
			var integrationPoint = new List<IntegrationPoint>()
			{
				new IntegrationPoint()
				{
					JobHistory = new int[] {1, 2, 3}
				},
					new IntegrationPoint()
				{
					JobHistory = new int[] {1, 2, 3}
				}
			};
			service.IntegrationPointLibrary.Query(Arg.Any<Query<RDO>>()).Returns(integrationPoint);
			deleteHistoryService.DeleteHistoriesAssociatedWithIPs(new List<int>(){1,2});
			
			//ACT

			//do call
			
			//ASSERT
			service.IntegrationPointLibrary.Received().Update(Arg.Is<IEnumerable<IntegrationPoint>>(x=>x.All(y=>!y.JobHistory.Any()))); //
		}

		[Test]
		public void deleteHistoriesAssociatedWithIPs_integrationPoint_correctIdsSentToRemove()
		{
			//ARRANGE
			var service = NSubstitute.Substitute.For<IRSAPIService>();

			var deleteHistoryService = new DeleteHistoryService(service);
			var integrationPoint = new List<IntegrationPoint>()
			{
				new IntegrationPoint()
				{
					JobHistory = new int[] {1, 2, 3}
				},
					new IntegrationPoint()
				{
					JobHistory = new int[] {4,5, 6}
				}
			};
			service.IntegrationPointLibrary.Query(Arg.Any<Query<RDO>>()).Returns(integrationPoint);
			deleteHistoryService.DeleteHistoriesAssociatedWithIPs(new List<int>(){1,2});
			
			//ACT

			//do call
			
			//ASSERT
			var allJobHistory = new int[] {1,2,3,4,5,6};
			service.JobHistoryLibrary.Received().Delete(Arg.Is<IEnumerable<int>>(x => x.All(allJobHistory.Contains)));
			
		}

		
	
	}
}
