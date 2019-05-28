﻿using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	public class ImportTransferDataContextTests : TestBase
	{
		private IDataReader _dataReader;
		private ImportTransferDataContext _instance;

		[SetUp]
		public override void SetUp()
		{
			_dataReader = Substitute.For<IDataReader>();
			IDataReaderFactory dataReaderFactory = Substitute.For<IDataReaderFactory>();
			dataReaderFactory.GetDataReader(Arg.Any<FieldMap[]>(), Arg.Any<string>()).Returns(_dataReader);

			_instance = new ImportTransferDataContext(dataReaderFactory, string.Empty, new List<FieldMap>());
		}

		[Test]
		public void ItShouldReturnDataReaderAssignedInConstructor()
		{
			//Assert
			Assert.AreSame(_dataReader, _instance.DataReader);
		}

		[Test]
		public void ItShouldCallDisposeOnDataReader()
		{
			_instance.Dispose();

			_dataReader.Received(1).Dispose();
		}

		[Test]
		public void TestTotalItemsFoundProperty()
		{
			_instance.TotalItemsFound = 42;
			Assert.AreEqual(42, _instance.TotalItemsFound);
		}
	}
}
