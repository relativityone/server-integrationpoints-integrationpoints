using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class TaskExtension
	{
		/// <summary>
		/// Get task results without marshaling the original context.
		/// </summary>
		/// <param name="task">the task to retrieve its result from</param>
		/// <returns>The result of type T</returns>
		public static T GetResultsWithoutContextSync<T>(this Task<T> task)
		{
			ConfiguredTaskAwaitable<T> awaitableTask = task.ConfigureAwait(false);
			ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter awaiter = awaitableTask.GetAwaiter();
			T results = awaiter.GetResult();
			return results;
		}
	}
}