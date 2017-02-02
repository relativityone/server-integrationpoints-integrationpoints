using System;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using System.Data;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobFactory : IImportJobFactory
	{
		public IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataReader sourceData)
		{
			IJobImport rv;
			switch (GetJobContextType(settings))
			{
				case JobContextType.RelativityToRelativityImages:
					IImportSettingsBaseBuilder<ImageSettings> imageRelativityToRelativityImportSettingsBuilder = new ImageRelativityToRelativityImportSettingsBuilder(importApi);
					rv = new ImageJobImport(settings, importApi, imageRelativityToRelativityImportSettingsBuilder, sourceData);
					break;
				case JobContextType.ImportImagesFromLoadFile:
					IImportSettingsBaseBuilder<ImageSettings> imageImportSettingsBuilder = new ImageImportSettingsBuilder(importApi);
					rv = new ImageJobImport(settings, importApi, imageImportSettingsBuilder, sourceData);
					break;
				case JobContextType.Native:
					IImportSettingsBaseBuilder<Settings> nativeImportSettingsBuilder = new NativeImportSettingsBuilder(importApi);
					rv = new NativeJobImport(settings, importApi, nativeImportSettingsBuilder, sourceData);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			rv.RegisterEventHandlers();
			return rv;
		}

		enum JobContextType
		{
			RelativityToRelativityImages,
			ImportImagesFromLoadFile,
			Native
		}

		private static JobContextType GetJobContextType(ImportSettings settings)
		{
			const string relativity = "relativity";	
			if (settings.Provider == relativity && settings.ImageImport)
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
