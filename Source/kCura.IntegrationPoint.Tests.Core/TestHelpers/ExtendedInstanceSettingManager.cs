using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.InstanceSetting;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedInstanceSettingManager : IInstanceSettingManager
	{
		private ExecutionIdentity _identity;
		private Lazy<IInstanceSettingManager> _managerWrapper;
		private readonly ITestHelper _helper;

		private readonly object _lock = new object();
		private IInstanceSettingManager Manager => _managerWrapper.Value;

		public ExtendedInstanceSettingManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IInstanceSettingManager>(helper.CreateUserProxy<IInstanceSettingManager>);
		}
		public void Dispose()
		{
			lock (_lock)
			{
				Manager.Dispose();
				_managerWrapper = new Lazy<IInstanceSettingManager>(_helper.CreateUserProxy<IInstanceSettingManager>);
			}
		}

		public async Task<InstanceSettingQueryResultSet> QueryAsync(Query query)
		{
			return await Manager.QueryAsync(query);
		}

		public async Task<InstanceSettingQueryResultSet> QueryAsync(Query query, int length)
		{
			return await Manager.QueryAsync(query, length);
		}

		public async Task<InstanceSettingQueryResultSet> QuerySubsetAsync(string queryToken, int start, int length)
		{
			return await Manager.QuerySubsetAsync(queryToken, start, length);
		}

		public async Task<global::Relativity.Services.InstanceSetting.InstanceSetting> ReadSingleAsync(int instanceSettingArtifactID)
		{
			return await Manager.ReadSingleAsync(instanceSettingArtifactID);
		}

		public async Task<int> CreateSingleAsync(global::Relativity.Services.InstanceSetting.InstanceSetting instanceSettingDTO)
		{
			return await Manager.CreateSingleAsync(instanceSettingDTO);
		}

		public async Task UpdateSingleAsync(global::Relativity.Services.InstanceSetting.InstanceSetting instanceSettingDTO)
		{
			await Manager.UpdateSingleAsync(instanceSettingDTO);
		}

		public async Task DeleteSingleAsync(int instanceSettingArtifactID)
		{
			await Manager.DeleteSingleAsync(instanceSettingArtifactID);
		}

		public async Task<List<ValueTypeData>> GetAllValueTypesAsync()
		{
			return await Manager.GetAllValueTypesAsync();
		}

		public async Task EncryptInstanceSettingAsync(int instanceSettingArtifactID)
		{
			await Manager.EncryptInstanceSettingAsync(instanceSettingArtifactID);
		}
	}
}
