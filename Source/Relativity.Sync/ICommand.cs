using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface ICommand<in T> where T : IConfiguration
	{
		Task<bool> CanExecuteAsync(CancellationToken token);

		Task ExecuteAsync(CancellationToken token);
	}
}