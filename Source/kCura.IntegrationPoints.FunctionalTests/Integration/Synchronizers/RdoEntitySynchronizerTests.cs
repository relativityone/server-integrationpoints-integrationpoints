using System;
using System.Collections;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Synchronizers
{
    [TestExecutionCategory.CI, TestLevel.L1]
    public class RdoEntitySynchronizerTests : TestsBase
    {

        [IdentifiedTest("C78348C9-1504-4274-B277-AD62CFD883D9")]
        public void GetFields_ShouldPass()
        {
            // Arrange
            RdoEntitySynchronizer sut = PrepareSut((importJob) =>
            {
            });

            ImportSettings importSettings = new ImportSettings
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                CaseArtifactId = ArtifactProvider.NextId(),
                CopyFilesToDocumentRepository = true,
            };
            DataSourceProviderConfiguration configuration =
                new DataSourceProviderConfiguration(JsonConvert.SerializeObject(importSettings));

            //IEnumerable<FieldEntry> fields
            // Act
            IEnumerable<FieldEntry> fields = sut.GetFields(configuration);

            // Assert
        }

        private RdoEntitySynchronizer PrepareSut(Action<FakeJobImport> importAction)
        {
            Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(importAction)).LifestyleSingleton());

            return PrepareSut();
        }

        private RdoEntitySynchronizer PrepareSut()
        {
            return Container.Resolve<RdoEntitySynchronizer>();
        }
    }
}
