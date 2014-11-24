SELECT 
			[JobID]
			,[AgentTypeID]
			,[LockedByAgentID]
			,[WorkspaceID]
			,[RelatedObjectArtifactID]
			,[TaskType]
			,[NextRunTime]
			,[LastRunTime]
			,[ScheduleRule]
			,[JobDetail]
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

