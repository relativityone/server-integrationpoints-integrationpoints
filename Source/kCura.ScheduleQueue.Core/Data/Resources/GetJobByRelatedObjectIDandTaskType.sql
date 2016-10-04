﻿SELECT 
			[JobID]
			,[RootJobID]
			,[ParentJobID]
			,[AgentTypeID]
			,[LockedByAgentID]
			,[WorkspaceID]
			,[RelatedObjectArtifactID]
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
FROM
			[eddsdbo].[{0}] WITH(NOLOCK)
WHERE
			[WorkspaceID] = @WorkspaceID
	AND 
			[RelatedObjectArtifactID] = @RelatedObjectArtifactID
	AND 
			[TaskType] IN ({1})
	AND
			NOT [ScheduleRule] IS NULL	
