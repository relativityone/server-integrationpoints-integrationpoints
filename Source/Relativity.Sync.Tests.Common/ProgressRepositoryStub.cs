using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common
{
    /// <summary>
    ///     Small stub for testing classes with a dependency on <see cref="IProgressRepository"/>. Stores RDOs in a list.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class ProgressRepositoryStub : IProgressRepository
    {
        public List<ProgressStub> ProgressObjects { get; } = new List<ProgressStub>();

        public Task<IProgress> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, SyncJobStatus status)
        {
            var newProgressObject = new ProgressStub(name)
            {
                SyncConfigurationArtifactId = syncConfigurationArtifactId,
                Order = order,
                Status = status
            };
            ProgressObjects.Add(newProgressObject);
            return Task.FromResult<IProgress>(newProgressObject);
        }

        public Task<IProgress> GetAsync(int workspaceArtifactId, int artifactId)
        {
            IProgress progress = ProgressObjects.First(x => x.ArtifactId == artifactId);
            return Task.FromResult(progress);
        }

        public Task<IReadOnlyCollection<IProgress>> QueryAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
        {
            return Task.FromResult(ProgressObjects as IReadOnlyCollection<IProgress>);
        }

        public Task<IProgress> QueryAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name)
        {
            IProgress progress = ProgressObjects.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCulture) && x.SyncConfigurationArtifactId == syncConfigurationArtifactId);
            return Task.FromResult(progress);
        }
    }
}
