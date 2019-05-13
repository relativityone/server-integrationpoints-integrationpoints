using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IImportJob : IDisposable
	{
		Task RunAsync(CancellationToken token);
	}
}