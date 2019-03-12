using System.Net;
using NUnit.Framework;

namespace Relativity.Sync.Tests.System
{
	/// <summary>
	/// This class sets up test environment for every test in this namespace
	/// </summary>
	[SetUpFixture]
	public sealed class SystemTestsSetup
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			SuppressCertificateCheckingIfConfigured();
		}

		private static void SuppressCertificateCheckingIfConfigured()
		{
			if (AppSettings.SuppressCertificateCheck)
			{
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			}
		}
	}
}
