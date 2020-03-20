using System.Net;
using System.Threading.Tasks;
using Banzai.Logging;
using NUnit.Framework;
using Relativity.Services.InstanceSetting;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.System.Core
{
	/// <summary>
	///     This class sets up test environment for every test in this namespace
	/// </summary>
	[SetUpFixture]
	public sealed class SystemTestsSetup
	{
		[OneTimeSetUp]
		public async Task RunBeforeAnyTests()
		{
			SuppressCertificateCheckingIfConfigured();
			await ConfigureRequiredInstanceSettings().ConfigureAwait(false);
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

		private static async Task ConfigureRequiredInstanceSettings()
		{
			await CreateInstanceSettingIfNotExist("WebAPIPath", "kCura.IntegrationPoints", ValueType.Text, AppSettings.RelativityWebApiUrl.AbsoluteUri).ConfigureAwait(false);
			await CreateInstanceSettingIfNotExist("AdminsCanSetPasswords", "Relativity.Authentication", ValueType.TrueFalse, "True").ConfigureAwait(false);
		}

		private static async Task CreateInstanceSettingIfNotExist(string name, string section, ValueType valueType, string value)
		{
			ServiceFactory serviceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
			using (IInstanceSettingManager settingManager = serviceFactory.CreateProxy<IInstanceSettingManager>())
			{
				Services.Query query = new Services.Query
				{
					Condition = $"'Name' == '{name}' AND 'Section' == '{section}'"
				};
				InstanceSettingQueryResultSet settingResult = await settingManager.QueryAsync(query).ConfigureAwait(false);

				if (settingResult.TotalCount == 0)
				{
					Services.InstanceSetting.InstanceSetting setting = new Services.InstanceSetting.InstanceSetting()
					{
						Name = name,
						Section = section,
						ValueType = valueType,
						Value = value
					};
					await settingManager.CreateSingleAsync(setting).ConfigureAwait(false);
				}
			}
		}
	}
}