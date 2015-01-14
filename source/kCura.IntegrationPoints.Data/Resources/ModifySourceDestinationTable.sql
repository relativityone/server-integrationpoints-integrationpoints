IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE Name = N'LibFileStream' and Object_ID = Object_ID(N'EDDSDBO.SourceProvider'))
BEGIN
		ALTER TABLE EDDSDBO.SourceProvider 
		ADD	LibFileStream varbinary(MAX) NULL
END