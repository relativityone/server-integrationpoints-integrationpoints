namespace kCura.IntegrationPoint.Tests.Core
{
	public class Helper
	{
		public Helper()
		{
			Import = new Import(this);
			Rest = new Rest(this);
			Rsapi = new Rsapi(this);
			SavedSearch = new SavedSearch(this);
			SharedVariables = new SharedVariables();
			Status = new Status(this);
			Workspace = new Workspace(this);
		}

		public Import Import { get; }

		public Rest Rest { get; }

		public Rsapi Rsapi { get; }

		public SavedSearch SavedSearch { get; }

		public SharedVariables SharedVariables { get; }

		public Status Status { get; }

		public Workspace Workspace { get; }
	}
}