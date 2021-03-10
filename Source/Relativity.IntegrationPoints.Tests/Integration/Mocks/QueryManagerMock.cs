﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Interfaces;
using kCura.Vendor.Castle.Core.Internal;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	class QueryManagerMock : IQueryManager
	{
		private readonly InMemoryDatabase _db;
		private readonly TestContext _context;

		private int _scheduleQueueCreateRequestCount;

		public QueryManagerMock(InMemoryDatabase database, TestContext context)
		{
			_db = database;
			_context = context;
		}

		public ICommand CreateScheduleQueueTable()
		{
			return new ActionCommand(() =>
			{
				++_scheduleQueueCreateRequestCount;
			});
		}

		public ICommand AddStopStateColumnToQueueTable()
		{
			return ActionCommand.Empty;
		}

		public IQuery<DataRow> GetAgentTypeInformation(Guid agentGuid)
		{
			var agent = _db.Agents.First(x => x.AgentGuid == agentGuid);
			
			return new ValueReturnQuery<DataRow>(agent.AsRow());
		}

		public IQuery<DataTable> GetNextJob(int agentId, int agentTypeId, int[] resourceGroupArtifactId)
		{
			var nextJob = _db.JobsInQueue.Where(x =>
					x.AgentTypeID == agentTypeId &&
					x.NextRunTime <= _context.CurrentDateTime &&
					(x.StopState == StopState.None || x.StopState == StopState.DrainStopped))
				.OrderByDescending(x => x.StopState)
				.FirstOrDefault();

			if (nextJob == null)
			{
				return new ValueReturnQuery<DataTable>(null);
			}

			nextJob.LockedByAgentID = agentId;
			nextJob.StopState = 0;

			return new ValueReturnQuery<DataTable>(nextJob.AsTable());
		}

		public ICommand UpdateScheduledJob(long jobId, DateTime nextUtcRunTime)
		{
			return new ActionCommand(() =>
			{
				var job = _db.JobsInQueue.Single(x => x.JobId == jobId);

				job.NextRunTime = nextUtcRunTime;
				job.LockedByAgentID = null;
			});
		}

		public ICommand UnlockScheduledJob(int agentId)
		{
			return new ActionCommand(() =>
			{
				var lockedJob = _db.JobsInQueue.FirstOrDefault(x => x.LockedByAgentID == agentId);

				if (lockedJob != null)
				{
					lockedJob.LockedByAgentID = null;
				}
			});
		}

		public ICommand UnlockJob(long jobId)
		{
			return new ActionCommand(() =>
			{
				var lockedJob = _db.JobsInQueue.FirstOrDefault(x => x.JobId == jobId);

				if (lockedJob != null)
				{
					lockedJob.LockedByAgentID = null;
				}
			});
		}

		public ICommand DeleteJob(long jobId)
		{
			return new ActionCommand(() =>
			{
				_db.JobsInQueue.RemoveAll(x => x.JobId == jobId);
			});
		}

		public IQuery<DataTable> CreateScheduledJob(int workspaceId, int relatedObjectArtifactId, string taskType, DateTime nextRunTime,
			int agentTypeId, string scheduleRuleType, string serializedScheduleRule, string jobDetails, int jobFlags,
			int submittedBy, long? rootJobId, long? parentJobId = null)
		{
			long newJobId = JobId.Next;

			var newJob = CreateJob(newJobId, workspaceId, relatedObjectArtifactId, taskType,
				nextRunTime, agentTypeId, scheduleRuleType, serializedScheduleRule,
				jobDetails, jobFlags, submittedBy, rootJobId, parentJobId);

			_db.JobsInQueue.Add(newJob);

			return new ValueReturnQuery<DataTable>(newJob.AsTable());
		}

		public ICommand CreateNewAndDeleteOldScheduledJob(long oldScheduledJobId, int workspaceID, int relatedObjectArtifactID,
			string taskType, DateTime nextRunTime, int AgentTypeID, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int SubmittedBy, long? rootJobID, long? parentJobID = null)
		{
			return new ActionCommand(() =>
			{
				_db.JobsInQueue.RemoveAll(x => x.JobId == oldScheduledJobId);

				long newJobId = JobId.Next;

				var newJob = CreateJob(newJobId, workspaceID, relatedObjectArtifactID, taskType,
					nextRunTime, AgentTypeID, scheduleRuleType, serializedScheduleRule,
					jobDetails, jobFlags, SubmittedBy, rootJobID, parentJobID);

				_db.JobsInQueue.Add(newJob);
			});
		}

		public ICommand CleanupJobQueueTable()
		{
			return new ActionCommand(() =>
			{
				_db.JobsInQueue.Where(x => _db.Agents.Exists(a => a.ArtifactId == x.LockedByAgentID))
					.ForEach(x => x.LockedByAgentID = null);

				_db.JobsInQueue.RemoveAll(x => x.LockedByAgentID == null && 
				                               _db.Workspaces.FirstOrDefault(w => w.ArtifactId == x.WorkspaceID) == null);
			});
		}

		public IQuery<DataTable> GetAllJobs()
		{
			var dataTable = DatabaseSchema.AgentSchema();

			_db.JobsInQueue.ForEach(x => dataTable.ImportRow(x.AsRow()));

			return new ValueReturnQuery<DataTable>(dataTable);
		}

		public IQuery<int> UpdateStopState(IList<long> jobIds, StopState state)
		{
			int affectedRows = 0;
			_db.JobsInQueue
				.Where(x => jobIds.Contains(x.JobId))
				.ForEach(x =>
				{
					++affectedRows;
					x.StopState = state;
				});

			return new ValueReturnQuery<int>(affectedRows);
		}

		public IQuery<DataTable> GetJobByRelatedObjectIdAndTaskType(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
		{
			var jobs = _db.JobsInQueue.Where(x =>
				x.WorkspaceID == workspaceId &&
				x.RelatedObjectArtifactID == relatedObjectArtifactId &&
				taskTypes.Contains(x.TaskType));

			var dataTable = DatabaseSchema.ScheduleQueueSchema();
			jobs.ForEach(x => dataTable.ImportRow(x.AsRow()));

			return new ValueReturnQuery<DataTable>(dataTable);
		}

		public IQuery<DataTable> GetJobsByIntegrationPointId(long integrationPointId)
		{
			var jobs = _db.JobsInQueue.Where(x => x.RelatedObjectArtifactID == integrationPointId);

			var dataTable = DatabaseSchema.ScheduleQueueSchema();
			jobs.ForEach(x => dataTable.ImportRow(x.AsRow()));

			return new ValueReturnQuery<DataTable>(dataTable);
		}

		public IQuery<DataTable> GetJob(long jobId)
		{
			var jobs = _db.JobsInQueue.Where(x => x.JobId == jobId);

			var dataTable = DatabaseSchema.ScheduleQueueSchema();
			jobs.ForEach(x => dataTable.ImportRow(x.AsRow()));

			return new ValueReturnQuery<DataTable>(dataTable);
		}

		#region Test Verification

		public void ShouldCreateQueueTable()
		{
			_scheduleQueueCreateRequestCount.Should().BePositive();
		}

		#endregion

		#region Implementation Details
		private class ValueReturnQuery<T> : IQuery<T>
		{
			private readonly T _value;

			public ValueReturnQuery(T value)
			{
				_value = value;
			}

			public T Execute()
			{
				return _value;
			}
		}

		private class ActionCommand : ICommand
		{
			private readonly Action _action;

			public ActionCommand(Action action)
			{
				_action = action;
			}

			public static ActionCommand Empty => new ActionCommand(() => { });

			public void Execute()
			{
				_action();
			}
		}

		private JobTest CreateJob(long jobId, int workspaceId, int relatedObjectArtifactId, string taskType,
			DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int submittedBy, long? rootJobId, long? parentJobId)
		{
			return new JobTest
			{
				JobId = jobId,
				RootJobId = rootJobId,
				ParentJobId = parentJobId,
				AgentTypeID = agentTypeId,
				LockedByAgentID = null,
				WorkspaceID = workspaceId,
				RelatedObjectArtifactID = relatedObjectArtifactId,
				TaskType = taskType,
				NextRunTime = nextRunTime,
				LastRunTime = null,
				ScheduleRuleType = scheduleRuleType,
				SerializedScheduleRule = serializedScheduleRule,
				JobDetails = jobDetails,
				JobFlags = jobFlags,
				SubmittedDate = _context.CurrentDateTime,
				SubmittedBy = submittedBy,
				StopState = _db.JobsInQueue.FirstOrDefault(x => x.ParentJobId > 0)?.StopState ?? StopState.None
			};
		}
		#endregion
	}
}