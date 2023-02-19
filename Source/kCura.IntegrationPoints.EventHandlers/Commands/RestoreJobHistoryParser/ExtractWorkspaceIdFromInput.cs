using kCura.IntegrationPoints.Core.Managers.Implementations;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class ExtractWorkspaceIdFromInput : IParserStateTransition
    {
        private readonly char _separtor;

        public ExtractWorkspaceIdFromInput(char separtor)
        {
            _separtor = separtor;
        }

        public ParserStateEnum DoTransition(CurrentParserState state)
        {
            state.WorkspaceIdSeparatorPos = state.Input.LastIndexOf(_separtor);
            if (state.WorkspaceIdSeparatorPos == -1)
            {
                return EndParsingWithEmptyResult(state);
            }

            int? workspaceId = ParseWorkspaceId(state.Input.Substring(state.WorkspaceIdSeparatorPos + 1));
            if (workspaceId == null)
            {
                return EndParsingWithEmptyResult(state);
            }

            state.WorkspaceId = workspaceId.Value;
            return ParserStateEnum.WorkspaceIdParsed;
        }

        private static ParserStateEnum EndParsingWithEmptyResult(CurrentParserState state)
        {
            state.Result = DestinationWorkspaceElementsParsingResult.Empty;
            return ParserStateEnum.Ended;
        }

        private int? ParseWorkspaceId(string input)
        {
            int workspaceId;
            if (int.TryParse(input.Trim(), out workspaceId))
            {
                return workspaceId;
            }
            return null;
        }
    }
}
