using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Interfaces
{
	internal interface IRetryHandler
	{
		Task<T> ExecuteWithRetriesAsync<T>(
			Func<Task<T>> function, 
			[CallerMemberName] string callerName = ""
		);

		Task ExecuteWithRetriesAsync(
			Func<Task> function,
			[CallerMemberName] string callerName = ""
		);
	}
}
