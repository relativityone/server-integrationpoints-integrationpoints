using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;
using System;
using ILogger = Serilog.ILogger;

namespace kCura.IntegrationPoints.UITests.NUnitExtensions
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class RetryOnErrorAttribute : PropertyAttribute, IWrapSetUpTearDown
	{
		private readonly int _maximumNumberOfRepeats;
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(RetryOnErrorAttribute));

		/// <inheritdoc />
		/// <summary>
		/// Construct a RetryOnErrorAttribute
		/// </summary>
		public RetryOnErrorAttribute() : base(GetRepeatOnErrorCount)
		{
			_maximumNumberOfRepeats = GetRepeatOnErrorCount;
		}

		private static int GetRepeatOnErrorCount => SharedVariables.UiTestRepeatOnErrorCount;

		/// <inheritdoc />
		/// <summary>
		/// Wrap a command and return the result.
		/// </summary>
		/// <param name="command">The command to be wrapped</param>
		/// <returns>The wrapped command</returns>
		public TestCommand Wrap(TestCommand command)
		{
			return new RetryCommand(command, Log, _maximumNumberOfRepeats);
		}
	}
}
