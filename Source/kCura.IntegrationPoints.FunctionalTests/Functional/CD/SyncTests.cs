using Relativity.Testing.Identification;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Atata;
using Relativity.Testing.Framework.Web.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Tests.Functional.CD
{
    [TestType.UI]
    [TestType.MainFlow]
    public class SyncTests : TestsBase
    {
        public SyncTests()
            : base(nameof(SyncTests))
        {
        }

        [Category("Regression")]
        [IdentifiedTest("1C37F97A-9DB3-4FED-BDAC-685864C03152")]
        [TestExecutionCategory.RAPCD.Verification.Functional]
        public void SyncLoadFirstPage()
        {
            // Arrange
            LoginAsStandardUser();

            // Act
            RelativityProviderConnectToSourcePage relativityProviderPage =
                Being.On<IntegrationPointListPage>(Workspace.ArtifactID)
                    .NewIntegrationPoint.ClickAndGo()
                    .ApplyModel(new
                    {
                        Name = nameof(SyncLoadFirstPage)
                    })
                    .RelativityProviderNext.ClickAndGo();

            // Assert
            relativityProviderPage.SavedSearch.IsEnabled
                .Value.Should().BeTrue();
        }
    }
}
