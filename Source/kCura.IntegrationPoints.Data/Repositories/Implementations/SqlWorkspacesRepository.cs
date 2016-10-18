using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Core;
using Relativity.Data.JobQueueing;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class SqlWorkspacesRepository : IWorkspacesRepository
    {
        private readonly BaseContext _context;
        private readonly List<int> _allowedWorkspaceStates;

        public SqlWorkspacesRepository(BaseContext context)
        {
            _context = context;

            _allowedWorkspaceStates = new List<int>
            {
                (int)WorkspaceUpgrade.WorkspaceUpgradeStatus.Completed
            };
        }

        public IEnumerable<WorkspaceDTO> RetrieveAllActive()
        {
            var workspaces = new List<WorkspaceDTO>();

            var dataview = WorkspaceUpgradeJobQueries.RetrieveAll(_context.DBContext);
            foreach (DataRow row in dataview.Table.Rows)
            {
                if (_allowedWorkspaceStates.Contains(int.Parse(row["Status"].ToString())))
                {
                    workspaces.Add(new WorkspaceDTO
                    {
                        ArtifactId = int.Parse(row["WorkspaceArtifactID"].ToString()),
                        Name = row["Name"].ToString()
                    });
                }
            }

            return workspaces;
        }
    }
}