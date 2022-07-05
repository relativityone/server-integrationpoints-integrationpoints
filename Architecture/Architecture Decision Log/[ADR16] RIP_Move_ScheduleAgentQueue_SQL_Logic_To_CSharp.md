# Move ScheduleAgentQueue SQL logic to C#

## Status

Proposed

## Context

We have 20+ SQL scripts used for ScheduleAgentQueue table management. These scripts contain a lot of business logic which is very hard to maintain, modify. In addition, they are unfit for testing and imposible to debug through.

The second problem is - we are tied to SQL storage because of these scripts.

## Decision

The idea is to move the business logic from SQL scripts to C# code to make it maintainable and to become independent from SQL storage by hiding ScheduleAgentQueue table access behind abstraction layer - this will enable us in the future to replace current storage to other one (we look towards no-sql).

The proposition is to use Dapper ORM and repository pattern. 

Dapper is a micro-ORM (object-related mapper), it allows to connect object code to a relational database. Micro means, it has truncated functionality compared to full-blown ORMs like Entity Framework, but a big advantage of Dapper is it has no performance overhead.

We will still need to have some SQL code, but we will try to keep it as simple as possible to ensure basic CRUD operations, the business logic will be moved to the repository class.

## Sample implementation

```csharp
public class JobModel
{
		public long JobId { get; private set; }
		public long? RootJobId { get; private set; }
		public long? ParentJobId { get; private set; }
        public int AgentTypeID { get; private set; }
        public int? LockedByAgentID { get; private set; }
		public int WorkspaceID { get; private set; }

        ...
}

public class JobRepository : IJobRepository
{
    public async Task<IReadOnlyList<JobModel>> GetAllAsync()
    {
        var sql = "SELECT * FROM ScheduleAgentQueue";
        using (var connection = new SqlConnection(configuration.GetConnectionString("EDDS")))
        {
            connection.Open();
            var result = await connection.QueryAsync<JobModel>(sql);
            return result.ToList();
        }
    }

    public async Task<JobModel> GetByIdAsync(int id)
    {
        var sql = "SELECT * FROM ScheduleAgentQueue WHERE JobID = @Id";
        using (var connection = new SqlConnection(configuration.GetConnectionString("EDDS")))
        {
            connection.Open();
            var result = await connection.QuerySingleOrDefaultAsync<JobModel>(sql, new { Id = id });
            return result;
        }
    }

    public async Task<int> DeleteAsync(int id)
    {
        var sql = "DELETE FROM ScheduleAgentQueue WHERE JobID = @Id";
        using (var connection = new SqlConnection(configuration.GetConnectionString("EDDS")))
        {
            connection.Open();
            var result = await connection.ExecuteAsync(sql, new { Id = id });
            return result;
        }
    }
}
```

## References

* <https://www.learndapper.com/>