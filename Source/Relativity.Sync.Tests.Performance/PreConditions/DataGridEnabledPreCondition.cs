using System;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.Performance.PreConditions
{
	internal class DataGridEnabledPreCondition : IPreCondition
	{
		private readonly ServiceFactory _serviceFactory;
		private readonly int _workspaceId;

		public string Name => $"{nameof(DataGridEnabledPreCondition)} - {_workspaceId}";

		public DataGridEnabledPreCondition(ServiceFactory serviceFactory, int workspaceId)
		{
			_serviceFactory = serviceFactory;
			_workspaceId = workspaceId;
		}

		public bool Check()
		{
			using (IWorkspaceManager workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				var result = workspaceManager.ReadAsync(_workspaceId)
					.GetAwaiter().GetResult();

				return result.EnableDataGrid;
			}
		}

		public FixResult TryFix()
		{
			using (IWorkspaceManager workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				var workspaceResponse = workspaceManager.ReadAsync(_workspaceId)
					.GetAwaiter().GetResult();

				WorkspaceRequest updateRequest = new WorkspaceRequest(workspaceResponse)
				{
					EnableDataGrid = true
				};

				workspaceManager.UpdateAsync(_workspaceId, updateRequest)
					.GetAwaiter().GetResult();
			}

			return Check()
				? FixResult.Fixed()
				: FixResult.Error(new Exception($"{nameof(DataGridEnabledPreCondition)} - Check is still failing after fix"));
		}
	}
}
