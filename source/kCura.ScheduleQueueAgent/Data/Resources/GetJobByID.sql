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
			,[JobDetails]
			,[JobFlags]
			,[SubmittedDate]
			,[SubmittedBy]
FROM
			[eddsdbo].[{0}] WITH(NOLOCK)
WHERE
			JobID = @JobID
