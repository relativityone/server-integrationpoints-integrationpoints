/* 
This SQL script will create the table necessary for the SQLServerProvider for
the Relativity.Toggles library
*/

IF NOT EXISTS( SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Toggle' )
BEGIN

	CREATE TABLE [Toggle] (
				[Name] nvarchar(100) NOT NULL,
				[IsEnabled] bit NOT NULL,
				CONSTRAINT PK_Toggle PRIMARY KEY CLUSTERED ([Name])
	)

END
