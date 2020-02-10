﻿using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Extensions;
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

		public static async Task CreateOrUpdateAsync(string section, string name, string value)
		{
			global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting =
				await QueryAsync(section, name).ConfigureAwait(false);

			if (instanceSetting == null)
			{
				await CreateAsync(section, name, value, ValueType.TrueFalse).ConfigureAwait(false);
			}
			else
			{
				instanceSetting.Value = value;
				await UpdateAsync(instanceSetting).ConfigureAwait(false);
			}
		}

		private static Task<int> CreateAsync(string section, string name, string value, ValueType valueType)
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
					return instanceSettingManager.CreateSingleAsync(instanceSetting);
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to create Instance Setting. Exception: {ex.Message}", ex);
				}
			}
		}

		private static Task UpdateAsync(global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting)
		{
			using (IInstanceSettingManager instanceSettingManager = Helper.CreateProxy<IInstanceSettingManager>())
			{
				try
				{
					return instanceSettingManager.UpdateSingleAsync(instanceSetting);
				}
				catch (Exception ex)
				{
					throw new Exception($"Error: Failed to update Instance Setting. Exception: {ex.Message}", ex);
				}
			}
		}
	}
}