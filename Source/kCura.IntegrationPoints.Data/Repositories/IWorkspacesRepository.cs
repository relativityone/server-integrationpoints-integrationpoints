using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Responsible for handling Workspaces functionality
    /// </summary>
    public interface IWorkspacesRepository
    {
        /// <summary>
        /// Retrieves all active workspaces which also are not pending for or upgrading
        /// </summary>
        /// <returns>A collection of WorkspaceDTO objects</returns>
        IEnumerable<WorkspaceDTO> RetrieveAllActive();
    }
}