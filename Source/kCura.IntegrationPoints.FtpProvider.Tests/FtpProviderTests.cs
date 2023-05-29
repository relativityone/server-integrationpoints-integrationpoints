using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FtpProvider.Tests
{
	[TestFixture, Category("Unit")]
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
		[TestCase("\"AAA\", \"bbb\"  ,\"CcC\"")]
		public void ValidateColumns_AllMatch(string columns)
		{
			var settings = new Settings()
			{
				ColumnList = new List<FieldEntry>()
				{
					new FieldEntry() {FieldIdentifier = "aAa"},
					new FieldEntry() {FieldIdentifier = "BBB"},
					new FieldEntry() {FieldIdentifier = "CcC"}
				}
			};

			var ftpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.DoesNotThrow(() => ftpProvider.ValidateColumns(columns, settings, _parserOptions));
		}

		[Test, System.ComponentModel.Description("Validates columns when some of them are missing")]
		[TestCase("AAA, bbb")]
		[TestCase("bbb")]
		[TestCase("")]
		public void ValidateColumns_SomeMissing(string columns)
		{
			var settings = new Settings()
			{
				ColumnList = new List<FieldEntry>()
				{
					new FieldEntry() {FieldIdentifier = "aAa"},
					new FieldEntry() {FieldIdentifier = "BBB"},
					new FieldEntry() {FieldIdentifier = "CcC"}
				}

			};

			var ftpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.Throws<kCura.IntegrationPoints.FtpProvider.Helpers.Exceptions.ColumnsMissmatchException>(() => ftpProvider.ValidateColumns(columns, settings, _parserOptions));
		}

		[Test, System.ComponentModel.Description("Validates columns when all missing in settings 3 vs 0")]
		public void ValidateColumns_AllMissingInSettings()
		{
			var columns = "AAA,bbb,CcC";
			var settings = new Settings()
			{
				ColumnList = new List<FieldEntry>() { }
			};

			var ftpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory, _helper);

			Assert.Throws<kCura.IntegrationPoints.FtpProvider.Helpers.Exceptions.ColumnsMissmatchException>(() => ftpProvider.ValidateColumns(columns, settings, _parserOptions));
		}
	}
}