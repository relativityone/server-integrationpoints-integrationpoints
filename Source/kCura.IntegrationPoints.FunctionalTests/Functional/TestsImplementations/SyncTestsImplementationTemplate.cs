using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Common.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal abstract class SyncTestsImplementationTemplate
    {
        protected readonly ITestsImplementationTestFixture TestsImplementationTestFixture;
        protected readonly Dictionary<string, Workspace> DestinationWorkspaces = new Dictionary<string, Workspace>();

        protected string IntegrationPointName { get; set; }
        protected Workspace DestinationWorkspace { get; set; }

        protected SyncTestsImplementationTemplate(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            TestsImplementationTestFixture = testsImplementationTestFixture;
        }

        public virtual void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(TestsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile());
        }

        public virtual void OnTearDownFixture()
        {
            foreach (KeyValuePair<string, Workspace> destinationWorkspace in DestinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace.Value);
            }
        }

        public abstract IntegrationPointViewPage CreateIntegrationPointViewPage();

        public abstract void RunIntegrationPoint(IntegrationPointViewPage integrationPointViewPage);

        public abstract void AssertIntegrationPointSummaryPageGeneralTab(IntegrationPointViewPage integrationPointViewPage);

        public abstract void AssertIntegrationPointJobHistory(IntegrationPointViewPage integrationPointViewPage);

        protected Workspace CreateDestinationWorkspace()
        {
            string workspaceName = $"Sync - Dest {Guid.NewGuid()}";

            Workspace workspace = RelativityFacade.Instance.CreateWorkspace(workspaceName, TestsImplementationTestFixture.Workspace.Name);

            DestinationWorkspaces.Add(workspaceName, workspace);

            workspace.InstallLegalHold();

            return workspace;
        }

        protected int GetCorrectlyTaggedDocumentsCount(List<RelativityObject> documents, string taggedField, string tagValue)
        {
            return documents.Where(x => FieldTagMatchesExpectedValue(x, taggedField, tagValue)).Count();
        }

        protected int GetJobId(int workspaceId, string jobName)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryGuid },
                    Fields = new FieldRef[] { new FieldRef { Name = "Job ID" } },
                    Condition = $"(('Name' LIKE '{jobName}'))"
                };

                List<RelativityObject> result = objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue)
                    .GetAwaiter().GetResult().Objects.ToList();

                return result.FirstOrDefault().ArtifactID;
            }
        }

        protected List<RelativityObject> GetDocumentsTagsDataFromSourceWorkspace(int workspaceId)
        {
            FieldRef[] fields = new FieldRef[] { new FieldRef { Name = "Relativity Destination Case" } };
            return GetDocumentsWithSelectedFields(workspaceId, fields);
        }

        protected List<RelativityObject> GetDocumentsTagsDataFromDestinationWorkspace(int workspaceId)
        {
            FieldRef[] fields = new FieldRef[] { new FieldRef { Name = "Relativity Source Case" },
                new FieldRef { Name = "Relativity Source Job" } };

            return GetDocumentsWithSelectedFields(workspaceId, fields);
        }

        private bool FieldTagMatchesExpectedValue(RelativityObject doc, string fieldName, string expectedTagValue)
        {
            object fieldValue = doc.FieldValues.Where(f => f.Field.Name == fieldName).FirstOrDefault().Value;
            return fieldValue == null ? false : ((IList<RelativityObjectValue>)fieldValue).Where(x => x.Name == expectedTagValue).Any();
        }

        private List<RelativityObject> GetDocumentsWithSelectedFields(int workspaceId, FieldRef[] fields)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                    Fields = fields
                };

                return objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue)
                    .GetAwaiter().GetResult().Objects.ToList();
            }
        }
    }
}
