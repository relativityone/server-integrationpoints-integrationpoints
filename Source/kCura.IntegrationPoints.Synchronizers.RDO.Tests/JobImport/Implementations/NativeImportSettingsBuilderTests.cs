using System;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.JobImport.Implementations
{
    [TestFixture, Category("Unit")]
    public class NativeImportSettingsBuilderTests
    {
        [Test]
        public void PopulateFrom_ShouldThrowMeaningfulException_WhenWorkspaceDoesNotExistAmongAvailableWorkspaces()
        {
            // Arrange
            var importApiStub = new Mock<IImportAPI>();
            importApiStub
                .Setup(x => x.Workspaces())
                .Returns(Enumerable.Empty<Workspace>());

            var sut = new NativeImportSettingsBuilder(importApiStub.Object);

            const int caseArtifactId = 10020430;

            // Act
            Action action = () => sut.PopulateFrom(new ImportSettings(new DestinationConfiguration { CaseArtifactId = caseArtifactId }), Mock.Of<Settings>());

            // Assert
            action
                .ShouldThrowExactly<IntegrationPointsException>()
                .Where(e => e.Message == $"No workspace (id: {caseArtifactId}) found among available workspaces.");
        }
    }
}
