using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class ParseFederatedInstanceName : IParserStateTransition
    {
        private List<FederatedInstanceDto> _federatedInstances;
        private readonly char _separator;
        private readonly IFederatedInstanceManager _federatedInstanceManager;
        private readonly string _thisInstanceName;

        public ParseFederatedInstanceName(char separator, string thisInstanceName, IFederatedInstanceManager federatedInstanceManager)
        {
            _separator = separator;
            _thisInstanceName = thisInstanceName;
            _federatedInstanceManager = federatedInstanceManager;
        }

        public ParserStateEnum DoTransition(CurrentParserState state)
        {
            int currentSeparatorPos = -1;
            FederatedInstanceDto federatedInstance;
            do
            {
                currentSeparatorPos = state.Input.IndexOf(_separator, currentSeparatorPos + 1);
                string currentlyCheckedName = state.Input.Substring(0, currentSeparatorPos).Trim();
                federatedInstance = TryGetFederatedInstanceByName(currentlyCheckedName);
            } while (federatedInstance == null && currentSeparatorPos != state.WorkspaceIdSeparatorPos && currentSeparatorPos != -1);
            if (federatedInstance == null)
            {
                state.Result = new DestinationWorkspaceElementsParsingResult(state.Input, _thisInstanceName);
            }
            else
            {
                state.Result =
                    new DestinationWorkspaceElementsParsingResult(state.Input.Substring(currentSeparatorPos + 1).TrimStart(), federatedInstance.Name, federatedInstance.ArtifactId);
            }
            return ParserStateEnum.Ended;
        }

        private FederatedInstanceDto TryGetFederatedInstanceByName(string name)
        {
            if (_federatedInstances == null)
            {
                _federatedInstances = _federatedInstanceManager.RetrieveAll().ToList();
            }
            return _federatedInstances.FirstOrDefault(instance => instance.Name == name);
        }
    }
}
