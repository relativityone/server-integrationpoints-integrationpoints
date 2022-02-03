using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class ProductionHelper : WorkspaceHelperBase
    {
        public ProductionHelper(WorkspaceTest workspace) : base(workspace)
        {
        }

        public IList<ProductionTest> GetAllProductions()
        {
            return Workspace.Productions;
        }
    }
}
