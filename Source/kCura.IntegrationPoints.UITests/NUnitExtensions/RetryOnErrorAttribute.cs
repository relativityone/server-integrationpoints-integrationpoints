using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using ILogger = Serilog.ILogger;


namespace kCura.IntegrationPoints.UITests.NUnitExtensions
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class RetryOnErrorAttribute : PropertyAttribute, IWrapSetUpTearDown
	{
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(RetryOnErrorAttribute));

		private readonly int _count;

		/// <inheritdoc />
		/// <summary>
		/// Construct a RetryOnErrorAttribute
		/// </summary>
		public RetryOnErrorAttribute() : base(GetRetryOnErrorCount)
		{
			_count = GetRetryOnErrorCount;
		}

		private static int GetRetryOnErrorCount => SharedVariables.UiTestRepeatOnErrorCount;

	#region IWrapSetUpTearDown Members

		/// <inheritdoc />
		/// <summary>
		/// Wrap a command and return the result.
		/// </summary>
		/// <param name="command">The command to be wrapped</param>
		/// <returns>The wrapped command</returns>
		public TestCommand Wrap(TestCommand command)
		{
			return new RetryCommand(command, _count);
		}

		#endregion

		#region Nested RetryCommand Class

		/// <summary>
		/// The test command for the RetryOnErrorAttribute
		/// </summary>
		public class RetryCommand : DelegatingTestCommand
		{
			private readonly int _retryCount;

			/// <inheritdoc />
			/// <summary>
			/// Initializes a new instance of the <see cref="T:kCura.IntegrationPoints.UITests.NUnitExtensions.RetryOnErrorAttribute.RetryCommand" /> class.
			/// </summary>
			/// <param name="innerCommand">The inner command.</param>
			/// <param name="retryCount">The number of repetitions</param>
			public RetryCommand(TestCommand innerCommand, int retryCount)
				: base(innerCommand)
			{
				_retryCount = retryCount;
			}

			/// <inheritdoc />
			/// <summary>
			/// Runs the test, saving a TestResult in the supplied TestExecutionContext.
			/// </summary>
			/// <param name="context">The context in which the test should run.</param>
			/// <returns>A TestResult</returns>
			public override TestResult Execute(TestExecutionContext context)
			{
				int count = _retryCount;

				while (count-- > 0)
				{
					context.CurrentResult = innerCommand.Execute(context);

					if (!TestFailedOrErrorOccured(context.CurrentResult.ResultState))
					{
						break;
					}

					LogTestInfo(_retryCount - count);
				}

				return context.CurrentResult;
			}

			private static bool TestFailedOrErrorOccured(ResultState state)
			{
				return state.Equals(ResultState.Error)
					|| state.Equals(ResultState.Failure)
					|| state.Equals(ResultState.ChildFailure);
			}

			private static void LogTestInfo(int retryNumber)
			{
				TestContext tc = TestContext.CurrentContext;
				Log.Error("Test {TestName} failed for the {RetryNumber} time.\nStatus: {TestStatus}\n" +
					"Message: {ErrorMessage}\nStacktrace:\n{TestStacktrace}",
					tc.Test.FullName, retryNumber, tc.Result.Outcome.Status, tc.Result.Message, tc.Result.StackTrace);
			}
		}

		#endregion
	}
}
