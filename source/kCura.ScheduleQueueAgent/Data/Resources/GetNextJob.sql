IF EXISTS(SELECT TOP 1 JobID FROM [eddsdbo].[{0}] WHERE [AgentID] = @AgentID)
BEGIN
	--This Agent has stopped before finalizing this job previously
	--So, pick it up again and finish it.
	SELECT TOP (1)
				[JobID],
				[AgentTypeID],
				[LockedByAgentID],
				[WorkspaceID],
				[RelatedObjectArtifactID],
				[TaskType],
				[NextRunTime],
				[LastRunTime],
				[ScheduleRules],
				[JobDetail],
				[JobFlags],
				[SubmittedDate],
				[SubmittedBy]
	FROM
				[eddsdbo].[{0}]
	WHERE 
				[LockedByAgentID] = @AgentID
END
ELSE
BEGIN
	UPDATE TOP (1) [eddsdbo].[{0}]
	SET
			[AgentID]	= @AgentID
	OUTPUT 
			INSERTED.[JobID],
			INSERTED.[AgentTypeID],
			INSERTED.[LockedByAgentID],
			INSERTED.[WorkspaceID],
			INSERTED.[RelatedObjectArtifactID],
			INSERTED.[TaskType],
			INSERTED.[NextRunTime],
			INSERTED.[LastRunTime],
			INSERTED.[ScheduleRules],
			INSERTED.[JobDetail],
			INSERTED.[JobFlags],
			INSERTED.[SubmittedDate],
			INSERTED.[SubmittedBy]
	FROM 
			[eddsdbo].[{0}] q WITH (UPDLOCK, READPAST, ROWLOCK)
		INNER JOIN 
			[eddsdbo].[Case] c 
		ON q.[WorkspaceID] = c.[ArtifactID]
	WHERE
		q.[LockedByAgentID] IS NULL
		AND q.[AgentTypeID] = @AgentTypeID
		AND q.[NextRunTime] <= GETUTCDATE()
		AND c.ResourceGroupArtifactID IN (@ResourceGroupArtifactIDs)
END