using Banzai.Logging;
using NUnit.Framework;
using Relativity.Sync.Tests.Unit.Helpers;

namespace Relativity.Sync.Tests.Unit
{
	[SetUpFixture]
	public sealed class TestsSetupClass
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// This SetUp is required because .NET Framework cannot handle dynamic binding in Banzai logger
			LogWriter.SetFactory(new LogWriterFactory());
		}
	}
}