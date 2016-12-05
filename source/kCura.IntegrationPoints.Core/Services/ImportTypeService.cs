using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using Relativity.Toggles;
using kCura.IntegrationPoints.Core.Toggles;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ImportTypeService : IImportTypeService
	{
		private readonly IToggleProvider _toggleProvider;

		public ImportTypeService(IToggleProvider toggleProvider)
		{
			_toggleProvider = toggleProvider;
		}

		public List<ImportType> GetImportTypes()
		{
			bool isShowImportProviderToggleEnabled = _toggleProvider.IsEnabled<ShowImportProviderNonDocumentTypesToggle>();

			List<ImportType> importTypes = new List<ImportType>();

			importTypes.Add(new ImportType("Document Load File", ImportType.ImportTypeValue.Document));

			if (!isShowImportProviderToggleEnabled) { return importTypes; }

			importTypes.Add(new ImportType("Image Load File", ImportType.ImportTypeValue.Image));
			importTypes.Add(new ImportType("Production Load File", ImportType.ImportTypeValue.Production));

			return importTypes;
		}
	}
}
