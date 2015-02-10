﻿using System;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Services
{
	public class AgentJobManager : IJobManager
	{
		private readonly IEddsServiceContext _context;
		private readonly IJobService _jobService;
		private readonly kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		public AgentJobManager(IEddsServiceContext context, IJobService jobService, kCura.Apps.Common.Utils.Serializers.ISerializer serializer)
		{
			_context = context;
			_jobService = jobService;
			_serializer = serializer;
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID, IScheduleRule rule)
		{
			try
			{
				string serializedDetails = _serializer.Serialize(jobDetails);
				if (rule != null)
				{
					_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), rule, serializedDetails, _context.UserID);
				}
				else
				{
					_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), DateTime.UtcNow, serializedDetails, _context.UserID);
				}
			}
			catch (AgentNotFoundException anfe)
			{
				throw new Exception(Properties.ErrorMessages.NoAgentInstalled, anfe);
			}
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID)
		{
			try
			{
				string serializedDetails = _serializer.Serialize(jobDetails);
				_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), DateTime.UtcNow, serializedDetails, _context.UserID);
			}
			catch (AgentNotFoundException anfe)
			{
				throw new Exception(Properties.ErrorMessages.NoAgentInstalled, anfe);
			}
		}
	}
}
