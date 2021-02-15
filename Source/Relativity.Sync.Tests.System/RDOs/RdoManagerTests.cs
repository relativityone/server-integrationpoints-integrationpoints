using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.RDOs
{
    internal class RdoManagerTests : SystemTest
    {
        private RdoManager _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new RdoManager(Logger, new ServicesManagerStub(), new RdoGuidProvider());
        }

        [IdentifiedTest("5CDA93A5-F9D2-4C8A-B4CC-EFB832D8746D")]
        public async Task EnsureTypeExist_ShouldCreateType_WhenTypeDoesNotExist()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(workspace.ArtifactID);

            // Assert
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var typeQueryResult = await objectManager.QueryAsync(workspace.ArtifactID, new QueryRequest
                {
                    Condition = $"'Name' == '{SampleRdo.ExpectedRdoInfo.Name}'",
                    Fields = new[]
                    {
                        new FieldRef {Name = "Artifact Type ID"}
                    },
                    ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.ObjectType}
                }, 0, 1).ConfigureAwait(false);

                var existingFieldsQueryResult = await objectManager.QueryAsync(workspace.ArtifactID, new QueryRequest()
                {
                    Condition = $"'FieldArtifactTypeID' == {typeQueryResult.Objects.First().FieldValues.First().Value}",
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int) ArtifactType.Field
                    }
                }, 0, int.MaxValue).ConfigureAwait(false);

                foreach (var fieldInfo in SampleRdo.ExpectedRdoInfo.Fields.Values)
                {
                    existingFieldsQueryResult.Objects.Any(x => x.Guids.Contains(fieldInfo.Guid)).Should().BeTrue();
                }
            }
        }

        [IdentifiedTest("2180C723-8911-4645-AB95-D92883037939")]
        public async Task EnsureTypeExists_ShouldCreateMissingFields()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            await _sut.EnsureTypeExistsAsync<SampleRdo>(workspace.ArtifactID).ConfigureAwait(false);
            
            // Act
            await _sut.EnsureTypeExistsAsync<ExtendedSampleRdo>(workspace.ArtifactID).ConfigureAwait(false);
            
            // Assert
            using (var guidManager = ServiceFactory.CreateProxy<IArtifactGuidManager>())
            {
                var newFieldExists = await guidManager
                    .GuidExistsAsync(workspace.ArtifactID, new Guid("E44D02A2-9BD1-4BB1-A5D9-281F25666359"))
                    .ConfigureAwait(false);

                newFieldExists.Should().BeTrue();
            }
        }
    }
}