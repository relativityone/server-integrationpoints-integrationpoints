--bypass duplicate records
UPDATE	[EDDSResource].[eddsdbo].[{0}]
SET
				[LockedByJobID]	= -1
FROM 
				[EDDSResource].[eddsdbo].[{0}] t1 
JOIN
				(
					SELECT * FROM [EDDSResource].[eddsdbo].[{0}] WHERE NOT [LockedByJobID] IS NULL
				) t2
	ON		t1.[CustodianID] = t2.[CustodianID] AND t1.[ManagerID] = t2.[ManagerID] 
WHERE
				t1.[LockedByJobID] IS NULL
				

--get next batch
UPDATE	[EDDSResource].[eddsdbo].[{0}]
SET
				[LockedByJobID]	= @JobID
OUTPUT 
				INSERTED.[CustodianID],
				INSERTED.[ManagerID]
FROM 
				[EDDSResource].[eddsdbo].[{0}] t1
WHERE
				t1.[LockedByJobID] IS NULL
