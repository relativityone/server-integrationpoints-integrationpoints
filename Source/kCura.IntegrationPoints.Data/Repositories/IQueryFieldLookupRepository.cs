using Relativity;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IQueryFieldLookupRepository
    {
        /// <summary>
        /// Returns cached ViewFieldInfo object.
        /// </summary>
        /// <param name="fieldArtifactID"></param>
        /// <returns></returns>
        ViewFieldInfo GetFieldByArtifactID(int fieldArtifactID);

        /// <summary>
        /// Returns cached ViewField type as string.
        /// </summary>
        /// <param name="fieldArtifactID"></param>
        /// <returns></returns>
        FieldTypeHelper.FieldType GetFieldTypeByArtifactID(int fieldArtifactID);

        /// <summary>
        /// Returns uncached ViewFieldInfo.
        /// </summary>
        /// <param name="fieldArtifactID"></param>
        /// <returns></returns>
        ViewFieldInfo RunQueryForViewFieldInfo(int fieldArtifactID);

    }
}
