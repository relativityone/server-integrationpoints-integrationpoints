SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[eddsdbo].[{0}]') AND type in (N'U'))
BEGIN
	CREATE TABLE [eddsdbo].[{0}](
		[ID] [bigint] IDENTITY(1,1) NOT NULL,
		[JobID] [bigint] NOT NULL,
		[TaskType] [nvarchar](255) NOT NULL,
		[Status] [int] NOT NULL,
		[AgentID] [int] NULL,
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
