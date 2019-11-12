using System;
using System.Collections.Generic;
using System.IO;

namespace kCura.IntegrationPoints.Domain
{
    public interface IPluginProvider
    {
        IDictionary<ApplicationBinary, Stream> GetPluginLibraries(Guid applicationGuid);
    }
}
