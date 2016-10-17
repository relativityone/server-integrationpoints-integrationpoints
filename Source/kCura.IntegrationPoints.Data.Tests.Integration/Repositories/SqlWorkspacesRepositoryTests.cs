using System;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
    [TestFixture]
    public class SqlWorkspacesRepositoryTests
    {
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.Pending, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.UpgradingScripts, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.PendingApplicationUpgrade, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.UpgradingApplications, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.Completed, ExpectedResult = 1)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.FailedScriptUpgrade, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.FailedApplicationUpgrade, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.Canceled, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.Disabled, ExpectedResult = 0)]
        [TestCase(WorkspaceUpgrade.WorkspaceUpgradeStatus.Failed, ExpectedResult = 0)]
        public int ItShouldRetrieveAllActive(WorkspaceUpgrade.WorkspaceUpgradeStatus status)
        {
            // arrange
            var workspaceArtifactId = 1000001;
            var workspaceName = "Workspace1";

            var dt = new DataTable();
            dt.Columns.Add("WorkspaceArtifactID");
            dt.Columns.Add("Name");
            dt.Columns.Add("Status");

            dt.Rows.Add(
                workspaceArtifactId.ToString(),
                workspaceName,
                ((int)status).ToString()
            );

            var dbContext = Substitute.For<kCura.Data.RowDataGateway.BaseContext>();
            dbContext.ExecuteSqlStatementAsDataTable(Arg.Any<string>(), Arg.Any<kCura.Data.ParameterList>(), Arg.Any<int>())
                .Returns(dt);

            var context = Substitute.For<BaseContext>();
            context.DBContext.Returns(dbContext);

            var repository = new SqlWorkspacesRepository(context);

            // act
            var actual = repository.RetrieveAllActive();

            // assert
            if (status == WorkspaceUpgrade.WorkspaceUpgradeStatus.Completed)
            {
                WorkspaceDTO workspace = actual.First();
                Assert.That(workspace.ArtifactId, Is.EqualTo(workspaceArtifactId));
                Assert.That(workspace.Name, Is.EqualTo(workspaceName));
            }

            return actual.Count();
        }
    }
}