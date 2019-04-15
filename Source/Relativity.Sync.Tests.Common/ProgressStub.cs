using System;
using System.Threading.Tasks;
using IProgress = Relativity.Sync.Storage.IProgress;

namespace Relativity.Sync.Tests.Common
{
	internal sealed class ProgressStub : IProgress
	{
		public ProgressStub() { }

		public ProgressStub(string name)
		{
			Name = name;
		}

		public int ArtifactId { get; set; }
		public string Name { get; set; }
		public int Order { get; set; }
		public string Status { get; set; }
		public string Exception { get; set; }
		public Exception ActualException { get; set; }
		public string Message { get; set; }
		public int SyncConfigurationArtifactId { get; set; }
		public Task SetStatusAsync(string status)
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
