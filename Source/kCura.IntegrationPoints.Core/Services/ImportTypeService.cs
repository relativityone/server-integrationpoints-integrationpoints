using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public class ImportTypeService : IImportTypeService
    {
        public List<ImportType> GetImportTypes(bool isRdo)
        {
            List<ImportType> importTypes = new List<ImportType>();

            importTypes.Add(new ImportType("Document Load File", ImportType.ImportTypeValue.Document));

            if (isRdo) { return importTypes; }

            importTypes.Add(new ImportType("Image Load File", ImportType.ImportTypeValue.Image));
            importTypes.Add(new ImportType("Production Load File", ImportType.ImportTypeValue.Production));

            return importTypes;
        }
    }
}
