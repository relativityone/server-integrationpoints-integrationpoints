using System.Net;
using Banzai.Logging;
using NUnit.Framework;
using Relativity.Services.InstanceSetting;
using Relativity.Services.ServiceProxy;
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
			ConfigureRequiredInstanceSettings();
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

		private static void ConfigureRequiredInstanceSettings()
		{
			CreateInstanceSettingIfNotExist("WebAPIPath", "kCura.IntegrationPoints", ValueType.Text, AppSettings.RelativityWebApiUrl.AbsoluteUri);
			CreateInstanceSettingIfNotExist("AdminsCanSetPasswords", "Relativity.Authentication", ValueType.TrueFalse, "True");
		}

		private static void CreateInstanceSettingIfNotExist(string name, string section, ValueType valueType, string value)
		{
			ServiceFactory serviceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
			using (IInstanceSettingManager settingManager = serviceFactory.CreateProxy<IInstanceSettingManager>())
			{
				Services.Query query = new Services.Query
				{
					Condition = $"'Name' == '{name}' AND 'Section' == '{section}'"
				};
				InstanceSettingQueryResultSet settingResult = settingManager.QueryAsync(query).Result;

				if (settingResult.TotalCount == 0)
				{
					Services.InstanceSetting.InstanceSetting setting = new Services.InstanceSetting.InstanceSetting()
					{
						Name = name,
						Section = section,
						ValueType = valueType,
						Value = value
					};
					settingManager.CreateSingleAsync(setting).Wait();
				}
			}
		}
	}
}