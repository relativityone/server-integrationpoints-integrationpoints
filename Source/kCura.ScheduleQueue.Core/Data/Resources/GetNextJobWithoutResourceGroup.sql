IF EXISTS(SELECT TOP 1 JobID FROM [eddsdbo].[{0}] WHERE [LockedByAgentID] = @AgentID)
BEGIN
	--This Agent has stopped before finalizing this job previously
	--So, pick it up again and finish it.
	SELECT TOP (1)
				[JobID],
				[RootJobID],
				[ParentJobID],
				[AgentTypeID],
				[LockedByAgentID],
				[WorkspaceID],
				[RelatedObjectArtifactID],
				[TaskType],
				[NextRunTime],
				[LastRunTime],
				[ScheduleRuleType],
				[ScheduleRule],
				[JobDetails],
				[JobFlags],
				[SubmittedDate],
				[SubmittedBy],
				[StopState]
	FROM
				[eddsdbo].[{0}]
	WHERE 
				[LockedByAgentID] = @AgentID
END
ELSE
BEGIN
	UPDATE [eddsdbo].[{0}]
	SET
			[LockedByAgentID]	= @AgentID,
			[StopState] = 0
	OUTPUT 
			INSERTED.[JobID],
			INSERTED.[RootJobID],
			INSERTED.[ParentJobID],
			INSERTED.[AgentTypeID],
			INSERTED.[LockedByAgentID],
			INSERTED.[WorkspaceID],
			INSERTED.[RelatedObjectArtifactID],
			INSERTED.[TaskType],
			INSERTED.[NextRunTime],
			INSERTED.[LastRunTime],
			INSERTED.[ScheduleRuleType],
			INSERTED.[ScheduleRule],
			INSERTED.[JobDetails],
			INSERTED.[JobFlags],
			INSERTED.[SubmittedDate],
			INSERTED.[SubmittedBy],
			INSERTED.[StopState]
	WHERE [JobID] =
	(
		SELECT TOP 1 [JobID]
		FROM [eddsdbo].[{0}] q WITH (UPDLOCK, READPAST, ROWLOCK, INDEX([IX_{0}_LockedByAgentID_AgentTypeID_NextRunTime]))
			INNER JOIN [eddsdbo].[Case] c 
			ON q.[WorkspaceID] = c.[ArtifactID]
		WHERE
			q.[LockedByAgentID] IS NULL
			AND q.[AgentTypeID] = @AgentTypeID
			AND q.[NextRunTime] <= GETUTCDATE()
			AND q.[StopState] IN (0, 8)
		ORDER BY
			CASE [StopState]
				WHEN 8 
					THEN 0
				ELSE 1
			END
	)
END