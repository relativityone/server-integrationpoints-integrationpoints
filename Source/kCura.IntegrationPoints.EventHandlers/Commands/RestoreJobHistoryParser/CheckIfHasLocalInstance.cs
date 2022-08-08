using System;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class CheckIfHasLocalInstance : IParserStateTransition
    {
        private readonly char _separtor;
        private readonly string _thisInstanceName;

        public CheckIfHasLocalInstance(char separtor, string thisInstanceName)
        {
            _separtor = separtor;
            _thisInstanceName = thisInstanceName;
        }

        public ParserStateEnum DoTransition(CurrentParserState state)
        {
            int firstSeparatorPos = state.Input.IndexOf(_separtor);
            if (string.Equals(state.Input.Substring(0, firstSeparatorPos - 1).TrimEnd(), _thisInstanceName, StringComparison.CurrentCultureIgnoreCase))
            {
                state.Result =
                    new DestinationWorkspaceElementsParsingResult(state.Input.Substring(firstSeparatorPos + 1).TrimStart(),
                        _thisInstanceName);
                return ParserStateEnum.Ended;
            }
            return ParserStateEnum.NeedsFederatedInstanceNameParsing;
        }
    }
}