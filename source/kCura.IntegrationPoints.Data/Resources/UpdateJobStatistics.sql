
declare @update nvarchar(max) = '
UPDATE [EDDSRESOURCE].[EDDSDBO].[' + @tableName + ']
	SET
		[TotalRecords] = @total,
		[ErrorRecords] = @errored
	WHERE
		[JobId] = @id

SELECT
	SUM(COALESCE([TotalRecords],0)) as [TotalRecords],
	SUM(COALESCE([ErrorRecords],0)) as [ErrorRecords]
FROM [EDDSRESOURCE].[EDDSDBO].[' + @tableName + ']';
DECLARE @uParams nvarchar(max) = N'@id bigint, @total bigint, @errored bigint';
EXECUTE sp_executesql @update, @uParams, @id = @jobID, @total = @total, @errored = @errored
