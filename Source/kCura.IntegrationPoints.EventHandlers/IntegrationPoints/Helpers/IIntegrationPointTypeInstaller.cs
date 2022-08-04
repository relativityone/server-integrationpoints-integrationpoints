using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    public interface IIntegrationPointTypeInstaller
    {
        void Install(Dictionary<Guid, string> types);
    }
}