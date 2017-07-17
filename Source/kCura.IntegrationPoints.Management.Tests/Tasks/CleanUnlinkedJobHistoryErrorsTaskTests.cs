using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Management.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Management.Tests.Tasks
{
	[TestFixture]
	public class CleanUnlinkedJobHistoryErrorsTaskTests : TestBase
	{
		private CleanUnlinkedJobHistoryErrorsTask _instance;
		private IDeleteHistoryErrorService _deleteHistoryErrorService;
		private IUnlinkedJobHistoryService _unlinkedJobHistoryService;

		public override void SetUp()
		{
			_deleteHistoryErrorService = Substitute.For<IDeleteHistoryErrorService>();
			_unlinkedJobHistoryService = Substitute.For<IUnlinkedJobHistoryService>();

			_instance = new CleanUnlinkedJobHistoryErrorsTask(_deleteHistoryErrorService, _unlinkedJobHistoryService);
		}

		[Test]
		public void ItShouldDeleteUnlinkedJobHistoryErrors()
		{
			List<int> workspaceIds = new List<int> {366406};
			List<int> jobHistoryIds = new List<int> {300, 471};

			_unlinkedJobHistoryService.FindUnlinkedJobHistories(workspaceIds[0]).Returns(jobHistoryIds);

			// ACT
			_instance.Run(workspaceIds);

			// ASSERT
			_unlinkedJobHistoryService.Received(1).FindUnlinkedJobHistories(workspaceIds[0]);
			_deleteHistoryErrorService.Received(1).DeleteErrorAssociatedWithHistories(jobHistoryIds, workspaceIds[0]);
		}
	}
}