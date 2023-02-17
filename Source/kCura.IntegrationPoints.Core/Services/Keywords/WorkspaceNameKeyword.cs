using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
    public class WorkspaceNameKeyword : IKeyword
    {
        private readonly ICaseServiceContext _context;
        private readonly IWorkspaceRepository _workspaceRepository;

        public string KeywordName { get { return "\\[WORKSPACE.NAME]"; } }

        // TODO: Resolve workspaceId instead of ICaseServiceContext
        public WorkspaceNameKeyword(ICaseServiceContext context, IWorkspaceRepository workspaceRepository)
        {
            _context = context;
            _workspaceRepository = workspaceRepository;
        }

        public string Convert()
        {
            string workspaceName = _workspaceRepository.Retrieve(_context.WorkspaceID).Name;
            return workspaceName;
        }
    }
}
