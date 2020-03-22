using Relativity.Automation.Utility;
using Relativity.Services.InstanceSetting;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance
{
	public static class RTFSubstitute
	{
		public static async Task CreateOrUpdateInstanceSetting(IKeplerServiceFactory serviceFactory,
			string name, string section, ValueType valueType, string value)
		{
			using (IInstanceSettingManager settingManager = serviceFactory.GetAdminServiceProxy<IInstanceSettingManager>())
			{
				Services.Query query = new Services.Query
				{
					Condition = $"'Name' == '{name}' AND 'Section' == '{section}'"
				};
				InstanceSettingQueryResultSet settingResult = await settingManager.QueryAsync(query).ConfigureAwait(false);

				if (settingResult.Results.Any())
				{
					Services.InstanceSetting.InstanceSetting setting = settingResult.Results.Single().Artifact;
					setting.Value = value;
					await settingManager.UpdateSingleAsync(setting).ConfigureAwait(false);
				}
				else
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
