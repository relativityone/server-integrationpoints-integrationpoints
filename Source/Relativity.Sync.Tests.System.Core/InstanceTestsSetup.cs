using System.Net;
using System.Threading.Tasks;
using Banzai.Logging;
using NUnit.Framework;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Environmental;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.System.Core.Helpers;
using System.Data.SqlClient;
using System.Security;

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
		public virtual async Task RunBeforeAnyTests()
		{
			SuppressCertificateCheckingIfConfigured();

			// REL-451648
			await SetToggleAsync("Relativity.Core.Api.TogglePerformance.ObjectManagerUseStaticStatelessValidators", false).ConfigureAwait(false);

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
			Logger.LogInformation("Overriding Banzai logger");
			LogWriter.SetFactory(new SyncLogWriterFactory(AppSettings.UseLogger ? (ISyncLog)new ConsoleLogger() : new EmptyLogger()));
		}

		private async Task ConfigureRequiredInstanceSettings()
		{
			Logger.LogInformation("Configuring instance settings");
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

		private static async Task SetToggleAsync(string toggleName, bool toggleValue)
		{
			SecureString password = new NetworkCredential("", AppSettings.SqlPassword).SecurePassword;
			password.MakeReadOnly();

			SqlCredential credential = new SqlCredential(AppSettings.SqlUsername, password);

			using (SqlConnection connection = new SqlConnection($"Data Source={AppSettings.SqlServer};Initial Catalog=EDDS", credential))
			{
				connection.Open();

				SqlCommand toggleExistsCommand = new SqlCommand(@"SELECT Count(*) FROM [EDDS].[eddsdbo].[Toggle] WHERE Name = @toggleName", connection);
				toggleExistsCommand.Parameters.AddWithValue("toggleName", toggleName);
				if ((int) await toggleExistsCommand.ExecuteScalarAsync() > 0)
				{
					SqlCommand toggleUpdateCommand = new SqlCommand(@"UPDATE [EDDS].[eddsdbo].[Toggle] SET IsEnabled = @toggleValue WHERE Name = @toggleName", connection);
					toggleUpdateCommand.Parameters.AddWithValue("toggleValue", toggleValue);
					toggleUpdateCommand.Parameters.AddWithValue("toggleName", toggleName);

					await toggleUpdateCommand.ExecuteNonQueryAsync();
				}
				else
				{
					SqlCommand toggleInsertCommand = new SqlCommand(@"INSERT INTO [EDDS].[eddsdbo].[Toggle] (Name, IsEnabled) VALUES (@toggleName, @toggleValue)", connection);
					toggleInsertCommand.Parameters.AddWithValue("toggleName", toggleName);
					toggleInsertCommand.Parameters.AddWithValue("toggleValue", toggleValue);

					await toggleInsertCommand.ExecuteNonQueryAsync();
				}
			}
		}
	}
}