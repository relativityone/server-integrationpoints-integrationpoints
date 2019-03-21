using System.Net;
using Banzai.Logging;
using NUnit.Framework;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.System
{
	/// <summary>
	///     This class sets up test environment for every test in this namespace
	/// </summary>
	[SetUpFixture]
	public sealed class SystemTestsSetup
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			SuppressCertificateCheckingIfConfigured();
			OverrideBanzaiLogger();
		}

		private static void SuppressCertificateCheckingIfConfigured()
		{
			if (AppSettings.SuppressCertificateCheck)
			{
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			}
		}

		private void OverrideBanzaiLogger()
		{
			LogWriter.SetFactory(new SyncLogWriterFactory(new EmptyLogger()));
		}
	}
}