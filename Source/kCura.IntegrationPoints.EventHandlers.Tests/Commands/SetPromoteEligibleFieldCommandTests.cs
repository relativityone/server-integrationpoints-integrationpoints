using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class SetPromoteEligibleFieldCommandTests : TestBase
	{
		private IRSAPIService _rsapiService;
		private SetPromoteEligibleFieldCommand _command;

		public override void SetUp()
		{
			_rsapiService = Substitute.For<IRSAPIService>();

			_command = new SetPromoteEligibleFieldCommand(_rsapiService);
		}

		[Test]
		public void GoldWorkflow()
		{
			var integrationPoints = new List<Data.IntegrationPoint>
			{
				new Data.IntegrationPoint
				{
					PromoteEligible = false
				}
			};
			var integrationPointProfiles = new List<IntegrationPointProfile>
			{
				new IntegrationPointProfile
				{
					PromoteEligible = false
				}
			};

			_rsapiService.GetGenericLibrary<Data.IntegrationPoint>().Query(Arg.Any<Query<RDO>>()).Returns(integrationPoints);
			_rsapiService.GetGenericLibrary<IntegrationPointProfile>().Query(Arg.Any<Query<RDO>>()).Returns(integrationPointProfiles);

			_command.Execute();

			_rsapiService.GetGenericLibrary<Data.IntegrationPoint>().Received(1).Query(Arg.Any<Query<RDO>>());
			_rsapiService.GetGenericLibrary<IntegrationPointProfile>().Received(1).Query(Arg.Any<Query<RDO>>());

			_rsapiService.GetGenericLibrary<Data.IntegrationPoint>().Received(1).Update(Arg.Is<IEnumerable<Data.IntegrationPoint>>(x => x.All(y => y.PromoteEligible.Value)));
			_rsapiService.GetGenericLibrary<IntegrationPointProfile>().Received(1).Update(Arg.Is<IEnumerable<IntegrationPointProfile>>(x => x.All(y => y.PromoteEligible.Value)));
		}
	}
}