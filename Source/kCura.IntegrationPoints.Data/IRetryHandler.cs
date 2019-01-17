using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data
{
	internal interface IRetryHandler
	{
		T ExecuteWithRetries<T>(Func<Task<T>> function, [CallerMemberName] string callerName = "");

		Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> function, [CallerMemberName] string callerName = "");
	}
}
