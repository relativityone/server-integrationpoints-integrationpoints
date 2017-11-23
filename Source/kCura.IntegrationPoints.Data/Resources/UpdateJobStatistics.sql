UPDATE {0}.[{1}]
	SET
		[TotalRecords] = @total,
		[ErrorRecords] = @errored,
		[ImportErrors] = @importErrors
	WHERE
		[JobId] = @jobID

SELECT
	SUM(COALESCE([TotalRecords],0)) as [TotalRecords],
	SUM(COALESCE([ErrorRecords],0)) as [ErrorRecords],
	SUM(COALESCE([ImportErrors],0)) as [ImportErrors]
FROM {0}.[{1}]
