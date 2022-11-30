using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal sealed class DestinationWorkspaceObjectTypesCreationExecutorTests : SystemTest
    {
        private WorkspaceRef _destinationWorkspace;
        private WorkspaceRef _sourceWorkspace;
        private readonly List<Guid> _guids = new List<Guid>()
        {
            new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC"),
            new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7"),
            new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5"),
            new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0"),
            new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19"),
            new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f"),
            new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231"),
            new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169"),
            new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f")
        };

        [SetUp]
        public async Task SetUp()
        {
            _sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
        }

        [IdentifiedTest("b393cc92-f5b9-4e5c-8e3e-b8e48507746b")]
        public async Task ItShouldCreateObjectTypesAndFields()
        {
            // Verify that object types and fields are not existing in a workspace before we run the test
            await VerifyObjectTypesAndFieldsExistAsync(false).ConfigureAwait(false);

            ConfigurationStub configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
                DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID
            };

            configuration.SyncConfigurationArtifactId = Rdos.CreateSyncConfigurationRdoAsync(configuration.SourceWorkspaceArtifactId,
                configuration, TestLogHelper.GetLogger()).GetAwaiter().GetResult();

            // act
            ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IDestinationWorkspaceObjectTypesCreationConfiguration>(configuration);
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            await VerifyObjectTypesAndFieldsExistAsync(true).ConfigureAwait(false);
        }

        private async Task VerifyObjectTypesAndFieldsExistAsync(bool expectExisting)
        {
            using (IArtifactGuidManager guidManager = ServiceFactory.CreateProxy<IArtifactGuidManager>())
            {
                foreach (Guid guid in _guids)
                {
                    bool guidExists = await guidManager.GuidExistsAsync(_destinationWorkspace.ArtifactID, guid).ConfigureAwait(false);
                    if (expectExisting)
                    {
                        Assert.IsTrue(guidExists);
                    }
                    else
                    {
                        Assert.IsFalse(guidExists);
                    }
                }
            }
        }
    }
}
