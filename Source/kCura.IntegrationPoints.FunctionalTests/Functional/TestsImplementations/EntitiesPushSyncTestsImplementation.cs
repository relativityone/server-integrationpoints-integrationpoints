﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Navigation;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class EntitiesPushSyncTestsImplementation : SyncTestsImplementationTemplate
    {
        private int _entitiesCount = 10;
        private string _viewName;

        public EntitiesPushSyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : base(testsImplementationTestFixture)
        {
        }

        public override IntegrationPointViewPage CreateIntegrationPointViewPage()
        {
            PrepareEntities(_entitiesCount).GetAwaiter().GetResult();

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(TestsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            _viewName = "Entities - Legal Hold View";
            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage
                .CreateSyncRdoIntegrationPoint(IntegrationPointName, DestinationWorkspace, IntegrationPointTransferredObjects.Entity, _viewName);

            return integrationPointViewPage;
        }

        public override void AssertIntegrationPointSummaryPageGeneralTab(IntegrationPointViewPage integrationPointViewPage)
        {
	        string sInstanceName = RelativityFacade.Instance.Resolve<IInstanceSettingsService>()
		        .Get("FriendlyInstanceName", "Relativity.Authentication").Value;

	        integrationPointViewPage.SummaryPageGeneralTab.Name.ExpectTo.BeVisibleWithRetries(3);
            integrationPointViewPage.SummaryPageGeneralTab.Overwrite.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.ExportType.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.SourceDetails.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.SourceWorkspace.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.SourceRelativityInstance.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.TransferedObject.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.DestinationWorkspace.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.MultiSelectOverlay.ExpectTo.BeVisibleWithRetries();

            integrationPointViewPage.GetName().ShouldBeEquivalentTo(IntegrationPointName);
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; View");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"View: {_viewName}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo($"This instance({sInstanceName})");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Entity);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(DestinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);

            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisibleWithRetries();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();
        }

        public override void AssertIntegrationPointJobHistory(IntegrationPointViewPage integrationPointViewPage)
        {
            string jobStatus = integrationPointViewPage.GetJobStatus(IntegrationPointName);
            int totalItemsCount = integrationPointViewPage.GetTotalItemsCount(IntegrationPointName);
            int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(IntegrationPointName);

            jobStatus.Should().Be("Completed");
            totalItemsCount.Should().Be(_entitiesCount);
            transferredItemsCount.Should().Be(_entitiesCount);
        }

        private async Task PrepareEntities(int count)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                int entityArtifactTypeId = await GetArtifactTypeIdAsync(TestsImplementationTestFixture.Workspace.ArtifactID, "Entity").ConfigureAwait(false);

                ObjectTypeRef entityObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = entityArtifactTypeId
                };

                FieldRef[] fields = new[]
                {
                    new FieldRef()
                    {
                        Name = "Full Name"
                    },
                    new FieldRef()
                    {
                        Name = "Email"
                    }
                };

                IReadOnlyList<IReadOnlyList<object>> values = Enumerable
                    .Range(1, count)
                    .Select(i => new List<object>()
                {
                    $"Employee {i}",
                    $"employee-{i}@company.com"
                })
                .ToList();

                MassCreateResult massCreateResult = await objectManager.CreateAsync(
                TestsImplementationTestFixture.Workspace.ArtifactID,
                new MassCreateRequest
                {
                    ObjectType = entityObjectType,
                    Fields = fields,
                    ValueLists = values
                },
                CancellationToken.None).ConfigureAwait(false);

                if (!massCreateResult.Success)
                {
                    throw new Exception($"Mass creation of Entities failed: {massCreateResult.Message}");
                }
            }
        }

        private async Task<int> GetArtifactTypeIdAsync(int workspaceId, string artifactTypeName)
        {
            using (var service = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectTypeManager>())
            {
                List<ObjectTypeIdentifier> artifactTypes = await service.GetAvailableParentObjectTypesAsync(workspaceId).ConfigureAwait(false);
                ObjectTypeIdentifier artifactType = artifactTypes.FirstOrDefault(x => x.Name == artifactTypeName);

                if (artifactType == null)
                {
                    throw new Exception($"Can't find Artifact Type: {artifactTypeName}");
                }

                return artifactType.ArtifactTypeID;
            }
        }
    }
}
