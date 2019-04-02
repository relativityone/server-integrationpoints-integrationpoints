using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IProgress
	{
		int ArtifactId { get; }
		string Name { get; }
		int Order { get; }
		string Status { get; }
		string Exception { get; }
		string Message { get; }
		Task SetStatusAsync(string status);
		Task SetExceptionAsync(Exception exception);
		Task SetMessageAsync(string message);
	}
}