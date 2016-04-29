namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public interface IRdoSynchronizerProvider
	{
		void CreateOrUpdateLdapSourceType();
		int GetRdoSynchronizerId();
	}
}