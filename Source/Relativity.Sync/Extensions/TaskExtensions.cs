using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Extensions
{
	internal static class TaskExtensions
	{
		/// <summary>
		/// Executes a task with a specified timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the task result.</typeparam>
		/// <param name="task">The task to execute.</param>
		/// <param name="timeout">The timeout duration.</param>
		/// <returns>The result of the task.</returns>
		/// <exception cref="TimeoutException">Thrown when the operation times out.</exception>
		public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
			using (var timeoutCancellationTokenSource = new CancellationTokenSource())
			{
				var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token)).ConfigureAwait(false);
				if (completedTask == task)
				{
					timeoutCancellationTokenSource.Cancel();
					return await task.ConfigureAwait(false);  // Very important in order to propagate exceptions
				}
				else
				{
					throw new TimeoutException("The operation has timed out.");
				}
			}
		}
	}
}
