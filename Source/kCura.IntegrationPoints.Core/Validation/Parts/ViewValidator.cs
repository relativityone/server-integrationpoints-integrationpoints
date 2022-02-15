using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class ViewValidator : BasePartsValidator<int>
    {
        private readonly IAPILog _logger;
        private readonly IRelativityObjectManager _objectManager;

        public ViewValidator(IRelativityObjectManager objectManager, IAPILog logger)
        {
            _objectManager = objectManager;
            _logger = logger;
        }

        public override ValidationResult Validate(int viewId)
        {
            var result = new ValidationResult();

            RelativityObject view = RetrieveView(viewId);

            if (view == null)
            {
                result.Add(ValidationMessages.ViewNoAccess);
            }

            return result;
        }

        private RelativityObject RetrieveView(int viewId)
        {
            try
            {
                QueryRequest queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.View
                    },
                    Condition = $"'Artifact ID' == {viewId}"
                };

                List<RelativityObject> result = _objectManager.Query(queryRequest);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred retrieving view in {validator}", nameof(SavedSearchValidator));
                string message = IntegrationPointsExceptionMessages.CreateErrorMessageRetryOrContactAdministrator("retrieving view");
                throw new IntegrationPointsException(message, ex);
            }
        }
    }
}
