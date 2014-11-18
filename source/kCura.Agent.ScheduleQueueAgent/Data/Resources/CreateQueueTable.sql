/****** 
Script Number:		1
Script Date:		11/13/2014 10:10:00 
Script Creator:		Art Kelenzon
Script Description:	Creating Schedule Queue table and corresponding indexes
******/
USE [EDDS]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[eddsdbo].[{0}]') AND type in (N'U'))
BEGIN
CREATE TABLE [eddsdbo].[{0}](
	[JobID] [bigint] IDENTITY(1,1) NOT NULL,
	[AgentTypeID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[LockedByAgentID] [int] NULL,
	[WorkspaceID] [int] NOT NULL,
	[RelatedObjectArtifactID] [int] NULL,
	[TaskType] [nvarchar](255) NOT NULL,
	[NextRunTime] [datetime] NOT NULL,
	[LastRunTime] [datetime] NULL,
	[ScheduleRules] [nvarchar](max) NULL,
	[JobDetail] [nvarchar](max) NULL,
	[JobFlags] [int] NOT NULL,
	[SubmittedDate] [datetime] NOT NULL,
	[SubmittedBy] [int] NOT NULL,
 CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
(
	[JobID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[eddsdbo].[{0}]') AND name = N'IX_{0}_LockedByAgentID_AgentTypeID_NextRunTime')
CREATE NONCLUSTERED INDEX [IX_{0}_LockedByAgentID_AgentTypeID_NextRunTime] ON [eddsdbo].[{0}] 
(
	[LockedByAgentID] ASC,
	[AgentTypeID] ASC,
	[NextRunTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[eddsdbo].[{0}]') AND name = N'IX_{0}_WorkspaceID_RelatedObjectArtifactID')
CREATE NONCLUSTERED INDEX [IX_{0}_WorkspaceID_RelatedObjectArtifactID] ON [eddsdbo].[{0}] 
(
	[WorkspaceID] ASC,
	[RelatedObjectArtifactID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
