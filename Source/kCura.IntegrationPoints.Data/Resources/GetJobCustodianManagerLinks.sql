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
	ON		t1.[CustodianID] = t2.[CustodianID] AND t1.[ManagerID] = t2.[ManagerID] 
WHERE
				t1.[LockedByJobID] IS NULL
				

--get next batch
UPDATE			{0}.[{1}]
SET
				[LockedByJobID]	= @JobID
OUTPUT 
				INSERTED.[CustodianID],
				INSERTED.[ManagerID]
FROM 
				{0}.[{1}] t1
WHERE
				t1.[LockedByJobID] IS NULL
