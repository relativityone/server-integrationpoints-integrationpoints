using System;
using System.Threading.Tasks;
using System.Reflection;
using Relativity.API;
using Relativity.Sync.KeplerFactory;
using Relativity.Services.InstanceSetting;
using Relativity.Services;

namespace Relativity.Sync.Telemetry
{
	/// <inheritdoc/>
	internal sealed class EnvironmentPropertyProvider : IEnvironmentPropertyProvider
	{
		private const string _INSTANCENAME_SETTING_NAME = "FriendlyInstanceName";
		private const string _INSTANCENAME_SETTING_SECTION = "Relativity.Authentication";
		private const string _INSTANCENAME_DEFAULT_VALUE = "unknown";

		/// <summary>
		///     Synchronously creates an instance of <see cref="IEnvironmentPropertyProvider"/>.
		/// </summary>
		/// <param name="helper">API helper used to create the instance, if necessary</param>
		/// <param name="logger">Logger used to log any events related to the creation of this instance</param>
		/// <returns>New instance of <see cref="IEnvironmentPropertyProvider"/></returns>
		public static IEnvironmentPropertyProvider Create(IServicesMgr helper, ISyncLog logger)
		{
			return CreateAsync(helper, logger).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		///     Creates an instance of <see cref="IEnvironmentPropertyProvider"/>.
		/// </summary>
		/// <param name="helper">API helper used to create the instance, if necessary</param>
		/// <param name="logger">Logger used to log any events related to the creation of this instance</param>
		/// <returns>New instance of <see cref="IEnvironmentPropertyProvider"/></returns>
		public static async Task<IEnvironmentPropertyProvider> CreateAsync(IServicesMgr helper, ISyncLog logger)
		{
			using (IInstanceSettingManager instanceSettingManager = helper.CreateProxy<IInstanceSettingManager>(ExecutionIdentity.System))
			{
				Services.Query query = CreateInstanceSettingQuery();

				string relativityInstanceName = _INSTANCENAME_DEFAULT_VALUE;
				try
				{
					InstanceSettingQueryResultSet result = await instanceSettingManager.QueryAsync(query).ConfigureAwait(false);

					if (result.Success && result.Results.Count > 0)
					{
						relativityInstanceName = result.Results[0].Artifact.Value;
					}
					else
					{
						if (result.Success)
						{
							logger.LogWarning(
								$"No results found when querying for Instance Setting {_INSTANCENAME_SETTING_SECTION}:{_INSTANCENAME_SETTING_NAME} on Relativity.Sync startup; defaulting to \"{{value}}\"",
								relativityInstanceName);
						}
						else
						{
							logger.LogWarning(
								$"Query for Instance Setting {_INSTANCENAME_SETTING_SECTION}:{_INSTANCENAME_SETTING_NAME} on Relativity.Sync startup failed ('{{error}}'); defaulting to \"{{value}}\"",
								result.Message,
								relativityInstanceName);
						}
					}
				}
				catch (Exception ex)
				{
					logger.LogWarning(
						ex,
						$"Query for Instance Setting {_INSTANCENAME_SETTING_SECTION}:{_INSTANCENAME_SETTING_NAME} on Relativity.Sync startup threw an exception; defaulting to \"{{value}}\"",
						relativityInstanceName);
				}

				var instance = new EnvironmentPropertyProvider(relativityInstanceName);
				return instance;
			}
		}

		private static Services.Query CreateInstanceSettingQuery()
		{
			var sectionCondition = new TextCondition(InstanceSettingFieldNames.Section, TextConditionEnum.EqualTo, _INSTANCENAME_SETTING_SECTION);
			var nameCondition = new TextCondition(InstanceSettingFieldNames.Name, TextConditionEnum.EqualTo, _INSTANCENAME_SETTING_NAME);
			return new Services.Query
			{
				Condition = new CompositeCondition(nameCondition, CompositeConditionEnum.And, sectionCondition).ToQueryString()
			};
		}

		private EnvironmentPropertyProvider(string instanceName)
		{
			InstanceName = instanceName;
		}

		/// <inheritdoc/>
		public string InstanceName { get; }

		/// <inheritdoc/>
		public string CallingAssembly => Assembly.GetCallingAssembly().GetName().Name;
	}
}
