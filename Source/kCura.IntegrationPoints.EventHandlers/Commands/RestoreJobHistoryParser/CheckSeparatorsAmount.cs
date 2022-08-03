using System.Linq;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class CheckSeparatorsAmount : IParserStateTransition
    {
        private readonly char _separtor;
        private readonly string _thisInstanceName;

        public CheckSeparatorsAmount(char separtor, string thisInstanceName)
        {
            _separtor = separtor;
            _thisInstanceName = thisInstanceName;
        }

        public ParserStateEnum DoTransition(CurrentParserState state)
        {
            state.SeparatorsCount = state.Input.Count(c => c == _separtor);
            if (state.SeparatorsCount < 2)
            {
                state.Result = new DestinationWorkspaceElementsParsingResult(state.Input, _thisInstanceName);
                return ParserStateEnum.Ended;
            }
            return ParserStateEnum.NeedsLocalInstanceCheck;
        }
    }
}