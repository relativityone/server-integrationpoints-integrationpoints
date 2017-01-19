using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FtpProvider.Tests
{
	[TestFixture]
	public class FtpProviderTests : TestBase
	{
		private IConnectorFactory _connectorFactory;
		private ISettingsManager _settingsManager;
		private IParserFactory _parserFactory;
		private IDataReaderFactory _dataReaderFactory;
		private IHelper _helper;
		private ParserOptions _parserOptions;

		[SetUp]
		public override void SetUp()
		{
			_connectorFactory = NSubstitute.Substitute.For<IConnectorFactory>();
			_settingsManager = NSubstitute.Substitute.For<ISettingsManager>();
			_parserFactory = NSubstitute.Substitute.For<IParserFactory>();
			_dataReaderFactory = NSubstitute.Substitute.For<IDataReaderFactory>();
			_helper = NSubstitute.Substitute.For<IHelper>();
			_parserOptions = ParserOptions.GetDefaultParserOptions();
		}

		[Test, System.ComponentModel.Description("Validates columns when all match")]
		[TestCase("AAA, bbb ,CcC")]
		[TestCase("\"AAA\", \"bbb\",\"CcC\"")]
		public void ValidateColumns_AllMatch(string columns)
		{
			Settings settings = new Settings()
			{
				ColumnList = new List<FieldEntry>()
				{
					new FieldEntry() {FieldIdentifier = "aAa"},
					new FieldEntry() {FieldIdentifier = "BBB"},
					new FieldEntry() {FieldIdentifier = "CcC"}
				}
			};

			FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.DoesNotThrow(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
		}

		[Test, System.ComponentModel.Description("Validates columns when some missing 2 vs 3")]
		public void ValidateColumns_OneMissing()
		{
			string columns = "AAA,bbb";
			Settings settings = new Settings()
			{
				ColumnList = new List<FieldEntry>()                 {
					new FieldEntry() {FieldIdentifier = "aAa"},
					new FieldEntry() {FieldIdentifier = "BBB"},
					new FieldEntry() {FieldIdentifier = "CcC"}
				}

			};

			FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
		}

		[Test, System.ComponentModel.Description("Validates columns when all missing 0 vs 3")]
		public void ValidateColumns_AllMissing()
		{
			string columns = string.Empty;
			Settings settings = new Settings()
			{
				ColumnList = new List<FieldEntry>()
				{
					new FieldEntry() {FieldIdentifier = "aAa"},
					new FieldEntry() {FieldIdentifier = "BBB"},
					new FieldEntry() {FieldIdentifier = "CcC"}
				}

			};

			FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
		}

		[Test, System.ComponentModel.Description("Validates columns when some missing 3 vs 4")]
		public void ValidateColumns_SomeMissing2()
		{
			string columns = "AAA,bbb,CcC";
			Settings settings = new Settings()
			{
				ColumnList = new List<FieldEntry>()
				{
					new FieldEntry() {FieldIdentifier = "aAa"},
					new FieldEntry() {FieldIdentifier = "BBB"},
					new FieldEntry() {FieldIdentifier = "CcC"},
					new FieldEntry() {FieldIdentifier = "ddd"}
				}
			};

			FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
		}

		[Test, System.ComponentModel.Description("Validates columns when all missing in settings 3 vs 0")]
		public void ValidateColumns_SomeMissing()
		{
			string columns = "AAA,bbb,CcC";
			Settings settings = new Settings()
			{
				ColumnList = new List<FieldEntry>() { }
			};

			FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
		}
	}
}