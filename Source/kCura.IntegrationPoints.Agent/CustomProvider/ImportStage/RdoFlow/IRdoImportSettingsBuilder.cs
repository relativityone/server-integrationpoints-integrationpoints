using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal interface IRdoImportSettingsBuilder
    {
        Task<RdoImportConfiguration> BuildAsync(string destinationConfiguration, List<FieldMapWrapper> fieldMappings);
    }
}
