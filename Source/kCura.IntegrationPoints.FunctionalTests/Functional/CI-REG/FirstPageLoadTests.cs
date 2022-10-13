using Atata;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.CI;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.Testing.Framework.Web.Navigation;

namespace Relativity.IntegrationPoints.Tests.Functional.CI_REG
{
    public class FirstPageLoadTests : TestsBase
    {
        public FirstPageLoadTests()
        : base(nameof(FirstPageLoadTests))
        {
        }

        [Test]
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
