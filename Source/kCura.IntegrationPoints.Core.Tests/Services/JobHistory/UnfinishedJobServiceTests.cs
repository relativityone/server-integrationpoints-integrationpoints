using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
	[TestFixture]
	public class UnfinishedJobServiceTests : TestBase
	{
		private const int _WORKSPACE_ID = 551970;

		private UnfinishedJobService _instance;
		private IRSAPIService _rsapiService;

		public override void SetUp()
		{
			_rsapiService = Substitute.For<IRSAPIService>();

			IRSAPIServiceFactory rsapiServiceFactory = Substitute.For<IRSAPIServiceFactory>();
			rsapiServiceFactory.Create(_WORKSPACE_ID).Returns(_rsapiService);

			_instance = new UnfinishedJobService(rsapiServiceFactory);
		}

		[Test]
		public void ItShouldQueryForUnfinishedJobs()
		{
			// ACT
			_instance.GetUnfinishedJobs(_WORKSPACE_ID);

			// ASSERT
			_rsapiService.JobHistoryLibrary.Received(1).Query(Arg.Is<Query<RDO>>(x => CheckCondition(x)));
		}

		private bool CheckCondition(Query<RDO> query)
		{
			var unfinishedChoicesNames = new List<Guid>
			{
				JobStatusChoices.JobHistoryPending.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault(),
				JobStatusChoices.JobHistoryStopping.Guids.FirstOrDefault()
			};

			var singleChoiceCondition = (SingleChoiceCondition) query.Condition;
			var value = (IEnumerable<Guid>) singleChoiceCondition.Value;
			return value.SequenceEqual(unfinishedChoicesNames);
		}
	}
}