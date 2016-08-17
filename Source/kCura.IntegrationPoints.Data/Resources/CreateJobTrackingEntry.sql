﻿IF OBJECT_ID(N'{0}.[{1}]',N'U') IS NULL
BEGIN
	CREATE TABLE {0}.[{1}] 
	([JobID] bigint,
	[TotalRecords] int,
	[ErrorRecords] int,
	[Completed] bit,
	CONSTRAINT [PK_{1}] PRIMARY KEY CLUSTERED 
	(
		[JobID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF (NOT EXISTS (SELECT * FROM {0}.[{1}] WHERE JobID = @jobID))
BEGIN
	insert into {0}.[{1}] ([JobID],[Completed]) values (@jobID, 0)
END

