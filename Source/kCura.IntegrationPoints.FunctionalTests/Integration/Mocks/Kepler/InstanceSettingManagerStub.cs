using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Queries;
using Moq;
using Relativity.Services;
using Relativity.Services.InstanceSetting;

using RipInstanceSettings = kCura.IntegrationPoints.Core.Constants.InstanceSettings;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class InstanceSettingManagerStub : KeplerStubBase<IInstanceSettingManager>
    {
        private readonly TestContext _context;

        public InstanceSettingManagerStub(TestContext context)
        {
            _context = context;
        }

        public void SetupInstanceSetting()
        {
            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.RESTRICT_REFERENTIAL_FILE_LINKS_ON_IMPORT,
                RipInstanceSettings.RELATIVITY_CORE_SECTION,
                settings => settings.RestrictReferentialFileLinksOnImport);

            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.FRIENDLY_INSTANCE_NAME,
                RipInstanceSettings.RELATIVITY_AUTHENTICATION_SECTION,
                settings => settings.FriendlyInstanceName);

            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.LONG_TEXT_LIMIT_NAME,
                RipInstanceSettings.LONG_TEXT_LIMIT_SECTION,
                settings => settings.MaximumNumberOfCharactersSupportedByLongText);

            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.ALLOW_NO_SNAPSHOT_IMPORT,
                RipInstanceSettings.RELATIVITY_CORE_SECTION,
                settings => settings.AllowNoSnapshotImport);

            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.BLOCKED_HOSTS,
                RipInstanceSettings.INTEGRATION_POINTS_SECTION,
                settings => settings.RestrictReferentialFileLinksOnImport);

            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.DRAIN_STOP_TIMEOUT,
                RipInstanceSettings.INTEGRATION_POINTS_SECTION,
                settings => ((int)settings.DrainStopTimeout.TotalSeconds).ToString());

            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.IAPI_BATCH_SIZE,
                RipInstanceSettings.INTEGRATION_POINTS_SECTION,
                settings => settings.IApiBatchSize.ToString());

            SetupInstanceSettingInternal(_context.InstanceSettings,
                RipInstanceSettings.CUSTOM_PROVIDER_BATCH_SIZE,
                RipInstanceSettings.INTEGRATION_POINTS_SECTION,
                settings => settings.CustomProviderBatchSize.ToString());
        }

        private void SetupInstanceSettingInternal(InstanceSettings settings,
            string name, string section, Expression<Func<InstanceSettings, string>> returnedValueFunc)
        {
            Mock.Setup(x => x.QueryAsync(
                It.Is<Relativity.Services.Query>(q => q.Condition ==
                                                      InstanceSettingConditionBuilder.GetCondition(name, section)), 1))
                .Returns(() =>
                {
                    var result = new InstanceSettingQueryResultSet
                    {
                        Results = new List<Result<Relativity.Services.InstanceSetting.InstanceSetting>>
                        {
                            new Result<Relativity.Services.InstanceSetting.InstanceSetting>
                            {
                                Artifact = new Relativity.Services.InstanceSetting.InstanceSetting
                                {
                                    Value = returnedValueFunc.Compile().Invoke(settings),
                                },
                            }
                        },
                        Success = true,
                    };

                    return Task.FromResult(result);
                });
        }

    }
}
