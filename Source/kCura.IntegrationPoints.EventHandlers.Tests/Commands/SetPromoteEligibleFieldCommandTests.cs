using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.Commands;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class SetPromoteEligibleFieldCommandTests : TestBase
	{
		private IDBContext _dbContext;
		private SetPromoteEligibleFieldCommand _command;

		public override void SetUp()
		{
			_dbContext = Substitute.For<IDBContext>();

			_command = new SetPromoteEligibleFieldCommand(_dbContext);
		}

		[Test]
		public void GoldWorkflow()
		{
			_command.Execute();

			_dbContext.Received(1).ExecuteNonQuerySQLStatement("UPDATE [IntegrationPoint] SET [PromoteEligible] = 1 WHERE [PromoteEligible] IS NULL");
			_dbContext.Received(1).ExecuteNonQuerySQLStatement("UPDATE [IntegrationPointProfile] SET [PromoteEligible] = 1 WHERE [PromoteEligible] IS NULL");
		}
	}
}