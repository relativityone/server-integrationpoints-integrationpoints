using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi
{
    public class FakeImportApiFacade : IImportApiFacade
    {
        private readonly RelativityInstanceTest _instance;

        public FakeImportApiFacade(RelativityInstanceTest instance)
        {
            _instance = instance;
        }
        
        public HashSet<int> GetMappableArtifactIdsWithNotIdentifierFieldCategory(int workspaceArtifactID, int artifactTypeID)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<int, string> GetWorkspaceFieldsNames(int workspaceArtifactID, int artifactTypeID)
        {
            return _instance.Workspaces.First(x => x.ArtifactId == workspaceArtifactID).Fields
                .ToDictionary(x => x.ArtifactId, x => x.Name);
        }

        public Dictionary<int, string> GetWorkspaceNames()
        {
            return _instance.Workspaces.ToDictionary(x => x.ArtifactId, x => x.Name);
        }
    }
}