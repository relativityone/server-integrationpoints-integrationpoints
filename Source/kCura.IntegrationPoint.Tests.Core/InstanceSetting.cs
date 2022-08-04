using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using ValueType = Relativity.Services.InstanceSetting.ValueType;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class InstanceSetting
    {
        private static ITestHelper Helper => new TestHelper();

        public static async Task<global::Relativity.Services.InstanceSetting.InstanceSetting> QueryAsync(string section, string name)
        {
            using (IInstanceSettingManager instanceSettingManager = Helper.CreateProxy<IInstanceSettingManager>())
            {
                Query query = new Query();
                Condition sectionCondition = new TextCondition(InstanceSettingFieldNames.Section, TextConditionEnum.EqualTo, section);
                Condition nameCondition = new TextCondition(InstanceSettingFieldNames.Name, TextConditionEnum.EqualTo, name);
                Condition queryCondition = new CompositeCondition(sectionCondition, CompositeConditionEnum.And, nameCondition);
                query.Condition = queryCondition.ToQueryString();

                try
                {
                    InstanceSettingQueryResultSet instanceSettingQueryResultSet =
                        await instanceSettingManager.QueryAsync(query).ConfigureAwait(false);

                    return instanceSettingQueryResultSet.Results.FirstOrDefault()?.Artifact;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error: Failed to query Instance Setting. Exception: {ex.Message}", ex);
                }
            }
        }

        public static async Task<bool> CreateOrUpdateAsync(string section, string name, string value, ValueType type = ValueType.Text)
        {
            Console.WriteLine($"Updating Instance Setting - Name: {name}, Section: {section} with Value: {value}");
            global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting =
                await QueryAsync(section, name).ConfigureAwait(false);

            if (instanceSetting == null)
            {
                Console.WriteLine($"Instance Setting does not exist, create new one");
                await CreateAsync(section, name, value, type).ConfigureAwait(false);
            }
            else
            {
                instanceSetting.Value = value;
                await UpdateAsync(instanceSetting).ConfigureAwait(false);
            }

            var instanceSettingUpdated = await QueryAsync(section, name).ConfigureAwait(false);

            return instanceSettingUpdated.Value == value;
        }

        private static async Task<int> CreateAsync(string section, string name, string value, ValueType valueType)
        {
            using (IInstanceSettingManager instanceSettingManager = Helper.CreateProxy<IInstanceSettingManager>())
            {
                global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting =
                    new global::Relativity.Services.InstanceSetting.InstanceSetting
                    {
                        Section = section,
                        Name = name,
                        Value = value,
                        ValueType = valueType
                    };

                try
                {
                    return await instanceSettingManager.CreateSingleAsync(instanceSetting).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error: Failed to create Instance Setting. Exception: {ex.Message}", ex);
                }
            }
        }

        private static async Task UpdateAsync(global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting)
        {
            using (IInstanceSettingManager instanceSettingManager = Helper.CreateProxy<IInstanceSettingManager>())
            {
                try
                {
                    await instanceSettingManager.UpdateSingleAsync(instanceSetting).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error: Failed to update Instance Setting. Exception: {ex.Message}", ex);
                }
            }
        }
    }
}