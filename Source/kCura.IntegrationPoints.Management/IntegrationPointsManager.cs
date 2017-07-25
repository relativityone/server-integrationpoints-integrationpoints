using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Management.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Management
{
	public class IntegrationPointsManager : IIntegrationPointsManager
	{
		private readonly IAPILog _logger;
		private readonly IEnumerable<IManagementTask> _tasks;
		private readonly IApplicationRepository _applicationRepository;

		public IntegrationPointsManager(IAPILog logger, IEnumerable<IManagementTask> tasks, IApplicationRepository applicationRepository)
		{
			_tasks = tasks;
			_applicationRepository = applicationRepository;
			_logger = logger;
		}

		public void Start()
		{
			var workspaceArtifactIds = _applicationRepository.GetWorkspaceArtifactIdsWhereApplicationInstalled(Guid.Parse(Constants.IntegrationPoints.APPLICATION_GUID_STRING));
			if (workspaceArtifactIds.Count == 0)
			{
				_logger.LogInformation("RIP is not installed in any workspace. Skipping execution.");
				return;
			}
			foreach (var task in _tasks)
			{
				try
				{
					task.Run(workspaceArtifactIds);
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Failed to execute tasks {type}", task.GetType());
				}
			}
		}
	}
}