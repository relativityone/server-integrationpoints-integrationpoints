using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    internal interface IChoiceTreeToStringConverter
    {
        string ConvertTreeToString(IList<ChoiceWithChildInfoDto> choiceTree);
    }
}
