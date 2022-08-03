using System.Collections.Generic;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IFieldService
    {
        /// <summary>
        /// Retrieves all text fields for given Artifact Type. This includes both Fixed-Length Text and Long Text fields.
        /// </summary>
        IEnumerable<FieldEntry> GetAllTextFields(int workspaceId, int rdoTypeId);

        /// <summary>
        /// Retrieves long text fields only for given Artifact Type.
        /// </summary>
        IEnumerable<FieldEntry> GetLongTextFields(int workspaceId, int rdoTypeId);
    }
}
