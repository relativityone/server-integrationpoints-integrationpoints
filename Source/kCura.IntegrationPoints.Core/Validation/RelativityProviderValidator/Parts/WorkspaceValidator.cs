using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class WorkspaceValidator : BasePartsValidator<int>
	{
		private const string _WORKSPACE_INVALID_NAME_CHAR = ";";

		private readonly IWorkspaceRepository _workspaceRepository;

		private readonly string _prefix;

		public WorkspaceValidator(IWorkspaceRepository workspaceRepository, string prefix)
		{
			_workspaceRepository = workspaceRepository;
			_prefix = prefix;
		}

		public override ValidationResult Validate(int value)
		{
			var result = new ValidationResult();

			try
			{
				WorkspaceDTO workspaceDto = _workspaceRepository.Retrieve(value);

				if (workspaceDto.Name.Contains(_WORKSPACE_INVALID_NAME_CHAR))
				{
					result.Add($"{_prefix} {RelativityProviderValidationMessages.WORKSPACE_INVALID_NAME}");
				}
			}
			catch
			{
				result.Add($"{_prefix} {RelativityProviderValidationMessages.WORKSPACE_NOT_EXIST}");
			}

			return result;
		}
	}
}