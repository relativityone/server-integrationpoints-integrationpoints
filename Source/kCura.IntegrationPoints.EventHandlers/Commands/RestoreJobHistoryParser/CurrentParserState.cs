namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class CurrentParserState
    {
        public CurrentParserState(string input)
        {
            Input = input;
        }

        public string Input { get; set; }
        public int WorkspaceIdSeparatorPos { get; set; }
        public int SeparatorsCount { get; set; }

        public int WorkspaceId { get; set; }

        public DestinationWorkspaceElementsParsingResult Result { get; set; }
    }
}