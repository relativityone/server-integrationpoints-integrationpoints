using System.Threading.Tasks;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using NUnit.Framework;

namespace Relativity.Sync.Tests.System.GoldFlows
{
	[TestFixture]
	public class ImageGoldFlowTests : SystemTest
	{
		private GoldFlowTestSuite _goldFlowTestSuite;

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory, Dataset.Images)
				.ConfigureAwait(false);
		}
	}
}
