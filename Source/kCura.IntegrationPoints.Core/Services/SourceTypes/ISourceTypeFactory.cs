using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
    public interface ISourceTypeFactory
    {
        IEnumerable<SourceType> GetSourceTypes();
    }
}
