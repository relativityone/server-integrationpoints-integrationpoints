using kCura.IntegrationPoints.Contracts.Syncronizer;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public interface IDataSyncronizerFactory
	{
		IDataSyncronizer GetSyncronizer();
	}
}
