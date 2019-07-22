using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class FolderPathReaderFactory : IFolderPathReaderFactory
	{
		public IFolderPathReader Create(IDBContext dbContext, bool useDynamicFolderPath)
		{
			if (useDynamicFolderPath)
			{
				return new DynamicFolderPathReader(dbContext);
			}
			return new EmptyFolderPathReader();
		}
	}
}