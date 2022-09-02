using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.Entity;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Services.Synchronizer
{
    [TestFixture, Category("Unit")]
    public class ImportProviderRdoSynchronizerFactoryTests
    {
        private ImportProviderRdoSynchronizerFactory _sut;
        private Mock<IKernel> _kernel;
        private Mock<IObjectTypeRepository> _objectTypeRepository;
        

        private const int _ARTIFACT_TYPE_ID = 3242;
        private readonly string _rdoEntitySynchronizerAssemblyName = typeof(RdoEntitySynchronizer).AssemblyQualifiedName;
        private readonly string _rdoSynchronizerAssemblyName = typeof(RdoSynchronizer).AssemblyQualifiedName;

        [SetUp]
        public void Setup()
        {
            _kernel = new Mock<IKernel>();

            Mock<IWindsorContainer> container = new Mock<IWindsorContainer>();
            container.Setup(x => x.Kernel).Returns(_kernel.Object);

            _objectTypeRepository = new Mock<IObjectTypeRepository>();
            _sut = new ImportProviderRdoSynchronizerFactory(container.Object, _objectTypeRepository.Object);
        }

        [TestCase("Entity")]
        [TestCase("Custodian")]
        [TestCase("OtherName")]
        public void ItShouldCreateCustomSynchronizer_ForEntityObjectType_RegardlessOfItsName(string entityObjectTypeName)
        {
            // arrange
            SwitchObjectTypeToEntity(entityObjectTypeName);
            SwitchResolvedSynchronizerToEntitySynchronizer();

            // act
            _sut.CreateSynchronizer(ImportSettings, null, null);

            // assert
            _kernel.Verify(x => x.Resolve<IDataSynchronizer>(_rdoEntitySynchronizerAssemblyName));
        }

        [Test]
        public void ItShouldSetJobSubmitter_ForEntityObjectType()
        {
            // arrange
            SwitchObjectTypeToEntity();
            RdoEntitySynchronizer synchronizer = SwitchResolvedSynchronizerToEntitySynchronizer();
            Mock<ITaskJobSubmitter> taskSubmitter = new Mock<ITaskJobSubmitter>();

            // act
            _sut.CreateSynchronizer(ImportSettings, taskSubmitter.Object, null);

            // assert
            Assert.AreEqual(taskSubmitter.Object, synchronizer.TaskJobSubmitter);
        }

        [TestCase("Entity")]
        [TestCase("Custodian")]
        [TestCase("OtherName")]
        public void ItShouldCreateNormalSynchronizer_ForNonEntityObjectType_RegardlessOfItsName(string objectTypeName)
        {
            // arrange
            SwitchObjectTypeToNonEntity(objectTypeName);

            // act
            _sut.CreateSynchronizer(ImportSettings, null, null);

            // assert
            _kernel.Verify(x => x.Resolve<IDataSynchronizer>(_rdoSynchronizerAssemblyName));
        }

        private RdoEntitySynchronizer SwitchResolvedSynchronizerToEntitySynchronizer()
        {
            var logger = new Mock<IAPILog>();
            var logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(logger.Object);
            var helper = new Mock<IHelper>();
            helper.Setup(x => x.GetLoggerFactory()).Returns(logFactory.Object);
            Mock<IInstanceSettingsManager> instanceSettingsManager = new Mock<IInstanceSettingsManager>();
            instanceSettingsManager.Setup(x => x.GetIApiBatchSize()).Returns(1000);
            RdoEntitySynchronizer dataSynchronizer = new RdoEntitySynchronizer(null, null, null, helper.Object, null, null, instanceSettingsManager.Object);
            _kernel.Setup(x => x.Resolve<IDataSynchronizer>(_rdoEntitySynchronizerAssemblyName)).Returns(dataSynchronizer);

            return dataSynchronizer;
        }

        private static ImportSettings ImportSettings => new ImportSettings
        {
            ArtifactTypeId = _ARTIFACT_TYPE_ID
        };

        private void SwitchObjectTypeToEntity(string objectTypeName = "Entity")
        {
            Guid entityTypeGuid = ObjectTypeGuids.Entity;
            SwitchObjectType(objectTypeName, entityTypeGuid);
        }

        private void SwitchObjectTypeToNonEntity(string objectTypeName)
        {
            SwitchObjectType(objectTypeName, Guid.NewGuid());
        }

        private void SwitchObjectType(string objectTypeName, Guid objectTypeGuid)
        {
            var objectType = new ObjectTypeDTO
            {
                Name = objectTypeName,
                Guids = new List<Guid> { objectTypeGuid }
            };

            _objectTypeRepository.Setup(x => x.GetObjectType(_ARTIFACT_TYPE_ID)).Returns(objectType);
        }
    }
}
