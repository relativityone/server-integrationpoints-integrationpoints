using System.Net;
using System.Threading.Tasks;
using Banzai.Logging;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InstanceSetting;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;

namespace Relativity.Sync.Tests.System.Core
{
    /// <summary>
    ///     This class sets up test environment for every test in this namespace
    /// </summary>
    [SetUpFixture]
    public class InstanceTestsSetup
    {
        protected IAPILog Logger { get; }

        public InstanceTestsSetup()
        {
            Logger = TestLogHelper.GetLogger();
        }

        [OneTimeSetUp]
        public virtual async Task RunBeforeAnyTests()
        {
            SuppressCertificateCheckingIfConfigured();

            await ConfigureRequiredInstanceSettingsAsync().ConfigureAwait(false);

            OverrideBanzaiLogger();

            RelativityFacade.Instance.RelyOn<CoreComponent>();
            RelativityFacade.Instance.RelyOn<ApiComponent>();

            // InstallDataTransferLegacy();

            ConfigureRelativityInstanceURL();
        }

        private void ConfigureRelativityInstanceURL()
        {
            RelativityFacade.Instance.Resolve<IInstanceSettingsService>().UpdateValue("RelativityInstanceURL", "Relativity.Core", AppSettings.RelativityUrl.AbsoluteUri);
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
            LogWriter.SetFactory(new SyncLogWriterFactory(AppSettings.UseLogger ? (IAPILog)new ConsoleLogger() : new EmptyLogger()));
        }

        private async Task ConfigureRequiredInstanceSettingsAsync()
        {
            Logger.LogInformation("Configuring instance settings");
            await CreateInstanceSettingIfNotExistAsync("AdminsCanSetPasswords", "Relativity.Authentication", ValueType.TrueFalse, "True").ConfigureAwait(false);
        }

        private static async Task CreateInstanceSettingIfNotExistAsync(string name, string section, ValueType valueType, string value)
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
