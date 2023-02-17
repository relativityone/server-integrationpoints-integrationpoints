using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IImportTypeService
    {
        List<ImportType> GetImportTypes(bool isRdo);
    }
}
