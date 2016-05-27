using System.Collections.Generic;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Tests
{
    [TestFixture]
    public class FtpProviderTests
    {
        private IConnectorFactory _connectorFactory;
        private ISettingsManager _settingsManager;
        private IParserFactory _parserFactory;
        private IDataReaderFactory _dataReaderFactory;
        private ParserOptions _parserOptions;

        [SetUp]
        public void Setup()
        {
            _connectorFactory = NSubstitute.Substitute.For<IConnectorFactory>();
            _settingsManager = NSubstitute.Substitute.For<ISettingsManager>();
            _parserFactory = NSubstitute.Substitute.For<IParserFactory>();
            _dataReaderFactory = NSubstitute.Substitute.For<IDataReaderFactory>();
            _parserOptions = ParserOptions.GetDefaultParserOptions();
        }

        [Test, System.ComponentModel.Description("Validates columns when all match")]
        public void ValidateColumns_AllMatch()
        {
            string columns = " AAA,bbb,  CcC";
            Settings settings = new Settings()
            {
                ColumnList = new List<string>() { "aAa", "BBB", "CcC" }
            };

            FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory);

            Assert.DoesNotThrow(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
        }

        [Test, System.ComponentModel.Description("Validates columns when some missing 2 vs 3")]
        public void ValidateColumns_OneMissing()
        {
            string columns = "AAA, bbb";
            Settings settings = new Settings()
            {
                ColumnList = new List<string>() { "aAa", "BBB", "CcC" }
            };

            FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory);

            Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
        }

        [Test, System.ComponentModel.Description("Validates columns when all missing 0 vs 3")]
        public void ValidateColumns_AllMissing()
        {
            string columns = string.Empty;
            Settings settings = new Settings()
            {
                ColumnList = new List<string>() { "aAa", "BBB", "CcC" }
            };

            FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory);

            Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
        }

        [Test, System.ComponentModel.Description("Validates columns when some missing 3 vs 4")]
        public void ValidateColumns_SomeMissing2()
        {
            string columns = "AAA, bbb, CcC";
            Settings settings = new Settings()
            {
                ColumnList = new List<string>() { "aAa", "BBB", "CcC", "ddd" }
            };

            FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory);

            Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
        }

        [Test, System.ComponentModel.Description("Validates columns when all missing in settings 3 vs 0")]
        public void ValidateColumns_SomeMissing()
        {
            string columns = "AAA, bbb,  CcC";
            Settings settings = new Settings()
            {
                ColumnList = new List<string>() { }
            };

            FtpProvider FtpProvider = new FtpProvider(_connectorFactory, _settingsManager, _parserFactory, _dataReaderFactory);

            Assert.Throws<Exceptions.ColumnsMissmatchExcepetion>(() => FtpProvider.ValidateColumns(columns, settings, _parserOptions));
        }

    }
}