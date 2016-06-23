using System;
using Relativity.Services.InstanceSetting;
using ValueType = Relativity.Services.InstanceSetting.ValueType;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class InstanceSetting
	{
		public static int Create(string section, string name, string value, ValueType valueType)
		{
			using (IInstanceSettingManager instanceSettingManager = Kepler.CreateProxy<IInstanceSettingManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				global::Relativity.Services.InstanceSetting.InstanceSetting instanceSetting = new global::Relativity.Services.InstanceSetting.InstanceSetting
				{
					Section = section,
					Name = name,
					Value = name,
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
	}
}
