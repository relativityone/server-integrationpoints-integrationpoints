using System;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class DeleteIntegrationPointCommand : IEHCommand
    {
        private readonly ICorrespondingJobDelete _correspondingJobDelete;
        private readonly IIntegrationPointSecretDelete _integrationPointSecretDelete;
        private readonly IEHContext _context;

        public DeleteIntegrationPointCommand(ICorrespondingJobDelete correspondingJobDelete, IIntegrationPointSecretDelete integrationPointSecretDelete, IEHContext context)
        {
            _correspondingJobDelete = correspondingJobDelete;
            _integrationPointSecretDelete = integrationPointSecretDelete;
            _context = context;
        }

        public void Execute()
        {
            var workspaceId = _context.Helper.GetActiveCaseID();
            var integrationPointId = _context.ActiveArtifact.ArtifactID;

            try
            {
                _correspondingJobDelete.DeleteCorrespondingJob(workspaceId, integrationPointId);
            }
            catch (Exception ex)
            {
                throw new CommandExecutionException($"Failed to delete corresponding job(s). Error: {ex.Message}", ex);
            }

            try
            {
                _integrationPointSecretDelete.DeleteSecret(integrationPointId);
            }
            catch (Exception ex)
            {
                throw new CommandExecutionException($"Failed to delete corresponding secret. Error: {ex.Message}", ex);
            }
        }
    }
}