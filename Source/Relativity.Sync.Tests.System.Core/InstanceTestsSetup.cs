using System.Net;
using Banzai.Logging;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.System.Core
{
	/// <summary>
	///     This class sets up test environment for every test in this namespace
	/// </summary>
	[SetUpFixture]
	public class InstanceTestsSetup
	{
		protected ISyncLog Logger { get; }

		public InstanceTestsSetup()
		{
			Logger = TestLogHelper.GetLogger();
		}

		[OneTimeSetUp]
		public virtual void RunBeforeAnyTests()
		{
			SuppressCertificateCheckingIfConfigured();

			RelativityFacade.Instance.RelyOn<CoreComponent>();
			RelativityFacade.Instance.RelyOn<ApiComponent>();

			ConfigureRequiredInstanceSettings();

			OverrideBanzaiLogger();

			InstallDataTransferLegacyApplication();
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
			Logger.LogInformation("Overriding Banzai logger");
			LogWriter.SetFactory(new SyncLogWriterFactory(AppSettings.UseLogger ? (ISyncLog)new ConsoleLogger() : new EmptyLogger()));
		}

		private void ConfigureRequiredInstanceSettings()
		{
			Logger.LogInformation("Configuring instance settings.");

			IInstanceSettingsService instanceSettingsService = RelativityFacade.Instance.Resolve<IInstanceSettingsService>();

			instanceSettingsService.Require(new Testing.Framework.InstanceSetting
			{
				Name = "WebAPIPath",
				Section = "kCura.IntegrationPoints",
				ValueType = InstanceSettingValueType.Text,
				Value = AppSettings.RelativityWebApiUrl.AbsoluteUri
			});

			instanceSettingsService.Require(new Testing.Framework.InstanceSetting
			{
				Name = "AdminsCanSetPasswords",
				Section = "Relativity.Authentication",
				ValueType = InstanceSettingValueType.Text,
				Value = "True"
			});
		}

		private void InstallDataTransferLegacyApplication()
		{
			ILibraryApplicationService applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

			applicationService.InstallToLibrary(AppSettings.DataTransferLegacyRAP, new LibraryApplicationInstallOptions
			{
				CreateIfMissing = true
			});
		}
	}
}