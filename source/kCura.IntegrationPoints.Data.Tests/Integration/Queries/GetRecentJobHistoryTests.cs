using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Queries
{
	[Explicit]
	[TestFixture]
	public class GetRecentJobHistoryTests
	{

		public IRSAPIService Service
		{
			get
			{
				var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new UsernamePasswordCredentials("a@kcura.com", "Test1234!"))
				{
					APIOptions = { WorkspaceID = 1025258 }
				};
				var service = new RSAPIService();
				service.JobHistoryErrorLibrary = new RsapiClientLibrary<JobHistoryError>(client);
				return service;
			}
		}

		public JobHistoryErrorQuery HistoryQuery
		{
			get
			{
				return new JobHistoryErrorQuery(this.Service);
			}
		}

		public const int ONLY_ITEM_ERROR_ID = 1037898;
		public const int ONLY_JOB_LEVEL_ERROR_ID = 1037909;
		public const int BOTH_JOB_ITEM_AND_LEVEL_ERROR_ID = 1037912;
		public const int NO_ERRORS_ID = 1037917;

		[Test]
		public void JobHistoryHasOnlyJobItemErrors_ReturnsMostRecentJobItemError()
		{
			var service = this.HistoryQuery;

			var result = service.GetJobErrorFailedStatus(ONLY_ITEM_ERROR_ID);

			Assert.IsTrue(result.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorItem));
		}

		[Test]
		public void JobHistoryHasOnlyJobLevelErrors_ReturnsMostRecentJobLevel()
		{
			var service = this.HistoryQuery;

			var result = service.GetJobErrorFailedStatus(ONLY_JOB_LEVEL_ERROR_ID);

			Assert.IsTrue(result.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorJob));
		}

		[Test]
		public void JobHistoryHasBothJobLevelAndItemErrors_ReturnsMostRecentJobLevel()
		{
			var service = this.HistoryQuery;

			var result = service.GetJobErrorFailedStatus(BOTH_JOB_ITEM_AND_LEVEL_ERROR_ID);

			Assert.IsTrue(result.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorJob));
		}

		[Test]
		public void JobHistoryHasNoErrors_ReturnsNull()
		{
			var service = this.HistoryQuery;

			var result = service.GetJobErrorFailedStatus(NO_ERRORS_ID);

			Assert.IsTrue(result == null);
		}

	}
}
