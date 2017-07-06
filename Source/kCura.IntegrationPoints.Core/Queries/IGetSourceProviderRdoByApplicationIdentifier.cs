using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Queries
{
    public interface IGetSourceProviderRdoByApplicationIdentifier
    {
        List<SourceProvider> Execute(Guid appGuid);
    }
}