using System;
using System.Text;

namespace kCura.IntegrationPoints.Domain.Extensions
{
	/// <summary>
	/// TODO : move this class to a shared project - SAMO 7/19/2016.
	/// </summary>
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Returns a flattened string which represents the given exception instance.
		/// </summary>
		/// <param name="exception">A main exception object to use this method</param>
		public static string FlattenErrorMessages(this Exception exception)
		{
			if (exception == null)
			{
				return String.Empty;
			}

			var aggregateException = exception as AggregateException;
			var stringBuilder = new StringBuilder();
			bool isAggregateExceptionWithInnerExceptions = aggregateException?.InnerExceptions != null;

			stringBuilder.AppendLine(exception.Message);
			stringBuilder.AppendLine(exception.StackTrace);

			if (isAggregateExceptionWithInnerExceptions)
			{
				for (int i = 0; i < aggregateException.InnerExceptions.Count; i++)
				{
					int innerExceptionId = i + 1;
					stringBuilder.AppendLine($"Inner Exception {innerExceptionId}:");
					stringBuilder.AppendLine(aggregateException.InnerExceptions[i].FlattenErrorMessages());
				}
			}
			else if (exception.InnerException != null)
			{
				stringBuilder.AppendLine("Inner Exception:");
				stringBuilder.AppendLine(exception.InnerException.FlattenErrorMessages());
			}
			return stringBuilder.ToString();
		}
	}
}