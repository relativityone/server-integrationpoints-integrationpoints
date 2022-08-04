using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class JobHistoryDestinationWorkspaceParser
    {
        private const char _SEPARATOR = '-';
        private const string _THIS_INSTANCE_NAME = "This Instance";
        private readonly Dictionary<ParserStateEnum, IParserStateTransition> _stateTransitions;

        public JobHistoryDestinationWorkspaceParser(int workspaceId, IFederatedInstanceManager federatedInstanceManager, IWorkspaceManager workspaceManager)
        {
            _stateTransitions = new Dictionary<ParserStateEnum, IParserStateTransition>
            {
                {ParserStateEnum.Initial, new CheckForInputNullPath(workspaceId, workspaceManager, _THIS_INSTANCE_NAME)},
                {ParserStateEnum.NeedsWorkspaceIdParsing, new ExtractWorkspaceIdFromInput(_SEPARATOR) },
                {ParserStateEnum.WorkspaceIdParsed, new CheckSeparatorsAmount(_SEPARATOR, _THIS_INSTANCE_NAME)},
                {ParserStateEnum.NeedsLocalInstanceCheck, new CheckIfHasLocalInstance(_SEPARATOR, _THIS_INSTANCE_NAME) },
                {
                    ParserStateEnum.NeedsFederatedInstanceNameParsing,
                    new ParseFederatedInstanceName(_SEPARATOR, _THIS_INSTANCE_NAME, federatedInstanceManager)
                }
            };
        }

        public DestinationWorkspaceElementsParsingResult Parse(string input)
        {
            var parserState = new CurrentParserState(input);
            ParserStateEnum currentMachineState = ParserStateEnum.Initial;

            while (currentMachineState != ParserStateEnum.Ended)
            {
                currentMachineState = _stateTransitions[currentMachineState].DoTransition(parserState);
            }

            return parserState.Result;
        }
    }
}