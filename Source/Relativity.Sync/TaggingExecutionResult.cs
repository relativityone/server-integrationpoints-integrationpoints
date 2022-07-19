using System;
using System.Linq;

namespace Relativity.Sync
{
    internal sealed class TaggingExecutionResult : ExecutionResult
    {
        public int TaggedDocumentsCount { get; set; }

        internal TaggingExecutionResult(ExecutionStatus status, string message, Exception exception) : base(status, message, exception)
        {
        }

        /// <summary>
        /// Creates a <see cref="TaggingExecutionResult"/> for a failed tagging operation with the given message and exception.
        /// </summary>
        public new static TaggingExecutionResult Failure(string message, Exception exception)
            => new TaggingExecutionResult(ExecutionStatus.Failed, message, exception);

        /// <summary>
        /// Creates a <see cref="TaggingExecutionResult"/> for a successful operation.
        /// </summary>
        public new static TaggingExecutionResult Success()
            => new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, null);


        public static TaggingExecutionResult Compose(params TaggingExecutionResult[] executionResults)
        {
            var composedResult = TaggingExecutionResult.Success();
            
            if (executionResults.Any(x => x.Status != ExecutionStatus.Completed))
            {
                var notCompletedResults = executionResults.Where(x => x.Status != ExecutionStatus.Completed).ToList();

                var composedMessage = $"Messages of tagging that not completed successfully: {string.Join("\n", notCompletedResults.Select( m => m.Message))}";
                Exception composedException = new AggregateException(notCompletedResults.Select(m => m.Exception));
                composedResult = TaggingExecutionResult.Failure(composedMessage, composedException);
            }
            
            composedResult.TaggedDocumentsCount = executionResults.Min( x => x.TaggedDocumentsCount);

            return composedResult;
        }
    }
}
