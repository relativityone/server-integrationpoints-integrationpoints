using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Models.Validation;
using Microsoft.Owin.Security.Provider;
using Relativity.API;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class JobController : ApiController
    {
        private const string _RELATIVITY_USERID = "rel_uai";
        private const string _RUN_AUDIT_MESSAGE = "Transfer was attempted.";
        private const string _RETRY_AUDIT_MESSAGE = "Retry error was attempted.";
        private const string _STOP_AUDIT_MESSAGE = "Stop transfer was attempted.";
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IManagerFactory _managerFactory;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IAPILog _log;

        public JobController(IRepositoryFactory repositoryFactory, IManagerFactory managerFactory, IIntegrationPointService integrationPointService, IAPILog log)
        {
            _repositoryFactory = repositoryFactory;
            _managerFactory = managerFactory;
            _integrationPointService = integrationPointService;
            _log = log;
        }

        // POST API/Job/Run
        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to run the transfer job.")]
        public HttpResponseMessage Run(Payload payload)
        {
            try
            {
                _log.LogInformation("'Run' button clicked for Integration Point id: {id}", payload.ArtifactId);

                AuditAction(payload, _RUN_AUDIT_MESSAGE);

                IntegrationPointSlimDto integrationPoint = _integrationPointService
                    .ReadSlim(Convert.ToInt32(payload.ArtifactId));

                // this validation was introduced due to an issue with ARMed workspaces (REL-171985)
                // so far, ARM is not capable of copying SQL Secret Catalog records for integration points in workspace database
                // if secret store entry is missing, SecuredConfiguration property contains bare guid instead of JSON - that's why we check if it can be parsed as guid
                Guid parseResult;
                if (Guid.TryParse(integrationPoint.SecuredConfiguration, out parseResult))
                {
                    throw new IntegrationPointsException("Integration point secret store configuration is missing. " +
                                                         "This may be caused by RIP being restored from ARM backup. " +
                                                         "Please try to edit integration point configuration and reenter credentials.");
                }

                HttpResponseMessage httpResponseMessage = RunInternal(payload.AppId, payload.ArtifactId, ActionType.Run);

                return httpResponseMessage;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Run job.");
                throw;
            }
        }

        // POST API/Job/Retry
        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retry run of the transfer job.")]
        public HttpResponseMessage Retry(Payload payload, bool switchToAppendOverlayMode = false)
        {
            _log.LogInformation("'Retry' button clicked for Integration Point id: {id}", payload.ArtifactId);

            AuditAction(payload, _RETRY_AUDIT_MESSAGE);

            HttpResponseMessage httpResponseMessage = RunInternal(payload.AppId, payload.ArtifactId, ActionType.Retry, switchToAppendOverlayMode);

            return httpResponseMessage;
        }

        // POST API/Job/Stop
        [HttpPost]
        public HttpResponseMessage Stop(Payload payload)
        {
            AuditAction(payload, _STOP_AUDIT_MESSAGE);

            JobActionResult result = new JobActionResult();
            try
            {
                _integrationPointService.MarkIntegrationPointToStopJobs(payload.AppId, payload.ArtifactId);
            }
            catch (AggregateException exception)
            {
                // TODO: Add an extension to aggregate messages without stack traces. Place it in ExceptionExtensions.cs
                IEnumerable<string> innerExceptions = exception.InnerExceptions.Where(ex => ex != null).Select(ex => ex.Message);
                string aggregatedErrorMessage = $"{exception.Message} : {string.Join(",", innerExceptions)}";
                CreateRelativityError(aggregatedErrorMessage, exception.FlattenErrorMessagesWithStackTrace(), payload.AppId);

                result.Errors.AddRange(innerExceptions);
            }
            catch (IntegrationPointValidationException exception)
            {
                ValidationResultDTO validationResult = CreateResponseForFailedValidation(exception);
                result.Errors.AddRange(validationResult.Errors.Select(x => x.Message));
            }
            catch (Exception exception)
            {
                CreateRelativityError(exception.Message, exception.FlattenErrorMessagesWithStackTrace(), payload.AppId);

                result.Errors.Add(exception.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        private HttpResponseMessage RunInternal(int workspaceId, int relatedObjectArtifactId, ActionType action, bool switchToAppendOverlayMode = false)
        {
            JobActionResult result = new JobActionResult();
            try
            {
                ValidateRDOPermission(workspaceId);

                int userId = GetUserIdIfExists();
                if (action == ActionType.Run)
                {
                    _integrationPointService.RunIntegrationPoint(workspaceId, relatedObjectArtifactId, userId);
                }
                else
                {
                    _integrationPointService.RetryIntegrationPoint(workspaceId, relatedObjectArtifactId, userId, switchToAppendOverlayMode);
                }
            }
            catch (AggregateException exception)
            {
                IEnumerable<string> innerExceptions = exception.InnerExceptions.Where(ex => ex != null).Select(ex => ex.Message);

                result.Errors.Add(exception.Message);
                result.Errors.AddRange(innerExceptions);
            }
            catch (IntegrationPointValidationException exception)
            {
                ValidationResultDTO validationResult = CreateResponseForFailedValidation(exception);
                result.Errors.AddRange(validationResult.Errors.Select(x => x.Message));
            }
            catch (Exception exception)
            {
                _log.LogError(exception, "Error occurred in Run request: WorkspaceId {workspaceId}, IntegrationPointId {integrationPointId}, Action: {action}",
                    workspaceId, relatedObjectArtifactId, action);

                result.Errors.Add(exception.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        private ValidationResultDTO CreateResponseForFailedValidation(IntegrationPointValidationException exception)
        {
            var validationResultMapper = new ValidationResultMapper();
            return validationResultMapper.Map(exception.ValidationResult);
        }

        private void AuditAction(Payload payload, string auditMessage)
        {
            IAuditManager auditManager = _managerFactory.CreateAuditManager(payload.AppId);
            AuditElement audit = new AuditElement { AuditMessage = auditMessage };
            auditManager.RelativityAuditRepository.CreateAuditRecord(payload.ArtifactId, audit);
        }

        private int GetUserIdIfExists()
        {
            var user = User as ClaimsPrincipal;
            if (user != null)
            {
                foreach (Claim claim in user.Claims)
                {
                    if (_RELATIVITY_USERID.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Convert.ToInt32(claim.Value);
                    }
                }
            }
            return 0;
        }

        private void CreateRelativityError(string message, string fullText, int workspaceArtifactId)
        {
            IErrorManager errorManager = _managerFactory.CreateErrorManager();

            ErrorDTO error = new ErrorDTO()
            {
                Message = message,
                FullText = fullText,
                Source = APPLICATION_NAME,
                WorkspaceId = workspaceArtifactId
            };

            errorManager.Create(new[] { error });
        }

        public class Payload
        {
            public int AppId { get; set; }

            public int ArtifactId { get; set; }
        }

        private enum ActionType
        {
            Run,
            Retry
        }

        private void ValidateRDOPermission(int workspaceId)
        {
            ValidationResult validationResult = new ValidationResult();

            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceId);

            if (!permissionRepository.UserHasArtifactTypePermissions(ObjectTypeGuids.IntegrationPointGuid, new[] { ArtifactPermission.View, ArtifactPermission.Edit }))
            {
                validationResult.Add(PermissionErrors.INTEGRATION_POINT_RUN_RDO_PERMISSION);
            }

            if (!permissionRepository.UserHasArtifactTypePermissions(ObjectTypeGuids.JobHistoryGuid, new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create }))
            {
                validationResult.Add(PermissionErrors.JOB_HISTORY_RUN_RDO_PERMISSION);
            }

            if (!permissionRepository.UserHasArtifactTypePermissions(ObjectTypeGuids.JobHistoryErrorGuid, new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create }))
            {
                validationResult.Add(PermissionErrors.JOB_HISTORY_ERROR_RUN_RDO_PERMISSION);
            }

            if (!permissionRepository.UserHasArtifactTypePermission(ObjectTypeGuids.IntegrationPointTypeGuid, ArtifactPermission.View))
            {
                validationResult.Add(PermissionErrors.INTEGRATION_POINT_TYPE_RUN_RDO_PERMISSION);
            }

            if (!permissionRepository.UserHasArtifactTypePermission(ObjectTypeGuids.SourceProviderGuid, ArtifactPermission.View))
            {
                validationResult.Add(PermissionErrors.SOURCE_PROVIDER_RUN_RDO_PERMISSION);
            }

            if (!permissionRepository.UserHasArtifactTypePermission(ObjectTypeGuids.DestinationProviderGuid, ArtifactPermission.View))
            {
                validationResult.Add(PermissionErrors.DESTINATION_PROVIDER_RUN_RDO_PERMISSION);
            }

            if (!validationResult.IsValid)
            {
                throw new IntegrationPointValidationException(validationResult);
            }
        }
    }
}
