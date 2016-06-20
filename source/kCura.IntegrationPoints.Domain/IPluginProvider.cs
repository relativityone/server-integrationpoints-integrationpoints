using System;
using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.Contracts.Domain;

namespace kCura.IntegrationPoints.Domain
{
    public interface IPluginProvider
    {
        IDictionary<ApplicationBinary, Stream> GetPluginLibraries(Guid applicationGuid);
    }
}
