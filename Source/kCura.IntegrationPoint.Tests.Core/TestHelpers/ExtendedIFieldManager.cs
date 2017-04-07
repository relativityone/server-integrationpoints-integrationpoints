using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.FieldManager;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	class ExtendedIFieldManager : IFieldManager
	{
		private readonly ITestHelper _helper;
		private readonly ExecutionIdentity _identity;
		private Lazy<IFieldManager> _managerWrapper;
		private IFieldManager Manager => _managerWrapper.Value;

		public ExtendedIFieldManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IFieldManager>(helper.CreateUserProxy<IFieldManager>);
		}

		private readonly object _lock = new object();

		public void Dispose()
		{
			lock (_lock)
			{
				Manager.Dispose();
				_managerWrapper = new Lazy<IFieldManager>(_helper.CreateUserProxy<IFieldManager>);
			}
		}

		public async Task<List<FieldRef>> RetrieveBeginBatesFieldsAsync(int workspaceArtifactId)
		{
			return await Manager.RetrieveBeginBatesFieldsAsync(workspaceArtifactId);
		}

		public async Task<int?> RetrieveArtifactViewFieldIdAsync(int workspaceArtifactId, int fieldArtifactId)
		{
			return await Manager.RetrieveArtifactViewFieldIdAsync(workspaceArtifactId, fieldArtifactId);
		}

		public async Task UpdateFilterTypeAsync(int workspaceArtifactId, int artifactViewFieldId, string filterType)
		{
			await Manager.UpdateFilterTypeAsync(workspaceArtifactId, artifactViewFieldId, filterType);
		}

		public async Task SetOverlayBehaviorAsync(int workspaceArtifactId, int fieldArtifactId, bool fieldOverlayBehavior)
		{
			await Manager.SetOverlayBehaviorAsync(workspaceArtifactId, fieldArtifactId, fieldOverlayBehavior);
		}
	}
}
