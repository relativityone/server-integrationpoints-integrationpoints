namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public enum ParserStateEnum
    {
        Initial,
        NeedsWorkspaceIdParsing,
        WorkspaceIdParsed,
        NeedsLocalInstanceCheck,
        NeedsFederatedInstanceNameParsing,
        Ended
    }
}
