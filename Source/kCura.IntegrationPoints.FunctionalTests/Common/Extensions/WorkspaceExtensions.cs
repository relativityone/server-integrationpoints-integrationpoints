using System;
using System.Collections.Generic;
using System.Linq;
using Polly;
using Polly.Retry;
using Relativity.IntegrationPoints.Tests.Functional;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Logging;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Common.Extensions
{
    public static class WorkspaceExtensions
    {
        public static void InstallLegalHold(this Workspace workspace)
        {
            using (IObjectTypeManager service = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectTypeManager>())
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                ILogService logger = RelativityFacade.Instance.Log;
                const int maxRetriesCount = 3;
                const int betweenRetriesBase = 2;
                const int maxJitterMs = 100;

                RetryPolicy retryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetry(
                        maxRetriesCount,
                        retryAttempt =>
                {
                    TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(betweenRetriesBase, retryAttempt));
                    TimeSpan jitter = TimeSpan.FromMilliseconds(new Random().Next(0, maxJitterMs));
                    return delay + jitter;
                }, (Exception exception, TimeSpan waitTime, int retryCount, Context context) =>
                {
                    ILibraryApplicationService applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();
                    int applicationId = applicationService.Get(Const.Application.LEGAL_HOLD_APPLICATION_NAME).ArtifactID;
                    bool isLegalHoldInstalled = applicationService.IsInstalledInWorkspace(workspace.ArtifactID, applicationId);

                    string warningMessage = string.Format(
                        "Attempting to retry Legal Hold Installation, isLegalHoldInstalled - {0}, retryCount - {1}",
                        isLegalHoldInstalled,
                        retryCount);
                    logger.Warn(warningMessage);
                    logger.Error(exception);
                });

                retryPolicy.Execute(() =>
                {
                    InstallApplication(workspace, Const.Application.LEGAL_HOLD_APPLICATION_NAME);
                    string artifactTypeName = "Entity";

                    List<ObjectTypeIdentifier> artifactTypes = service.GetAvailableParentObjectTypesAsync(workspace.ArtifactID).GetAwaiter().GetResult();
                    ObjectTypeIdentifier artifactType = artifactTypes.FirstOrDefault(x => x.Name == artifactTypeName);

                    if (artifactType == null)
                    {
                        throw new Exception($"Can't find Artifact Type: {artifactTypeName}");
                    }

                    QueryRequest request = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                        Condition = $"'FieldArtifactTypeID' == {artifactType.ArtifactTypeID}",
                        IncludeNameInQueryResult = true,
                        RankSortOrder = SortEnum.Ascending
                    };

                    QueryResult queryResult = objectManager.QueryAsync(workspace.ArtifactID, request, 0, int.MaxValue)
                        .GetAwaiter()
                        .GetResult();

                    if (!queryResult.Objects.Any())
                    {
                        throw new NotFoundException($"Entity Fields not found for workspace - {workspace.Name}");
                    }
                });
            }
        }

        private static void InstallApplication(this Workspace workspace, string applicationName)
        {
            ILibraryApplicationService applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();
            applicationService.InstallToWorkspace(workspace.ArtifactID, applicationService.Get(applicationName).ArtifactID);
        }
    }
}
