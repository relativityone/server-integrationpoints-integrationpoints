using System;
using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.ArtifactGuid;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class KeplerArtifactGuidRepository : IArtifactGuidRepository
    {
        private readonly int _workspaceArtifactId;
        private readonly IServicesMgr _servicesMgr;

        public KeplerArtifactGuidRepository(int workspaceArtifactId, IServicesMgr servicesMgr)
        {
            _workspaceArtifactId = workspaceArtifactId;
            _servicesMgr = servicesMgr;
        }

        public void InsertArtifactGuidForArtifactId(int artifactId, Guid guid)
        {
            using (var artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                artifactGuidManager.CreateSingleAsync(_workspaceArtifactId, artifactId, new List<Guid> {guid}).Wait();
            }
        }

        public void InsertArtifactGuidsForArtifactIds(IDictionary<Guid, int> guidToIdDictionary)
        {
            using (var artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                foreach (var keyValuePair in guidToIdDictionary)
                {
                    artifactGuidManager.CreateSingleAsync(_workspaceArtifactId, keyValuePair.Value, new List<Guid> {keyValuePair.Key}).Wait();
                }
            }
        }

        public IDictionary<Guid, bool> GuidsExist(IEnumerable<Guid> guids)
        {
            var result = new Dictionary<Guid, bool>();
            using (var artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                foreach (var guid in guids)
                {
                    result[guid] = artifactGuidManager.GuidExistsAsync(_workspaceArtifactId, guid).GetAwaiter().GetResult();
                }
            }
            return result;
        }

        public bool GuidExists(Guid guid)
        {
            using (var artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                return artifactGuidManager.GuidExistsAsync(_workspaceArtifactId, guid).GetAwaiter().GetResult();
            }
        }

        public Dictionary<int, Guid> GetGuidsForArtifactIds(IEnumerable<int> artifactIds)
        {
            var result = new Dictionary<int, Guid>();
            using (var artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                foreach (var artifactId in artifactIds)
                {
                    var guids = artifactGuidManager.ReadSingleGuidsAsync(_workspaceArtifactId, artifactId).GetAwaiter().GetResult();
                    if (guids.Count != 0)
                    {
                        result[artifactId] = guids[0];
                    }
                }
            }
            return result;
        }

        public Dictionary<Guid, int> GetArtifactIdsForGuids(IEnumerable<Guid> guids)
        {
            var result = new Dictionary<Guid, int>();
            using (var artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                foreach (var guid in guids)
                {
                    result[guid] = artifactGuidManager.ReadSingleArtifactIdAsync(_workspaceArtifactId, guid).GetAwaiter().GetResult();
                }
            }
            return result;
        }
    }
}
