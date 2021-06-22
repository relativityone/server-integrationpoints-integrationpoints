select q.LockedByAgentID, q.StopState, s.Completed
from {0} as q
inner join {1} as s
on q.JobID = s.JobID
where q.LockedByAgentID <> NULL and q.RootJobID = @jobID