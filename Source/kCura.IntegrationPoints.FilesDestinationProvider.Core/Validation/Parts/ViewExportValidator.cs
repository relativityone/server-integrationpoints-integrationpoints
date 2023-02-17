using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public class ViewExportValidator : BasePartsValidator<ExportSettings>
    {
        private readonly IViewService _viewService;
        private readonly IAPILog _logger;

        public ViewExportValidator(IAPILog logger, IViewService viewService)
        {
            _logger = logger;
            _viewService = viewService;
        }

        public override ValidationResult Validate(ExportSettings value)
        {
            var result = new ValidationResult();
            ViewDTO view = RetrieveView(value);

            if (view == null)
            {
                result.Add(FileDestinationProviderValidationMessages.VIEW_NOT_EXIST);
            }

            return result;
        }

        private ViewDTO RetrieveView(ExportSettings value)
        {
            try
            {
                return _viewService.GetViewsByWorkspaceAndArtifactType(value.WorkspaceId, value.ArtifactTypeId)
                    .FirstOrDefault(x => x.ArtifactId.Equals(value.ViewId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving view in {validator}", nameof(ViewExportValidator));
                string message =
                    IntegrationPointsExceptionMessages.CreateErrorMessageRetryOrContactAdministrator("while retrieving View");
                throw new IntegrationPointsException(message, ex);
            }
        }
    }
}
