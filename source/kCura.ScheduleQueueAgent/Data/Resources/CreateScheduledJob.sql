DECLARE @job table
(
		[JobID] [bigint] IDENTITY(1,1) NOT NULL,
		[AgentTypeID] [int] NOT NULL,
		[LockedByAgentID] [int] NULL,
		[WorkspaceID] [int] NOT NULL,
		[RelatedObjectArtifactID] [int] NOT NULL,
		[TaskType] [nvarchar](255) NOT NULL,
		[NextRunTime] [datetime] NOT NULL,
		[LastRunTime] [datetime] NULL,
		[ScheduleRule] [nvarchar](max) NULL,
		[JobDetail] [nvarchar](max) NULL,
		[JobFlags] [int] NOT NULL,
		[SubmittedDate] [datetime] NOT NULL,
		[SubmittedBy] [int] NOT NULL
)
	
UPDATE 
		[eddsdbo].[{0}] WITH (UPDLOCK, ROWLOCK)
SET 
		[NextRunTime] = @NextRunTime
		,[ScheduleRules] = @ScheduleRules
		,[JobDetail] = @JobDetail
OUTPUT 
		Inserted.[JobID]
		,Inserted.[AgentTypeID]
		,Inserted.[LockedByAgentID]
		,Inserted.[WorkspaceID]
		,Inserted.[RelatedObjectArtifactID]
		,Inserted.[TaskType]
		,Inserted.[NextRunTime]
		,Inserted.[LastRunTime]
		,Inserted.[ScheduleRule]
		,Inserted.[JobDetail]
		,Inserted.[JobFlags]
		,Inserted.[SubmittedDate]
		,Inserted.[SubmittedBy]
INTO
		@job
WHERE 
		[WorkspaceID] = @WorkspaceID
	AND
		[RelatedObjectArtifactID] = @RelatedObjectArtifactID
	AND 
		[TaskType] = @TaskType
	AND
		(NOT @ScheduleRule IS NULL AND NOT [ScheduleRule] IS NULL)
	AND
		[AgentID] IS NULL

IF @@ROWCOUNT = 0
BEGIN
	IF EXISTS(SELECT TOP 1 JobID FROM [eddsdbo].[{0}] 
		WHERE 	
			[WorkspaceID] = @WorkspaceID
		AND 
			[RelatedObjectArtifactID] = @RelatedObjectArtifactID
		AND
			[TaskType] = @TaskType
		AND
			(NOT @ScheduleRule IS NULL AND NOT [ScheduleRule] IS NULL)
		AND
			NOT [AgentID] IS NULL
	)
	BEGIN
		RAISERROR ('Error: Job is currently being executed by Agent and is locked for updates.', -- Message text.
							 16, -- Severity.
							 1 -- State.
							 )
	END
	ELSE
	BEGIN
		INSERT INTO [eddsdbo].[{0}]
		(
			[AgentTypeID]
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
		)
		OUTPUT 
			Inserted.[JobID]
			,Inserted.[AgentTypeID]
			,Inserted.[LockedByAgentID]
			,Inserted.[WorkspaceID]
			,Inserted.[RelatedObjectArtifactID]
			,Inserted.[TaskType]
			,Inserted.[NextRunTime]
			,Inserted.[LastRunTime]
			,Inserted.[ScheduleRule]
			,Inserted.[JobDetail]
			,Inserted.[JobFlags]
			,Inserted.[SubmittedDate]
			,Inserted.[SubmittedBy]
		INTO
			@job
		VALUES
		(
			@AgentTypeID
			,@LockedByAgentID
			,@WorkspaceID
			,@RelatedObjectArtifactID
			,@TaskType
			,@NextRunTime
			,NULL
			,@ScheduleRule
			,@JobDetail
			,@JobFlags
			,GETUTCDATE()
			,@SubmittedBy
		)
	END
END
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
		@job