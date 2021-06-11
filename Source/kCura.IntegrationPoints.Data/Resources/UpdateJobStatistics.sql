UPDATE {0}.[{1}]
	SET
		[TotalRecords] += @total,
		[ErrorRecords] += @errored,
		[ImportApiErrors] += @importApiErrors
	WHERE
		[JobId] = @jobID

SELECT
	SUM(COALESCE([TotalRecords],0)) as [TotalRecords],
	SUM(COALESCE([ErrorRecords],0)) as [ErrorRecords],
	SUM(COALESCE([ImportApiErrors],0)) as [ImportApiErrors]
FROM {0}.[{1}]
