using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IQueryFieldLookupRepository
	{
		/// <summary>
		/// Returns cached ViewFieldInfo object.
		/// </summary>
		/// <param name="fieldArtifactId"></param>
		/// <returns></returns>
		ViewFieldInfo GetFieldByArtifactId(int fieldArtifactId);

		/// <summary>
		/// Returns cached ViewField type as string.
		/// </summary>
		/// <param name="fieldArtifactId"></param>
		/// <returns></returns>
		string GetFieldTypeByArtifactId(int fieldArtifactId);

		/// <summary>
		/// Returns uncached ViewFieldInfoFieldTypeExtender.
		/// </summary>
		/// <param name="fieldArtifactId"></param>
		/// <returns></returns>
		ViewFieldInfoFieldTypeExtender RunQueryForViewFieldInfo(int fieldArtifactId);

	}
}
