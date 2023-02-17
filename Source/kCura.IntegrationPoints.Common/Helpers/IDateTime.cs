using System;

namespace kCura.IntegrationPoints.Common.Helpers
{
    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }
}
