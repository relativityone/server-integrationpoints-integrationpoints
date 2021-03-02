//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using kCura.ScheduleQueue.Core;
//using kCura.ScheduleQueue.Core.Core;
//using kCura.ScheduleQueue.Core.Data;
//using kCura.ScheduleQueue.Core.Data.Interfaces;
//using kCura.Vendor.Castle.Core.Internal;
//using Moq;
//using Relativity.API;

//namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
//{
//	public class ScheduleQueueMock
//	{
//		private readonly DataTable _qDt;

//		public readonly List<DataRow> JobsInQueue = new List<DataRow>();

//		public List<int> Agents = new List<int>();

//		public List<int> Workspaces = new List<int>();

//		public DateTime CurrentAgentDateTime { get; set; }

//		public Mock<IQueryManager> QueryManagerMock { get; }

//		public ScheduleQueueMock()
//		{
//			_qDt = SetupQueueDataTable();

//			QueryManagerMock = new Mock<IQueryManager>();

//			SetupQueryManager();
//		}

//		private void SetupQueryManager()
//		{
//			QueryManagerMock.Setup(x => x.CreateScheduleQueueTable())
//				.Returns(new ActionCommand(() => { }));

//			QueryManagerMock.Setup(x => x.AddStopStateColumnToQueueTable())
//				.Returns(new ActionCommand(() => { }));

//			QueryManagerMock.Setup(x => x.GetAgentTypeInformation(It.IsAny<Guid>()))
//				.Returns((Guid agentGuid) =>
//				{
//					var dt = new DataTable();
//					dt.Columns.AddRange(new DataColumn[]
//					{
//						new DataColumn() {ColumnName = "AgentTypeID", DataType = typeof(int)},
//						new DataColumn() {ColumnName = "Name", DataType = typeof(string)},
//						new DataColumn() {ColumnName = "Fullnamespace", DataType = typeof(string), AllowDBNull = true},
//						new DataColumn() {ColumnName = "Guid", DataType = typeof(Guid)}
//					});

//					var newRow = dt.NewRow();

//					newRow["AgentTypeID"] = Const.Agent._RIP_AGENT_TYPE_ID;
//					newRow["Name"] = "Integration Points Agent";
//					newRow["Fullnamespace"] = "namespaceTEsfsafsa";
//					newRow["Guid"] = agentGuid;

//					return new ValueReturnQuery<DataRow>(newRow);
//				});

//			QueryManagerMock.Setup(x => x.GetNextJob(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[]>()))
//				.Returns((int agentId, int agentTypeId, int[] resourceGroupsIds) =>
//				{
//					var nextJob = JobsInQueue.Where(x => 
//							(int)x["AgentTypeID"] == agentTypeId &&
//							(DateTime)x["NextRunTime"] <= CurrentAgentDateTime &&
//							((StopState)x["StopState"] == StopState.None || (StopState)x["StopState"] == StopState.DrainStopped))
//						.OrderByDescending(x => (StopState)x["StopState"])
//						.FirstOrDefault();

//					if (nextJob == null)
//					{
//						return new ValueReturnQuery<DataTable>(null);
//					}

//					nextJob["LockedByAgentID"] = agentId;
//					nextJob["StopState"] = 0;

//					var dataTable = new DataTable();
//					dataTable.ImportRow(nextJob);

//					return new ValueReturnQuery<DataTable>(dataTable);
//				});

//			QueryManagerMock.Setup(x => x.UpdateScheduledJob(It.IsAny<long>(), It.IsAny<DateTime>()))
//				.Returns((long jobId, DateTime nextUtcRunTime) =>
//				{
//					Action action = () =>
//					{
//						var job = JobsInQueue.Single(x => (long)x["JobID"] == jobId);

//						job["NextRunTime"] = nextUtcRunTime;
//						job["LockedByAgentID"] = DBNull.Value;
//					};

//					return new ActionCommand(action);
//				});

//			QueryManagerMock.Setup(x => x.UnlockScheduledJob(It.IsAny<int>()))
//				.Returns((int agentId) =>
//				{
//					Action action = () =>
//					{
//						var lockedJob = JobsInQueue.FirstOrDefault(x => (int) x["LockedByAgentID"] == agentId);

//						if (lockedJob != null)
//						{
//							lockedJob["LockedByAgentID"] = DBNull.Value;
//						}
//					};

//					return new ActionCommand(action);
//				});

//			QueryManagerMock.Setup(x => x.UnlockJob(It.IsAny<long>()))
//				.Returns((long jobId) =>
//				{
//					return new ActionCommand(() =>
//					{
//						var lockedJob = JobsInQueue.FirstOrDefault(x => (int) x["JobID"] == jobId);

//						if (lockedJob != null)
//						{
//							lockedJob["LockedByAgentID"] = DBNull.Value;
//						}
//					});
//				});

//			QueryManagerMock.Setup(x => x.DeleteJob(It.IsAny<long>()))
//				.Returns((long jobId) => new ActionCommand(() =>
//				{
//					JobsInQueue.RemoveAll(x => (long) x["JobID"] == jobId);
//				}));

//			QueryManagerMock.Setup(x => x.DeleteJob(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<long>()))
//				.Returns((IDBContext context, string table, long jobId) => new ActionCommand(() =>
//				{
//					JobsInQueue.RemoveAll(x => (long)x["JobID"] == jobId);
//				}));

//			QueryManagerMock.Setup(x => x.CreateScheduledJob(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
//					It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
//					It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long?>()))
//				.Returns((int workspaceId, int relatedObjectArtifactId, string taskType,
//						DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
//						string jobDetails, int jobFlags, int submittedBy, long? rootJobId, long? parentJobId) =>
//						{
//							long newJobId = JobsInQueue.Max(x => (long) x["JobID"]) + 1;

//							var newJob = CreateJob(newJobId, workspaceId, relatedObjectArtifactId, taskType,
//								nextRunTime, agentTypeId, scheduleRuleType, serializedScheduleRule,
//								jobDetails, jobFlags, submittedBy, rootJobId, parentJobId);

//							JobsInQueue.Add(newJob);

//							var dataTable = new DataTable();
//							dataTable.ImportRow(newJob);

//							return new ValueReturnQuery<DataTable>(dataTable);
//						});

//			QueryManagerMock.Setup(x => x.CreateScheduledJob(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<int>(), 
//					It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), 
//					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long?>()))
//				.Returns((IDBContext context, string table, int workspaceId, int relatedObjectArtifactId, string taskType,
//					DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
//					string jobDetails, int jobFlags, int submittedBy, long? rootJobId, long? parentJobId) =>
//				{
//					long newJobId = JobsInQueue.Max(x => (long)x["JobID"]) + 1;

//					var newJob = CreateJob(newJobId, workspaceId, relatedObjectArtifactId, taskType,
//						nextRunTime, agentTypeId, scheduleRuleType, serializedScheduleRule,
//						jobDetails, jobFlags, submittedBy, rootJobId, parentJobId);

//					JobsInQueue.Add(newJob);

//					var dataTable = new DataTable();
//					dataTable.ImportRow(newJob);

//					return new ValueReturnQuery<DataTable>(dataTable);
//				});

//			QueryManagerMock.Setup(x => x.CleanupJobQueueTable())
//				.Returns(new ActionCommand(() =>
//				{
//					int[] agentIds = Agents.Select(x => x).ToArray();

//					JobsInQueue.Where(x => agentIds.Contains((int) x["LockedByAgentID"]))
//						.ForEach(x => x["LockedByAgentID"] = DBNull.Value);

//					JobsInQueue.RemoveAll(x => x["LockedByAgentID"] == null &&
//					                           !Workspaces.Contains((int) x["WorkspaceID"]));
//				}));

//			QueryManagerMock.Setup(x => x.GetAllJobs())
//				.Returns(() =>
//				{
//					var dataTable = new DataTable();

//					JobsInQueue.ForEach(x => dataTable.ImportRow(x));

//					return new ValueReturnQuery<DataTable>(dataTable);
//				});

//			QueryManagerMock.Setup(x => x.UpdateStopState(It.IsAny<IList<long>>(), It.IsAny<StopState>()))
//				.Returns((IList<long> jobIds, StopState state) =>
//				{
//					int affectedRows = 0;
//					JobsInQueue
//						.Where(x => jobIds.Contains((long)x["JobID"]))
//						.ForEach(x =>
//						{
//							++affectedRows;
//							x["StopState"] = state;
//						});

//					return new ValueReturnQuery<int>(affectedRows);
//				});

//			QueryManagerMock.Setup(x =>
//					x.GetJobByRelatedObjectIdAndTaskType(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
//				.Returns((int workspaceId, int relatedObjectArtifactId,
//					List<string> taskTypes) =>
//				{
//					var jobs = JobsInQueue.Where(x =>
//						(int) x["WorkspaceID"] == workspaceId &&
//						(int) x["RelatedObjectArtifactID"] == relatedObjectArtifactId &&
//						taskTypes.Contains((string) x["TaskType"]));

//					var dataTable = new DataTable();
//					jobs.ForEach(x => dataTable.ImportRow(x));

//					return new ValueReturnQuery<DataTable>(dataTable);
//				});

//			QueryManagerMock.Setup(x => x.GetJobsByIntegrationPointId(It.IsAny<long>()))
//				.Returns((long integrationPointId) =>
//				{
//					var jobs = JobsInQueue.Where(x => (int)x["RelatedObjectArtifactID"] == integrationPointId);

//					var dataTable = new DataTable();
//					jobs.ForEach(x => dataTable.ImportRow(x));

//					return new ValueReturnQuery<DataTable>(dataTable);
//				});

//			QueryManagerMock.Setup(x => x.GetJob(It.IsAny<long>()))
//				.Returns((long jobId) =>
//				{
//					var jobs = JobsInQueue.Where(x => (long)x["JobID"] == jobId);

//					var dataTable = new DataTable();
//					jobs.ForEach(x => dataTable.ImportRow(x));

//					return new ValueReturnQuery<DataTable>(dataTable);
//				});
//		}

//		public DataRow CreateJob(long jobId, int workspaceId, int relatedObjectArtifactId, string taskType,
//			DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
//			string jobDetails, int jobFlags, int submittedBy, long? rootJobId, long? parentJobId)
//		{
//			var newJob = _qDt.NewRow();

//			newJob["JobID"] = jobId;
//			newJob["RootJobID"] = rootJobId;
//			newJob["ParentJobID"] = parentJobId;
//			newJob["AgentTypeID"] = agentTypeId;
//			newJob["LockedByAgentID"] = DBNull.Value;
//			newJob["WorkspaceID"] = workspaceId;
//			newJob["RelatedObjectArtifactID"] = relatedObjectArtifactId;
//			newJob["TaskType"] = taskType;
//			newJob["NextRunTime"] = nextRunTime;
//			newJob["LastRunTime"] = DBNull.Value;
//			newJob["JobDetails"] = jobDetails;
//			newJob["JobFlags"] = jobFlags;
//			newJob["SubmittedDate"] = CurrentAgentDateTime;
//			newJob["SubmittedBy"] = submittedBy;
//			newJob["ScheduleRuleType"] = scheduleRuleType;
//			newJob["ScheduleRule"] = serializedScheduleRule;
//			newJob["StopState"] =
//				JobsInQueue.FirstOrDefault(x => (long?)x["ParentJobID"] > 0)?["StopState"] ?? StopState.None;

//			return newJob;
//		}

//		private DataTable SetupQueueDataTable()
//		{
//			DataTable dt = new DataTable();
//			dt.Columns.AddRange(new DataColumn[]
//			{
//				new DataColumn() {ColumnName = "JobID", DataType = typeof(long)},
//				new DataColumn() {ColumnName = "RootJobID", DataType = typeof(long), AllowDBNull = true},
//				new DataColumn() {ColumnName = "ParentJobID", DataType = typeof(long), AllowDBNull = true},
//				new DataColumn() {ColumnName = "AgentTypeID", DataType = typeof(int)},
//				new DataColumn() {ColumnName = "LockedByAgentID", DataType = typeof(int)},
//				new DataColumn() {ColumnName = "WorkspaceID", DataType = typeof(int)},
//				new DataColumn() {ColumnName = "RelatedObjectArtifactID", DataType = typeof(int)},
//				new DataColumn() {ColumnName = "TaskType", DataType = typeof(string)},
//				new DataColumn() {ColumnName = "NextRunTime", DataType = typeof(DateTime)},
//				new DataColumn() {ColumnName = "LastRunTime", DataType = typeof(DateTime), AllowDBNull = true},
//				new DataColumn() {ColumnName = "JobDetails", DataType = typeof(string)},
//				new DataColumn() {ColumnName = "JobFlags", DataType = typeof(int)},
//				new DataColumn() {ColumnName = "SubmittedDate", DataType = typeof(DateTime)},
//				new DataColumn() {ColumnName = "SubmittedBy", DataType = typeof(int)},
//				new DataColumn() {ColumnName = "ScheduleRuleType", DataType = typeof(string)},
//				new DataColumn() {ColumnName = "ScheduleRule", DataType = typeof(string)},
//				new DataColumn() {ColumnName = "StopState", DataType = typeof(int)}
//			});
//			return dt;
//		}

//		private class ValueReturnQuery<T> : IQuery<T>
//		{
//			private readonly T _value;

//			public ValueReturnQuery(T value)
//			{
//				_value = value;
//			}

//			public T Execute()
//			{
//				return _value;
//			}
//		}

//		private class ActionCommand : ICommand
//		{
//			private readonly Action _action;

//			public ActionCommand(Action action)
//			{
//				_action = action;
//			}

//			public void Execute()
//			{
//				_action();
//			}
//		}
//	}
//}
