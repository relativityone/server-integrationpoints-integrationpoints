SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM EDDSResource.sys.objects WHERE object_id = OBJECT_ID(N'[EDDSResource].[eddsdbo].[{0}]') AND type in (N'U'))
BEGIN
	CREATE TABLE [EDDSResource].[eddsdbo].[{0}](
		[ID] [bigint] IDENTITY(1,1) NOT NULL,
		[CustodianID] [nvarchar](1000) NOT NULL,
		[ManagerID] [nvarchar](1000) NOT NULL,
		[LockedByJobID] [bigint] NULL,
		[CreatedOn] [datetime] NOT NULL,
		CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
		(
			[ID] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

--TODO: Delete old tables

--delete records older than 1 day
--DELETE FROM [EDDSResource].[eddsdbo].[{0}]
--WHERE	
--			DATEDIFF(HOUR,[CreatedOn],GETUTCDATE()) > 24
--AND 
--			NOT [LockedByJobID] IS NULL

 
