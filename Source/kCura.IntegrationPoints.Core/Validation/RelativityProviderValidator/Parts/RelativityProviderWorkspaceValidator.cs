using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderWorkspaceValidator : BasePartsValidator<int>
    {
        private const string _WORKSPACE_INVALID_NAME_CHAR = ";";

        private readonly IWorkspaceManager _workspaceManager;

        private readonly string _prefix;

        public RelativityProviderWorkspaceValidator(IWorkspaceManager workspaceManager, string prefix)
        {
            _workspaceManager = workspaceManager;
            _prefix = prefix;
        }

        public override ValidationResult Validate(int value)
        {
            var result = new ValidationResult();

            try
            {
                WorkspaceDTO workspaceDto = _workspaceManager.RetrieveWorkspace(value);

                if (workspaceDto.Name.Contains(_WORKSPACE_INVALID_NAME_CHAR))
                {
                    result.Add($"{_prefix} {RelativityProviderValidationMessages.WORKSPACE_INVALID_NAME}");
                }
            }
            catch
            {
                result.Add($"{_prefix} {IntegrationPointProviderValidationMessages.WORKSPACE_NOT_EXIST}");
            }

            return result;
        }
    }
}