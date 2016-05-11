using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SqlArtifactGuidRepository : IArtifactGuidRepository
	{
		private readonly BaseContext _context;

		public SqlArtifactGuidRepository(BaseContext context)
		{
			_context = context;
		}

		public void InsertArtifactGuidForArtifactId(int artifactId, Guid guid)
		{
			const string insertSql = @"
			INSERT INTO [EDDSDBO].[ArtifactGuid] (ArtifactID, ArtifactGuid)
			VALUES (@ArtifactID, @ArtifactGuid)";

			var artifactIdParam = new SqlParameter("@ArtifactID", SqlDbType.Int) { Value = artifactId };
			var artifactGuidParam = new SqlParameter("@ArtifactGuid", SqlDbType.UniqueIdentifier) { Value = guid };

			_context.DBContext.ExecuteNonQuerySQLStatement(
				insertSql, 
				new [] {artifactIdParam, artifactGuidParam});
		}

		public void InsertArtifactGuidsForArtifactIds(IDictionary<Guid, int> guidToIdDictionary)
		{
			var sqlParameters = new List<SqlParameter>();
			var valueStrings = new List<string>();

			int i = 0;
			foreach(Guid key in guidToIdDictionary.Keys)
			{
				string artifactIdString = $"@ArtifactID{i}";
				string artifactGuidString = $"@ArtifactGuid{i}";
				int artifactId = guidToIdDictionary[key];
				var artifactIdParam = new SqlParameter(artifactIdString, SqlDbType.Int) { Value =  artifactId };
				var artifactGuidParam = new SqlParameter(artifactGuidString, SqlDbType.UniqueIdentifier) { Value = key };

				valueStrings.Add($"({artifactIdString}, {artifactGuidString})");

				sqlParameters.AddRange(new[] { artifactIdParam, artifactGuidParam });
				i++;
			}

			string insertSql = $@"
			INSERT INTO [EDDSDBO].[ArtifactGuid] (ArtifactID, ArtifactGuid)
			VALUES {String.Join(",", valueStrings)}";

			_context.DBContext.ExecuteNonQuerySQLStatement(
				insertSql,
				sqlParameters);
		}

		public bool GuidExists(Guid guid)
		{
			const string artifactGuidParamName = "@ArtifactGuid";
			string guidCheckSql = $@"
			SELECT CASE WHEN EXISTS (
				SELECT [ArtifactID]
				FROM [EDDSDBO].[ArtifactGuid] WITH (NOLOCK)
				WHERE [ArtifactGuid] = {artifactGuidParamName})
			THEN 1
			ELSE 0
			END as Result";

			var guidParameter = new SqlParameter(artifactGuidParamName, SqlDbType.UniqueIdentifier) { Value = guid };
			bool guidExists = _context.DBContext.ExecuteSqlStatementAsScalar<bool>(guidCheckSql, new[] { guidParameter });

			return guidExists;
		}

	    public IDictionary<Guid, bool> GuidsExist(IEnumerable<Guid> guids)
		{
			List<Guid> guidList = guids.ToList();
			string guidCSV = String.Join(",", guidList.Select(x => $"'{x}'"));
			string guidCheckSql = $@"
			SELECT [ArtifactGuid], [ArtifactID]
			FROM [EDDSDBO].[ArtifactGuid] WITH (NOLOCK)
			WHERE [ArtifactGuid] in ({guidCSV})";

			IDictionary<Guid, bool> guidDictionary = guidList.ToDictionary(x => x, y => false);
			using (SqlDataReader dataReader = _context.DBContext.ExecuteSQLStatementAsReader(guidCheckSql))
			{
				while (dataReader.Read())
				{
					Guid guid = dataReader.GetGuid(0);
					guidDictionary[guid] = true;
				}
			}

			return guidDictionary;
		}

        public Dictionary<int, Guid> GetGuidsForArtifactIds(IEnumerable<int> artifactIds)
        {
            var output = new Dictionary<int, Guid>();
            string artifactIdCsv = String.Join(",", artifactIds);
            string sql =  $"SELECT [ArtifactID],[ArtifactGuid] FROM [eddsdbo].[ArtifactGuid] WHERE [ArtifactID] IN ({artifactIdCsv})";

            DataTable result = _context.DBContext.ExecuteSqlStatementAsDataTable(sql);

            if (result != null && result.Rows != null && result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    output.Add((int)row[0], new Guid(row[1].ToString()));
                }
            }

            return output;
        }

		public Dictionary<Guid, int> GetArtifactIdsForGuids(IEnumerable<Guid> guids)
		{
			var output = new Dictionary<Guid, int>();
			string guidsCsv = $"'{String.Join("','", guids)}'";
			string sql = $"SELECT [ArtifactGuid], [ArtifactID] FROM [eddsdbo].[ArtifactGuid] WHERE [ArtifactGuid] IN ({guidsCsv})";

			DataTable result = _context.DBContext.ExecuteSqlStatementAsDataTable(sql);

			if (result != null && result.Rows != null && result.Rows.Count > 0)
			{
				foreach (DataRow row in result.Rows)
				{
					output.Add(new Guid(row[0].ToString()), (int)row[1]);
				}
			}

			return output;
		}
	}
}