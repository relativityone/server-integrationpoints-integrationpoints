using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Utils;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class CheckForInputNullPath : IParserStateTransition
    {
        private string _workspaceName;
        private readonly int _workspaceId;
        private readonly IWorkspaceManager _workspaceManager;
        private readonly string _thisInstanceName;

        public CheckForInputNullPath(int workspaceId, IWorkspaceManager workspaceManager, string thisInstanceName)
        {
            _workspaceId = workspaceId;
            _workspaceManager = workspaceManager;
            _thisInstanceName = thisInstanceName;
        }
        public ParserStateEnum DoTransition(CurrentParserState state)
        {
            if (state.Input != null)
            {
                return ParserStateEnum.NeedsWorkspaceIdParsing;
            }
            string destinationWorkspace = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(GetWorkspaceName(), _workspaceId);
            state.Result = new DestinationWorkspaceElementsParsingResult(destinationWorkspace, _thisInstanceName);
            return ParserStateEnum.Ended;
        }

        private string GetWorkspaceName()
        {
            return _workspaceName ?? (_workspaceName = _workspaceManager.RetrieveWorkspace(_workspaceId).Name);
        }
    }
}