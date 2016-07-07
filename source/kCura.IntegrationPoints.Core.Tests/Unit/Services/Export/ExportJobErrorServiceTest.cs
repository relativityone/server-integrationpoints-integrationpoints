using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;


namespace kCura.IntegrationPoints.Core.Tests.Unit.Services.Export
{
	[TestFixture]
	public class ExportJobErrorServiceTest
	{
		private IScratchTableRepository _scratchTable;
		private IRepositoryFactory _repositoryFactory;
		private IInstanceSettingRepository _instanceSettingRepository;
		private ExportJobErrorService _instance;
		private DateTime _dummyDate = new DateTime(2016, 4, 1);

		[SetUp]
		public void Setup()
		{
			_scratchTable = Substitute.For<IScratchTableRepository>();
			_instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_repositoryFactory.GetInstanceSettingRepository().Returns(_instanceSettingRepository);

			IScratchTableRepository[] tables = {_scratchTable};

			_instance = new ExportJobErrorService(tables, _repositoryFactory);
		}

		[Test]
		public void OneError_SmallerThanBatchSize_AutoBatchSize1000()
		{
			//Act
			_instance.OnRowError("docIdentifier", "Message");
			_instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done

			//Assert
			_scratchTable.Received(1).RemoveErrorDocuments(Arg.Any<ICollection<string>>());
		}

		[Test]
		public void MultipleErrors_SmallerThanBatchSize_ManuallySetBatchSize3000() //do this
		{
			//Arrange
			_instanceSettingRepository.GetConfigurationValue("kCura.IntegrationPoints", "RemoveErrorsFromScratchTableBatchSize").Returns("3000");

			//Act
			_instance.SetBatchSize();
			for (int numErrors = 0; numErrors < 2999; numErrors++)
			{
				_instance.OnRowError("docIdentifier", "Message");
			}

			_instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done

			//Assert
			_scratchTable.Received(1).RemoveErrorDocuments(Arg.Any<ICollection<string>>());
			_instanceSettingRepository.Received(1)
				.GetConfigurationValue("kCura.IntegrationPoints", "RemoveErrorsFromScratchTableBatchSize");
		}

		[Test]
		public void MultipleErrors_EqualToBatchSize_ManuallySetBatchSize5000()
		{
			//Arrange
			_instanceSettingRepository.GetConfigurationValue("kCura.IntegrationPoints", "RemoveErrorsFromScratchTableBatchSize").Returns("5000");

			//Act
			_instance.SetBatchSize();
			for (int numErrors = 0; numErrors < 5000; numErrors++)
			{
				_instance.OnRowError("docIdentifier", "Message");
			}

			_instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done, should not trigger a flush

			//Assert
			_scratchTable.Received(1).RemoveErrorDocuments(Arg.Any<ICollection<string>>());
			_instanceSettingRepository.Received(1)
				.GetConfigurationValue("kCura.IntegrationPoints", "RemoveErrorsFromScratchTableBatchSize");
		}

		[Test]
		public void MultipleErrors_EqualToBatchSize_ManuallySetMissingBatchSize()
		{
			//Arrange
			_instanceSettingRepository.GetConfigurationValue("kCura.IntegrationPoints", "RemoveErrorsFromScratchTableBatchSize").Returns(String.Empty);

			//Act
			_instance.SetBatchSize();
			for (int numErrors = 0; numErrors < 1000; numErrors++)
			{
				_instance.OnRowError("docIdentifier", "Message");
			}

			_instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done, should not trigger a flush

			//Assert
			_scratchTable.Received(1).RemoveErrorDocuments(Arg.Any<ICollection<string>>());
			_instanceSettingRepository.Received(1)
				.GetConfigurationValue("kCura.IntegrationPoints", "RemoveErrorsFromScratchTableBatchSize");
		}

		[Test]
		public void MultipleErrors_GreaterThanBatchSize_AutoBatchSize()
		{
			//Act
			for (int numErrors = 0; numErrors < 1001; numErrors++)
			{
				_instance.OnRowError("docIdentifier", "Message");
			}

			_instance.OnBatchComplete(_dummyDate, _dummyDate, 5, 1); //job done, errors leftover so a flush is triggered

			//Assert
			_scratchTable.Received(2).RemoveErrorDocuments(Arg.Any<ICollection<string>>());
		}
	}
}
