using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class PreCascadeDeleteIntegrationPointCommand : IEHCommand
    {
        private readonly IEHContext _context;
        private readonly IPreCascadeDeleteEventHandlerValidator _preCascadeDeleteValidator;
        private readonly IDeleteHistoryService _deleteHistoryService;
        private readonly IArtifactsToDelete _artifactsToDelete;

        public PreCascadeDeleteIntegrationPointCommand(IEHContext context, IPreCascadeDeleteEventHandlerValidator preCascadeDeleteValidator, IDeleteHistoryService deleteHistoryService,
            IArtifactsToDelete artifactsToDelete)
        {
            _context = context;
            _preCascadeDeleteValidator = preCascadeDeleteValidator;
            _deleteHistoryService = deleteHistoryService;
            _artifactsToDelete = artifactsToDelete;
        }


        public void Execute()
        {
            List<int> artifactIds = _artifactsToDelete.GetIds();
            artifactIds.ForEach(artifactId => _preCascadeDeleteValidator.Validate(_context.Helper.GetActiveCaseID(), artifactId));
            artifactIds.ForEach(artifactId => _deleteHistoryService.DeleteHistoriesAssociatedWithIP(artifactId));
        }
    }
}