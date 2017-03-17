namespace kCura.IntegrationPoints.Domain.Managers
{
	public interface IInstanceSettingsManager
	{
		string RetriveCurrentInstanceFriendlyName();

		bool RetrieveAllowNoSnapshotImport();
	}
}