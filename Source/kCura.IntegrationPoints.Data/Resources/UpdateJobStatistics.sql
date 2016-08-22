UPDATE {0}.[{1}]
	SET
		[TotalRecords] = @total,
		[ErrorRecords] = @errored
	WHERE
		[JobId] = @jobID

SELECT
	SUM(COALESCE([TotalRecords],0)) as [TotalRecords],
	SUM(COALESCE([ErrorRecords],0)) as [ErrorRecords]
FROM {0}.[{1}]
