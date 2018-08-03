--bypass duplicate records
UPDATE	{0}.[{1}]
SET
				[LockedByJobID]	= -1
FROM 
				{0}.[{1}] t1 
JOIN
				(
					SELECT * FROM {0}.[{1}] WHERE NOT [LockedByJobID] IS NULL
				) t2
	ON		t1.[EntityID] = t2.[EntityID] AND t1.[ManagerID] = t2.[ManagerID] 
WHERE
				t1.[LockedByJobID] IS NULL
				

--get next batch
UPDATE			{0}.[{1}]
SET
				[LockedByJobID]	= @JobID
OUTPUT 
				INSERTED.[EntityID],
				INSERTED.[ManagerID]
FROM 
				{0}.[{1}] t1
WHERE
				t1.[LockedByJobID] IS NULL
