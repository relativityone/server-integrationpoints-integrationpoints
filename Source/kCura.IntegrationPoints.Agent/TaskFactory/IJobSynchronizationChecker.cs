﻿using System;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
	public interface IJobSynchronizationChecker
	{
		void CheckForSynchronization(Type type, Job job, IntegrationPoint integrationPointDto, ScheduleQueueAgentBase agentBase);
	}
}