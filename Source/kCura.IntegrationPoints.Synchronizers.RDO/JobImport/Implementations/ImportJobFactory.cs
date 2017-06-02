using System;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobFactory : IImportJobFactory
	{
		public IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataTransferContext context)
		{
			IJobImport rv;
			switch (GetJobContextType(settings))
			{
				case JobContextType.RelativityToRelativityImagesProduction:
					IImportSettingsBaseBuilder<ImageSettings> imageProductionRelativityToRelativityImportSettingsBuilder = new ImageRelativityToRelativityImportSettingsBuilder(importApi);
					rv = new ProductionImageJobImport(settings, importApi, imageProductionRelativityToRelativityImportSettingsBuilder, context);
					break;
				case JobContextType.RelativityToRelativityImages:
					IImportSettingsBaseBuilder<ImageSettings> imageRelativityToRelativityImportSettingsBuilder = new ImageRelativityToRelativityImportSettingsBuilder(importApi);
					rv = new ImageJobImport(settings, importApi, imageRelativityToRelativityImportSettingsBuilder, context);
					break;
				case JobContextType.ImportImagesFromLoadFile:
					IImportSettingsBaseBuilder<ImageSettings> imageImportSettingsBuilder = new ImageImportSettingsBuilder(importApi);
					rv = new ImageJobImport(settings, importApi, imageImportSettingsBuilder, context);
					break;
				case JobContextType.Native:
					IImportSettingsBaseBuilder<Settings> nativeImportSettingsBuilder = new NativeImportSettingsBuilder(importApi);
					rv = new NativeJobImport(settings, importApi, nativeImportSettingsBuilder, context);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			rv.RegisterEventHandlers();
			return rv;
		}

		internal enum JobContextType
		{
			RelativityToRelativityImages,
			RelativityToRelativityImagesProduction,
			ImportImagesFromLoadFile,
			Native
		}

		internal static JobContextType GetJobContextType(ImportSettings settings)
		{
			const string relativity = "relativity";
			if (relativity == settings.Provider && settings.ProductionImport && settings.ImageImport)
			{
				return JobContextType.RelativityToRelativityImagesProduction;
			}
			else if (relativity == settings.Provider && settings.ImageImport)
			{
				return JobContextType.RelativityToRelativityImages;
			}
			else if (settings.ImageImport)
			{
				return JobContextType.ImportImagesFromLoadFile;
			}
			else
			{
				return JobContextType.Native;
			}
		}
	}
}
