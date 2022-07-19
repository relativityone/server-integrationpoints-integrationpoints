using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Stubs
{
    /// <summary>
    ///     Small stub for testing classes with a dependency on <see cref="IProgressRepository"/>.
    ///     Allows injection of return values w/o all of the mocking boilerplate.
    /// </summary>
    internal sealed class ProgressRepositoryStub : IProgressRepository
    {
        public List<ProgressStub> ForCreate { get; } = new List<ProgressStub>();
        public List<ProgressStub> ForGet { get; } = new List<ProgressStub>();
        public List<ProgressStub> ForQuery { get; } = new List<ProgressStub>();

        public Task<IProgress> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, SyncJobStatus status)
        {
            ProgressStub progress = ForCreate.First();
            ForCreate.RemoveAt(0);
            progress.Name = name;
            progress.Order = order;
            progress.Status = status;
            return Task.FromResult<IProgress>(progress);
        }

        public Task<IProgress> GetAsync(int workspaceArtifactId, int artifactId)
        {
            IProgress progress = ForGet.First(x => x.ArtifactId == artifactId);
            return Task.FromResult(progress);
        }

        public Task<IReadOnlyCollection<IProgress>> QueryAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
        {
            return Task.FromResult(ForQuery as IReadOnlyCollection<IProgress>);
        }

        public Task<IProgress> QueryAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name)
        {
            IProgress progress = ForQuery.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCulture));
            return Task.FromResult(progress);
        }
    }
}
