UPDATE			{0}.[{1}]
SET
				[LockedByJobID]	= NULL
FROM 
				{0}.[{1}] t1 WITH (UPDLOCK, ROWLOCK)
WHERE
				t1.[LockedByJobID] = @JobID
