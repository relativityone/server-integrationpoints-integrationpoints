using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Tests
{
	[TestFixture]
	public class ImportProviderTests : TestBase
	{
		private int MAX_COLS = 100;

		private IFieldParser _fieldParser;
		private IFieldParserFactory _fieldParserFactory;
		private IDataReaderFactory _dataReaderFactory;
		private IEnumerableParserFactory _enumerableParserFactory;
		private IDataTransferLocationServiceFactory _dataTransferLocationServiceFactory;
		private IDataTransferLocationService _dataTransferLocationService;
		private ISerializer _serializer;

		[SetUp]
		public override void SetUp()
		{
			_fieldParser = Substitute.For<IFieldParser>();
			_fieldParserFactory = Substitute.For<IFieldParserFactory>();;
			_dataReaderFactory = Substitute.For<IDataReaderFactory>();;
			_enumerableParserFactory = Substitute.For<IEnumerableParserFactory>();
			_dataTransferLocationServiceFactory = Substitute.For<IDataTransferLocationServiceFactory>();
			_dataTransferLocationService = Substitute.For<IDataTransferLocationService>();
			_serializer = new JSONSerializer();
		}

		[Test]
		public void ImportProviderCanGetFields()
		{
			List<string> testData = TestHeaders((new Random()).Next(MAX_COLS));
			IEnumerator<string> tdEnum = testData.GetEnumerator();
			tdEnum.MoveNext();

			_fieldParserFactory.GetFieldParser(null).ReturnsForAnyArgs(_fieldParser);
			_fieldParser.GetFields().Returns(testData);

			ImportProvider ip = new ImportProvider(_fieldParserFactory, _serializer);
			IEnumerable<FieldEntry> ipFields = ip.GetFields(new DataSourceProviderConfiguration(string.Empty));

			Assert.AreEqual(testData.Count, ipFields.Count());

			int tdIndex = 0;
			foreach (FieldEntry ipEntry in ipFields)
			{
				Assert.AreEqual(tdEnum.Current, ipEntry.DisplayName);
				Assert.AreEqual(tdIndex, Int32.Parse(ipEntry.FieldIdentifier));
				tdIndex++;
				tdEnum.MoveNext();
			}
		}

		[Test]
		public void ImportProviderCannotGetBatchableIds()
		{
			ImportProvider ip = new ImportProvider(_fieldParserFactory, _serializer);
			Assert.Throws<NotImplementedException>(() => ip.GetBatchableIds(null, new DataSourceProviderConfiguration(string.Empty)));
		}

		[Test]
		public void ImportProviderCannotGetData()
		{
			ImportProvider ip = new ImportProvider(_fieldParserFactory, _serializer);
			Assert.Throws<NotImplementedException>(() => ip.GetData(null, null, new DataSourceProviderConfiguration(string.Empty)));
		}

		private List<string> TestHeaders(int fieldCount)
		{
			return
				Enumerable
				.Range(0, fieldCount)
				.Select(x => string.Format("col-{0}", x))
				.ToList();
		}
	}
}

