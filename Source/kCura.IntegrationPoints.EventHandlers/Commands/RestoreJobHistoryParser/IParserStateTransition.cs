namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public interface IParserStateTransition
    {
        ParserStateEnum DoTransition(CurrentParserState state);
    }
}