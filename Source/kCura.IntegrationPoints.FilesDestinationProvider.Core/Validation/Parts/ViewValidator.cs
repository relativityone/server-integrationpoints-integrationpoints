using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public class ViewValidator : BasePartsValidator<ExportSettings>
	{
		private readonly IViewService _viewService;

		public ViewValidator(IViewService viewService)
		{
			_viewService = viewService;
		}

		public override ValidationResult Validate(ExportSettings value)
		{
			var result = new ValidationResult();

			var view = _viewService.GetViewsByWorkspaceAndArtifactType(value.WorkspaceId, value.ArtifactTypeId)
				.FirstOrDefault(x => x.ArtifactId.Equals(value.ViewId));

			if (view == null)
			{
				result.Add(FileDestinationProviderValidationMessages.VIEW_NOT_EXIST);
			}

			return result;
		}
	}
}