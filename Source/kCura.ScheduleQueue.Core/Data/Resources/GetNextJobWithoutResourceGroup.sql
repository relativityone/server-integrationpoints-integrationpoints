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
			AND (@RootJobID IS NULL OR q.[RootJobID] = @RootJobID)
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