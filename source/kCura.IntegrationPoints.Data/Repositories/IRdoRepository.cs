using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Rsapi Rdo Repository functionality
	/// </summary>
	public interface IRdoRepository
	{
		/// <summary>
		/// Queries an Rdo instance based on query conditions
		/// </summary>
		/// <param name="query">The rdo query with desired conditions</param>
		/// <returns>An Rdo QueryResultSet based on the query provided</returns>
		QueryResultSet<RDO> Query(Query<RDO> query);

		RDO QuerySingle(Query<RDO> query);

		int Create(RDO rdo);

		void Update(RDO rdo);

		RDO ReadSingle(int artifactId);
	}
}
