using System;
using System.Linq;
using System.Threading;
using Atata;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Common.LDAP.TestData;
using Relativity.IntegrationPoints.Tests.Common.Extensions;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ImportLDAPTestImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly HumanResourcesTestData _expectedTestData;

        public ImportLDAPTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;

            _expectedTestData = new HumanResourcesTestData();
        }

        public void OnSetUpFixture()
        {
            _testsImplementationTestFixture.Workspace.InstallLegalHold();
        }

        public void ImportFromLDAPGoldFlow()
        {
            // Arrange
            _testsImplementationTestFixture.LoginAsStandardUser();

            string integrationPointName = nameof(ImportFromLDAPGoldFlow);

            // Act
            var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            var importFromLDAPConnectToSourcePage = FillOutIntegrationPointEditPageForImportFromLDAP(integrationPointEditPage, integrationPointName);

            var importFromLDAPMapFieldsPage = FillOutImportFromLDAPConnectToSourcePage(importFromLDAPConnectToSourcePage);

            var integrationPointViewPage = SetFieldsMappingImportFromLDAPMapFieldsPage(importFromLDAPMapFieldsPage);

            integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(integrationPointName);

            // Assert
            int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(integrationPointName);
            int workspaceEntityCount = RelativityFacade.Instance.Resolve<IEntityService>().GetAll(_testsImplementationTestFixture.Workspace.ArtifactID).Length;
            transferredItemsCount.Should().Be(workspaceEntityCount)
                .And.Be(_expectedTestData.EntryIds.Count());
        }

        private static ImportFromLDAPConnectToSourcePage FillOutIntegrationPointEditPageForImportFromLDAP(IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
        {
            integrationPointEditPage.Type.Set(IntegrationPointTypes.Import);

            Thread.Sleep(TimeSpan.FromSeconds(2));

            integrationPointEditPage.ApplyModel(new IntegrationPointEditImport
            {
                Name = integrationPointName,
                Source = IntegrationPointSources.LDAP,
                TransferredObject = IntegrationPointTransferredObjects.Entity,
            });

            return integrationPointEditPage.ImportFromLDAPNext.ClickAndGo();
        }

        private ImportFromLDAPMapFieldsPage FillOutImportFromLDAPConnectToSourcePage(ImportFromLDAPConnectToSourcePage importFromLDAPConnectToSourcePage)
        {
            return importFromLDAPConnectToSourcePage
               .ApplyModel(new ImportFromLDAPConnectToSource
               {
                   ConnectionPath = GlobalConst.LDAP._OPEN_LDAP_CONNECTION_PATH(_expectedTestData.OU),
                   Username = GlobalConst.LDAP._OPEN_LDAP_USER,
                   Password = GlobalConst.LDAP._OPEN_LDAP_PASSWORD,
               }).Next.ClickAndGo();
        }

        private static IntegrationPointViewPage SetFieldsMappingImportFromLDAPMapFieldsPage(ImportFromLDAPMapFieldsPage importFromLDAPMapFieldsPage)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));

            importFromLDAPMapFieldsPage.Cn.DoubleClick();
            importFromLDAPMapFieldsPage.GivenName.DoubleClick();
            importFromLDAPMapFieldsPage.Sn.DoubleClick();
            importFromLDAPMapFieldsPage.UniqueID.DoubleClick();
            importFromLDAPMapFieldsPage.FirstName.DoubleClick();
            importFromLDAPMapFieldsPage.LastName.DoubleClick();

            return importFromLDAPMapFieldsPage.Save.ClickAndGo();
        }
    }
}
