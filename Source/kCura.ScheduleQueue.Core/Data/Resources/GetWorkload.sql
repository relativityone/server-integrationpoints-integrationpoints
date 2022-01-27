SELECT 
			COUNT(*)
FROM
			[eddsdbo].[{0}] WITH(NOLOCK)
WHERE
            [NextRunTime] <= GETUTCDATE()