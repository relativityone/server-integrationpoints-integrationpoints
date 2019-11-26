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
			ConfigureWebAPI();
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

		private static void ConfigureWebAPI()
		{
			const string name = "WebAPIPath";
			const string section = "kCura.IntegrationPoints";

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
						ValueType = ValueType.Text,
						Value = AppSettings.RelativityWebApiUrl.AbsoluteUri
					};
					settingManager.CreateSingleAsync(setting).Wait();
				}
			}
		}
	}
}