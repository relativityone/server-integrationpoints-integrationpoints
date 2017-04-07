using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIArtifactGuidManager : IArtifactGuidManager
	{
		private readonly ITestHelper _helper;
		private readonly ExecutionIdentity _identity;
		private Lazy<IArtifactGuidManager> _managerWrapper;
		private IArtifactGuidManager Manager => _managerWrapper.Value;

		public ExtendedIArtifactGuidManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IArtifactGuidManager>(helper.CreateUserProxy<IArtifactGuidManager>);
		}

		private readonly object _lock = new object();

		public void Dispose()
		{
			lock (_lock)
			{
				Manager.Dispose();
				_managerWrapper = new Lazy<IArtifactGuidManager>(_helper.CreateUserProxy<IArtifactGuidManager>);
			}
		}

		public async Task<bool> GuidExistsAsync(int workspaceId, Guid guid)
		{
			return await Manager.GuidExistsAsync(workspaceId, guid);
		}

		public async Task CreateSingleAsync(int workspaceId, int artifactId, List<Guid> guids)
		{
			await Manager.CreateSingleAsync(workspaceId, artifactId, guids);
		}

		public async Task<List<Guid>> ReadSingleGuidsAsync(int workspaceId, int artifactId)
		{
			return await Manager.ReadSingleGuidsAsync(workspaceId, artifactId);
		}

		public async Task<int> ReadSingleArtifactIdAsync(int workspaceId, Guid guid)
		{
			return await Manager.ReadSingleArtifactIdAsync(workspaceId, guid);
		}
	}
}
