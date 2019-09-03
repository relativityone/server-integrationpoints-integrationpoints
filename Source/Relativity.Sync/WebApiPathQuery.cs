using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
	internal sealed class WebApiPathQuery : IWebApiPathQuery
	{
		private const string _WEB_API_PATH_SETTING_SECTION = "kCura.IntegrationPoints";
		private const string _WEB_API_PATH_SETTING_NAME = "WebAPIPath";

		private readonly ISourceServiceFactoryForAdmin _serviceFactory;

		public WebApiPathQuery(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<string> GetWebApiPathAsync()
		{
			using (IInstanceSettingManager instanceSettingManager = await _serviceFactory.CreateProxyAsync<IInstanceSettingManager>().ConfigureAwait(false))
			{
				var query = new Services.Query
				{
					Condition = $"'Name' == '{_WEB_API_PATH_SETTING_NAME}' AND 'Section' == '{_WEB_API_PATH_SETTING_SECTION}'"
				};
				InstanceSettingQueryResultSet resultSet = await instanceSettingManager.QueryAsync(query).ConfigureAwait(false);

				if (resultSet.Success == false)
				{
					throw new SyncException($"Failed to query for '{_WEB_API_PATH_SETTING_NAME}' instance setting. Response message: {resultSet.Message}");
				}
				if (resultSet.TotalCount <= 0)
				{
					throw new SyncException($"Query for '{_WEB_API_PATH_SETTING_NAME}' instance setting from section '{_WEB_API_PATH_SETTING_SECTION}' returned empty results. Make sure instance setting exists.");
				}
				return resultSet.Results.First().Artifact.Value;
			}
		}
	}
}