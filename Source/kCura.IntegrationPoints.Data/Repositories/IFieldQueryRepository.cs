using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    ///     Responsible for handling Relativity fields
    /// </summary>
    public interface IFieldQueryRepository
    {
        /// <summary>
        ///     Retrieves the long text fields for an rdo
        /// </summary>
        /// <param name="rdoTypeID">The artifact id of the rdo's type</param>
        /// <returns>An array of ArtifactFieldDTO for the rdo</returns>
        Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeID);

        /// <summary>
        ///     Retrieves fields for an rdo
        /// </summary>
        /// <param name="rdoTypeID">The artifact id of the rdo's type</param>
        /// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
        /// <returns>An array of ArtifactDTO with populated fields for the given rdo type</returns>
        Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeID, HashSet<string> fieldNames);

        /// <summary>
        ///     Retrieves fields for an rdo
        /// </summary>
        /// <param name="rdoTypeID">The artifact id of the rdo's type</param>
        /// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
        /// <returns>An array of ArtifactDTO with populated fields for the given rdo type</returns>
        ArtifactDTO[] RetrieveFields(int rdoTypeID, HashSet<string> fieldNames);

        /// <summary>
        ///     Retrieves field Artifact if for rdo, display name and field type
        /// </summary>
        /// <param name="rdoTypeID">The artifact id of the rdo's type</param>
        /// <param name="displayName">The Display Name of the rdo's</param>
        /// <param name="fieldType">The Field Type of the rdo's</param>
        /// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
        /// <returns>Field artifact id</returns>
        ArtifactDTO RetrieveField(int rdoTypeID, string displayName, string fieldType, HashSet<string> fieldNames);

        /// <summary>
        ///     Retrieves the identifier field. NOTE : the returns ArtifactDTO contains name and 'is identifier' fields
        /// </summary>
        /// <param name="rdoTypeID"></param>
        /// <returns>the ArtifactDTO represents the identifier field of the object</returns>
        /// <remarks>the returns ArtifactDTO contains name and 'is identifier' fields</remarks>
        ArtifactDTO RetrieveIdentifierField(int rdoTypeID);

        /// <summary>
        /// Reads Artifact ID for given GUID
        /// </summary>
        int ReadArtifactID(Guid guid);

        /// <summary>
        ///     Retrieves potential begin bates fields
        /// </summary>
        /// <returns>A list of Artifact fields DTO</returns>
        ArtifactFieldDTO[] RetrieveBeginBatesFields();

        /// <summary>
        ///     Retrieves the artifact view field id for the given field
        /// </summary>
        /// <param name="fieldArtifactID">The artifact id of the field</param>
        /// <returns>The artifact view field id if found, <code>NULL</code> otherwise</returns>
        int? RetrieveArtifactViewFieldId(int fieldArtifactID);
    }
}