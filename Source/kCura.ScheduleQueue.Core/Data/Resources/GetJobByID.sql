﻿SELECT 
			[JobID]
			,[RootJobID]
			,[ParentJobID]
			,[AgentTypeID]
			,[LockedByAgentID]
			,[WorkspaceID]
			,[RelatedObjectArtifactID]
            ,[CorrelationID]
			,[TaskType]
			,[NextRunTime]
			,[LastRunTime]
			,[ScheduleRuleType]
			,[ScheduleRule]
			,[JobDetails]
			,[JobFlags]
			,[SubmittedDate]
			,[SubmittedBy]
			,[StopState]
			,[Heartbeat]
FROM
			[eddsdbo].[{0}] WITH(NOLOCK)
WHERE
			JobID = @JobID
