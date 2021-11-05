--If a scheduled job exists in the queue, but the workspace that created that job no longer exists,
--delete the job from the queue.

DELETE FROM [eddsdbo].[{0}] WITH(UPDLOCK, READPAST, ROWLOCK)
WHERE [LockedByAgentID] IS NULL AND [WorkspaceID] NOT IN (
	SELECT [ArtifactID]
	FROM [eddsdbo].[Case] WITH(NOLOCK)
)
