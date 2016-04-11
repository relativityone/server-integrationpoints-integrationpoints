using System;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.Helpers;
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
	}
}