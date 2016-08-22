SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[eddsdbo].[{0}]') AND type in (N'U'))
BEGIN
	CREATE TABLE [eddsdbo].[{0}](
		[ID] [bigint] IDENTITY(1,1) NOT NULL,
		[JobID] [bigint] NOT NULL,
		[RootJobID] [bigint] NULL,
		[ParentJobID] [bigint] NULL,
		[TaskType] [nvarchar](255) NOT NULL,
		[Status] [int] NOT NULL,
		[AgentID] [int] NULL,
		[WorkspaceID] [int] NULL,
		[RelatedObjectArtifactID] [int] NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedOn] [datetime] NOT NULL,
		[Details] [nvarchar](max) NULL,
		CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
		(
			[ID] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'RootJobID' AND [object_id] = OBJECT_ID(N'[eddsdbo].[{0}]'))
BEGIN
	 ALTER TABLE [eddsdbo].[{0}] ADD [RootJobID] [bigint] NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ParentJobID' AND [object_id] = OBJECT_ID(N'[eddsdbo].[{0}]'))
BEGIN
	 ALTER TABLE [eddsdbo].[{0}] ADD ParentJobID bigint NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'WorkspaceID' AND [object_id] = OBJECT_ID(N'[eddsdbo].[{0}]'))
BEGIN
	 ALTER TABLE [eddsdbo].[{0}] ADD WorkspaceID int NULL
END
