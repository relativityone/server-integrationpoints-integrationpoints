using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
    [TestFixture, Category("Unit")]
    public class ExportJobErrorServiceTest : TestBase
    {
        private IScratchTableRepository _scratchTable;
        private IRepositoryFactory _repositoryFactory;
        private IInstanceSettingRepository _instanceSettingRepository;
        private List<string> _documentIds;
        private ExportJobErrorService _instance;
        private DateTime _dummyDate = new DateTime(2016, 4, 1);
        private string _sectionName = IntegrationPoints.Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION;
        private string _settingName = IntegrationPoints.Domain.Constants.REMOVE_ERROR_BATCH_SIZE_INSTANCE_SETTING_NAME;

        [SetUp]
        public override void SetUp()
        {
            _scratchTable = Substitute.For<IScratchTableRepository>();
            _instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.GetInstanceSettingRepository().Returns(_instanceSettingRepository);
            _documentIds = new List<string>();

            IScratchTableRepository[] tables = {_scratchTable};

            _instance = new ExportJobErrorService(tables, _repositoryFactory, _documentIds);
        }

        [Test]
        public void OneError_SmallerThanBatchSize_AutoBatchSize1000()
        {
            //Arrange
            _instanceSettingRepository.GetConfigurationValue(_sectionName, _settingName).Returns(String.Empty);

            //Act
            _instance.OnRowError("docIdentifier", "Message");
            _instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done

            //Assert
            _scratchTable.Received(1).RemoveErrorDocuments(Arg.Is(_documentIds));
            _instanceSettingRepository.Received(1)
                .GetConfigurationValue(_sectionName, _settingName);
        }

        [Test]
        public void MultipleErrors_SmallerThanBatchSize_SetBatchSize3000() //do this
        {
            //Arrange
            _instanceSettingRepository.GetConfigurationValue(_sectionName, _settingName).Returns("3000");

            //Act
            for (int numErrors = 0; numErrors < 2999; numErrors++)
            {
                _instance.OnRowError("docIdentifier", "Message");
            }

            _instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done

            //Assert
            _scratchTable.Received(1).RemoveErrorDocuments(Arg.Is(_documentIds));
            _instanceSettingRepository.Received(1)
                .GetConfigurationValue(_sectionName, _settingName);
        }

        [Test]
        public void MultipleErrors_EqualToBatchSize_SetBatchSize5000()
        {
            //Arrange
            _instanceSettingRepository.GetConfigurationValue(_sectionName, _settingName).Returns("5000");

            //Act
            for (int numErrors = 0; numErrors < 5000; numErrors++)
            {
                _instance.OnRowError("docIdentifier", "Message");
            }

            _instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done, should not trigger a flush

            //Assert
            _scratchTable.Received(1).RemoveErrorDocuments(Arg.Is(_documentIds));
            _instanceSettingRepository.Received(1)
                .GetConfigurationValue(_sectionName, _settingName);
        }

        [Test]
        public void MultipleErrors_EqualToBatchSize_ManuallySetMissingBatchSize()
        {
            //Arrange
            _instanceSettingRepository.GetConfigurationValue(_sectionName, _settingName).Returns(String.Empty);

            //Act
            for (int numErrors = 0; numErrors < 1000; numErrors++)
            {
                _instance.OnRowError("docIdentifier", "Message");
            }

            _instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done, should not trigger a flush

            //Assert
            _scratchTable.Received(1).RemoveErrorDocuments(Arg.Is(_documentIds));
            _instanceSettingRepository.Received(1)
                .GetConfigurationValue(_sectionName, _settingName);
        }

        [Test]
        public void MultipleErrors_GreaterThanBatchSize_AutoBatchSize()
        {
            //Arrange
            _instanceSettingRepository.GetConfigurationValue(_sectionName, _settingName).Returns("1000");

            //Act
            for (int numErrors = 0; numErrors < 1001; numErrors++)
            {
                _instance.OnRowError("docIdentifier", "Message");
            }

            _instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done, errors leftover so a flush is triggered

            //Assert
            _scratchTable.Received(2).RemoveErrorDocuments(Arg.Is(_documentIds));
            _instanceSettingRepository.Received(1)
                .GetConfigurationValue(_sectionName, _settingName);
        }
    }
}
