using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal interface IDocumentImportSettingsBuilder
    {
        Task<DocumentImportConfiguration> BuildAsync(string destinationConfiguration, List<FieldMapWrapper> fieldMappings);
    }
}
