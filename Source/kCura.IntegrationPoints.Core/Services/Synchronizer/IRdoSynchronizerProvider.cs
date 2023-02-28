namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
    public interface IRdoSynchronizerProvider
    {
        void CreateOrUpdateDestinationProviders();
        int GetRdoSynchronizerId();
    }
}
