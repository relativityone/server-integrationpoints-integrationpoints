SELECT 
			[JobID]
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
FROM
			[eddsdbo].[{0}] WITH(NOLOCK)
WHERE
			[WorkspaceID] = @WorkspaceID
	AND 
			[RelatedObjectArtifactID] = @RelatedObjectArtifactID
	AND 
			[TaskType] = @TaskType

