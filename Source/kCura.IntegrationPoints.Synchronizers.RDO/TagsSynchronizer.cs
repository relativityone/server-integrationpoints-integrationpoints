using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class TagsSynchronizer : IDataSynchronizer
	{
		private readonly IDataSynchronizer _rdoSynchronizer;

		public TagsSynchronizer(IDataSynchronizer rdoSynchronizer)
		{
			_rdoSynchronizer = rdoSynchronizer;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			var updatedOptions = UpdateImportSettingsForTagging(options);
			return _rdoSynchronizer.GetFields(updatedOptions);
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			var updatedOptions = UpdateImportSettingsForTagging(options);
			_rdoSynchronizer.SyncData(data, fieldMap, updatedOptions);
		}

		public void SyncData(IDataTransferContext data, IEnumerable<FieldMap> fieldMap, string options)
		{
			var updatedOptions = UpdateImportSettingsForTagging(options);
			_rdoSynchronizer.SyncData(data, fieldMap, updatedOptions);
		}

		private string UpdateImportSettingsForTagging(string currentOptions)
		{
			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(currentOptions);
			importSettings.ProductionImport = false;
			importSettings.ImageImport = false;
			importSettings.UseDynamicFolderPath = false;
		    importSettings.ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles;
            return JsonConvert.SerializeObject(importSettings);
		}
	}
}