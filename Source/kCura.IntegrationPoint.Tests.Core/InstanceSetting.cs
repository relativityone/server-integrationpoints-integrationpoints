using System;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using ValueType = Relativity.Services.InstanceSetting.ValueType;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class InstanceSetting
	{
		public static int Create(string section, string name, string value, ValueType valueType)
		{
			using (IInstanceSettingManager instanceSettingManager = Kepler.CreateProxy<IInstanceSettingManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true)
			)
			{
				global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting = new global::Relativity.Services.InstanceSetting.InstanceSetting
				{
					Section = section,
					Name = name,
					Value = value,
					ValueType = valueType
				};

				try
				{
					int artifactId = instanceSettingManager.CreateSingleAsync(instanceSetting).ConfigureAwait(false).GetAwaiter().GetResult();
					return artifactId;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to create Instance Setting. Exception: {ex.Message}");
				}
			}
		}

		public static global::Relativity.Services.InstanceSetting.InstanceSetting Query(string section, string name)
		{
			using (IInstanceSettingManager instanceSettingManager = Kepler.CreateProxy<IInstanceSettingManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true)
			)
			{
				Query query = new Query();
				Condition sectionCondition = new TextCondition(InstanceSettingFieldNames.Section, TextConditionEnum.EqualTo, section);
				Condition nameCondition = new TextCondition(InstanceSettingFieldNames.Name, TextConditionEnum.EqualTo, name);
				Condition queryCondition = new CompositeCondition(sectionCondition, CompositeConditionEnum.And, nameCondition);
				query.Condition = queryCondition.ToQueryString();

				try
				{
					InstanceSettingQueryResultSet instanceSettingQueryResultSet = instanceSettingManager.QueryAsync(query).GetResultsWithoutContextSync();
					global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting = instanceSettingQueryResultSet.Results.FirstOrDefault()?.Artifact;
					return instanceSetting;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to query Instance Setting. Exception: {ex.Message}");
				}
			}
		}

		public static string UpsertAndReturnOldValueIfExists(string section, string name, string value)
		{
			global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting = Query(section, name);

			if (instanceSetting == null)
			{
				Create(section, name, value, ValueType.TrueFalse);
				return value;
			}

			if (string.Equals(instanceSetting.Value, value, StringComparison.OrdinalIgnoreCase))
			{
				return value;
			}

			string oldValue = instanceSetting.Value;
			instanceSetting.Value = value;
			Update(instanceSetting);

			return oldValue;
		}

		private static void Update(global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting)
		{
			using (IInstanceSettingManager instanceSettingManager = Kepler.CreateProxy<IInstanceSettingManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true)
			)
			{
				try
				{
					instanceSettingManager.UpdateSingleAsync(instanceSetting).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to update Instance Setting. Exception: {ex.Message}");
				}
			}
		}
	}
}