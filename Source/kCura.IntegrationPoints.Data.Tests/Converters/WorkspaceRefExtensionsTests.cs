using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Data.Tests.Converters
{
    [TestFixture, Category("Unit")]
    public class WorkspaceRefExtensionsTests
    {
        [Test]
        public void ToWorkspaceDTO_ShouldReturnNullForNullInput()
        {
            // arrange
            WorkspaceRef input = null;

            // act
            WorkspaceDTO result = input.ToWorkspaceDTO();

            // assert
            result.Should().BeNull("because input was null");
        }

        [Test]
        public void ToWorkspaceDTO_ShouldConvertValidObject()
        {
            // arrange
            const int artifactID = 2323124;
            const string name = "workspace name";

            var input = new WorkspaceRef(artifactID)
            {
                Name = name
            };

            // act
            WorkspaceDTO result = input.ToWorkspaceDTO();

            // assert
            result.ArtifactId.Should().Be(artifactID);
            result.Name.Should().Be(name);
        }

        [Test]
        public void ToWorkspaceDTOs_ShouldReturnNullForNullInput()
        {
            // arrange
            IEnumerable<WorkspaceRef> inputs = null;

            // act
            IEnumerable<WorkspaceDTO> results = inputs.ToWorkspaceDTOs();

            // assert
            results.Should().BeNull("because input was null");
        }

        [Test]
        public void ToWorkspaceDTOs_ShouldWorksForEmpytList()
        {
            // arrange
            IEnumerable<WorkspaceRef> inputs = Enumerable.Empty<WorkspaceRef>();

            // act
            IEnumerable<WorkspaceDTO> results = inputs.ToWorkspaceDTOs();

            // assert
            results.Should().BeEmpty("because input collection was empty");
        }

        [Test]
        public void ToWorkspaceDTOs_ShouldConvertValidObjects()
        {
            // arrange
            WorkspaceRef[] inputs =
            {
                new WorkspaceRef(421412)
                {
                    Name = "first workspace"
                },
                new WorkspaceRef(94314)
                {
                    Name = "second workspace"
                }
            };

            // act
            WorkspaceDTO[] results = inputs.ToWorkspaceDTOs().ToArray();

            // assert
            results.Length.Should().Be(inputs.Length);
            for (int i = 0; i < inputs.Length; i++)
            {
                results[i].ArtifactId.Should().Be(inputs[i].ArtifactID);
                results[i].Name.Should().Be(inputs[i].Name);
            }
        }
    }
}
