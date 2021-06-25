select q.LockedByAgentID, q.StopState
from [EDDS].[eddsdbo].[{0}] as q
inner join {1} as s
on q.JobID = s.JobID
where q.RootJobID = @jobID