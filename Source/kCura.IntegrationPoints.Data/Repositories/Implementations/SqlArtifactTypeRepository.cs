
using System.Data;
using System.Data.SqlClient;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SqlArtifactTypeRepository : IArtifactTypeRepository
	{
		private readonly BaseContext _context;

		public SqlArtifactTypeRepository(BaseContext context)
		{
			_context = context;
		}

		public int GetArtifactTypeIdFromArtifactTypeName(string artifactTypeName)
		{
			const string artifactTypeParameterId = "@ArtifactType";
			string sql = $"SELECT [ArtifactTypeID] FROM [EDDSDBO].[ArtifactType] WHERE [ArtifactType] = {artifactTypeParameterId}";
			var artifactParameter = new SqlParameter(artifactTypeParameterId, SqlDbType.VarChar) { Value = artifactTypeName };

			int artifactTypeId = _context.DBContext.ExecuteSqlStatementAsScalar<int>(sql, new[] { artifactParameter });

			return artifactTypeId;
		}
	}
}