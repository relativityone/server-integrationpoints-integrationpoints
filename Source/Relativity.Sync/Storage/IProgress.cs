using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
    internal interface IProgress
    {
        int ArtifactId { get; }
        string Exception { get; }
        string Message { get; }
        string Name { get; }
        int Order { get; }
        SyncJobStatus Status { get; }

        Task SetExceptionAsync(Exception exception);
        Task SetMessageAsync(string message);
        Task SetStatusAsync(SyncJobStatus status);
    }
}