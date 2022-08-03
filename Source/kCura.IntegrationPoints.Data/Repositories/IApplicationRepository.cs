using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IApplicationRepository
    {
        IList<int> GetWorkspaceArtifactIdsWhereApplicationInstalled(Guid applicationGuid);
    }
}