using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobFactory : IImportJobFactory
	{
		public IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataReader sourceData)
		{
			IJobImport rv;
			if (settings.ImageImport)
			{
				IImportSettingsBaseBuilder<ImageSettings> builder = new ImageImportSettingsBuilder(importApi);
				rv = new ImageJobImport(settings, importApi, builder, sourceData);
			}
			else
			{
				IImportSettingsBaseBuilder<Settings> builder = new NativeImportSettingsBuilder(importApi);
				rv = new NativeJobImport(settings, importApi, builder, sourceData);
			}
			rv.RegisterEventHandlers();
			return rv;
		}
	}
}
