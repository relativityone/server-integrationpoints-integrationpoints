using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using ILogger = Serilog.ILogger;

namespace kCura.IntegrationPoints.UITests.NUnitExtensions
{
	/// <summary>
	/// The test command for the RetryOnErrorAttribute
	/// </summary>
	internal class RetryCommand : DelegatingTestCommand
	{
		private readonly int _maximumNumberOfRepeats;
		private readonly ILogger _logger;

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:kCura.IntegrationPoints.UITests.NUnitExtensions.RetryCommand" /> class.
		/// </summary>
		/// <param name="innerCommand">The inner command.</param>
		/// <param name="logger">logger</param>
		/// <param name="maximumNumberOfRepeats">The number of repetitions</param>
		public RetryCommand(
			TestCommand innerCommand,
			ILogger logger,
			int maximumNumberOfRepeats)
			: base(innerCommand)
		{
			_logger = logger.ForContext<RetryCommand>();
			_maximumNumberOfRepeats = maximumNumberOfRepeats;
		}

		/// <inheritdoc />
		/// <summary>
		/// Runs the test, saving a TestResult in the supplied TestExecutionContext.
		/// </summary>
		/// <param name="context">The context in which the test should run.</param>
		/// <returns>A TestResult</returns>
		public override TestResult Execute(TestExecutionContext context)
		{
			for (int currentRepeat = 1; currentRepeat <= _maximumNumberOfRepeats; currentRepeat++)
			{
				context.CurrentResult = innerCommand.Execute(context);

				if (!HasErrorOccured(context.CurrentResult.ResultState))
				{
					break; // test succeeded
				}

				LogTestFailed(currentRepeat);

				if (!IsErrorRetriable(context.CurrentResult.Message))
				{
					LogNonRetriableError();
					break; // non retriable error
				}

				LogRetrying(currentRepeat);
			}

			return context.CurrentResult;
		}

		private static bool HasErrorOccured(ResultState state)
		{
			ResultState[] errorResultStates =
			{
				ResultState.Error,
				ResultState.Failure,
				ResultState.ChildFailure
			};

			return errorResultStates.Contains(state);
		}

		private static bool IsErrorRetriable(string errorMessage)
		{
			return IsSeleniumExceptionMessage(errorMessage) || IsPollyTimeoutExceptionMessage(errorMessage);
		}

		private static bool IsSeleniumExceptionMessage(string exceptionMessage)
		{
			string seleniumNamespace = typeof(OpenQA.Selenium.IWebDriver).Namespace;
			return exceptionMessage.Contains(seleniumNamespace);
		}

		private static bool IsPollyTimeoutExceptionMessage(string exceptionMessage)
		{
			string actionTimeout = typeof(Polly.Timeout.TimeoutRejectedException).Namespace;
			return exceptionMessage.Contains(actionTimeout);
		}

		private void LogTestFailed(int retryNumber)
		{
			TestContext tc = TestContext.CurrentContext;
			_logger.Error("Test {TestName} failed for the {RetryNumber} time.\nStatus: {TestStatus}\n" +
				"Message: {ErrorMessage}\nStacktrace:\n{TestStacktrace}",
				tc.Test.FullName, retryNumber, tc.Result.Outcome.Status, tc.Result.Message, tc.Result.StackTrace);
		}

		private void LogNonRetriableError()
		{
			_logger.Error("Test failed - encountered error is non-retriable");
		}

		private void LogRetrying(int currentRepeat)
		{
			_logger.Information(
				"Test failed - attempting to retry. Repeat {currentRepeat} out of {maximumNumberOfRepeats}",
				currentRepeat,
				_maximumNumberOfRepeats);
		}
	}
}
