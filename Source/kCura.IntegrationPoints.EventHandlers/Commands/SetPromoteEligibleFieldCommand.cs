using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SetPromoteEligibleFieldCommand : ICommand
	{
		private readonly IDBContext _workspaceDbContext;

		public SetPromoteEligibleFieldCommand(IDBContext workspaceDbContext)
		{
			_workspaceDbContext = workspaceDbContext;
		}

		public void Execute()
		{
			UpdateIntegrationPoints();
			UpdateIntegrationPointProfiles();
		}

		private void UpdateIntegrationPoints()
		{
			_workspaceDbContext.ExecuteNonQuerySQLStatement("UPDATE [IntegrationPoint] SET [PromoteEligible] = 1 WHERE [PromoteEligible] IS NULL");
		}

		private void UpdateIntegrationPointProfiles()
		{
			_workspaceDbContext.ExecuteNonQuerySQLStatement("UPDATE [IntegrationPointProfile] SET [PromoteEligible] = 1 WHERE [PromoteEligible] IS NULL");
		}


		public string SuccessMessage => "Promote Eligible field successfully updated.";
		public string FailureMessage => "Failed to update Promote Eligible field.";
	}
}