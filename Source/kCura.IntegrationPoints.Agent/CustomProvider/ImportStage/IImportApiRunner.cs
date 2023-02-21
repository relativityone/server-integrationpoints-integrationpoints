using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal interface IImportApiRunner
    {
        Task RunImportJobAsync(ImportJobContext importJobContext, string destinationConfiguration, List<FieldMapWrapper> fieldMappings);
    }
}
