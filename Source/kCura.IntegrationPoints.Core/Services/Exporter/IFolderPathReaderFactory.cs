using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public interface IFolderPathReaderFactory
	{
		IFolderPathReader Create(IDBContext dbContext, bool useDynamicFolderPath);
	}
}