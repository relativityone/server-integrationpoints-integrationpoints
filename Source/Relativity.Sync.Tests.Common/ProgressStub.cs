using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common
{
    [ExcludeFromCodeCoverage]

    internal sealed class ProgressStub : IProgress
    {
        public ProgressStub()
        {
        }

        public ProgressStub(string name)
        {
            Name = name;
        }

        public int ArtifactId { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public SyncJobStatus Status { get; set; }

        public string Exception { get; set; }

        public Exception ActualException { get; set; }

        public string Message { get; set; }

        public int SyncConfigurationArtifactId { get; set; }

        public Task SetStatusAsync(SyncJobStatus status)
        {
            Status = status;
            return Task.CompletedTask;
        }

        public Task SetExceptionAsync(Exception exception)
        {
            ActualException = exception;
            return Task.CompletedTask;
        }

        public Task SetMessageAsync(string message)
        {
            Message = message;
            return Task.CompletedTask;
        }
    }
}
