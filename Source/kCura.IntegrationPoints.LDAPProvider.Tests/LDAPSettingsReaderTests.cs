using System;
using System.DirectoryServices;
using kCura.IntegrationPoint.Tests.Core;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider.Tests
{
    [TestFixture, Category("Unit")]
    public class LDAPSettingsReaderTests : TestBase
    {
        private LDAPSettings _fullyFilledSettings;
        private string _faultySettingsString = "Not a proper settings string";
        private IHelper _helper;

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _helper = Substitute.For<IHelper>();
        }

        public override void SetUp()
        {
            _fullyFilledSettings = new LDAPSettings
            {
                AttributeScopeQuery = "scope",
                ConnectionAuthenticationType = AuthenticationTypesEnum.Delegation,
                ConnectionPath = "connection path",
                Filter = "filter",
                GetPropertiesItemSearchLimit = 123,
                IgnorePathValidation = true,
                ImportNested = true,
                MultiValueDelimiter = '_',
                PageSize = 432,
                PropertyNamesOnly = true,
                ProviderExtendedDN = ExtendedDNEnum.HexString,
                ProviderReferralChasing = ReferralChasingOption.External,
                SizeLimit = 4231
            };
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("       ")]
        public void GetSettings_InputStringNullOrEmpty_ThrowsArgumentException(string settingsString)
        {
            var provider = new LDAPSettingsReader(_helper);

            Assert.Throws<ArgumentException>(() => provider.GetSettings(settingsString));
        }

        [Test]
        public void GetSettings_InputNeedsUnwinding_ReturnsProperSettings()
        {
            string serializedSettings = JsonConvert.SerializeObject(_fullyFilledSettings);
            string woundSettings = JsonConvert.SerializeObject(serializedSettings);
            var provider = new LDAPSettingsReader(_helper);

            LDAPSettings result = provider.GetSettings(woundSettings);

            Assert.AreEqual(result.ConnectionPath, _fullyFilledSettings.ConnectionPath);
        }

        [TestCase(0)]
        [TestCase(-52943)]
        public void GetSettings_InputPageSizeShouldBeDefaulted_ReturnsProperSettings(int pageSize)
        {
            _fullyFilledSettings.PageSize = pageSize;
            string serializedSettings = JsonConvert.SerializeObject(_fullyFilledSettings);
            var provider = new LDAPSettingsReader(_helper);

            LDAPSettings result = provider.GetSettings(serializedSettings);

            Assert.AreEqual(result.PageSize, LDAPSettings.PAGESIZE_DEFAULT);
        }

        [TestCase("")]
        [TestCase("        ")]
        [TestCase(null)]
        public void GetSettings_InputFilterShouldBeDefaulted_ReturnsProperSettings(string filter)
        {
            _fullyFilledSettings.Filter = filter;
            string serializedSettings = JsonConvert.SerializeObject(_fullyFilledSettings);
            var provider = new LDAPSettingsReader(_helper);

            LDAPSettings result = provider.GetSettings(serializedSettings);

            Assert.AreEqual(result.Filter, LDAPSettings.FILTER_DEFAULT);
        }

        [TestCase(0)]
        [TestCase(-52943)]
        public void GetSettings_InputGetPropertiesItemSearchLimitShouldBeDefaulted_ReturnsProperSettings(int getPropertiesItemSearchLimit)
        {
            _fullyFilledSettings.GetPropertiesItemSearchLimit = getPropertiesItemSearchLimit;
            string serializedSettings = JsonConvert.SerializeObject(_fullyFilledSettings);
            var provider = new LDAPSettingsReader(_helper);

            LDAPSettings result = provider.GetSettings(serializedSettings);

            Assert.AreEqual(result.GetPropertiesItemSearchLimit, LDAPSettings.GETPROPERTIESITEMSEARCHLIMIT_DEFAULT);
        }

        [Test]
        public void GetSettings_InputMultiValueDelimiterShouldBeDefaulted_ReturnsProperSettings()
        {
            _fullyFilledSettings.MultiValueDelimiter = null;
            string serializedSettings = JsonConvert.SerializeObject(_fullyFilledSettings);
            var provider = new LDAPSettingsReader(_helper);

            LDAPSettings result = provider.GetSettings(serializedSettings);

            Assert.AreEqual(result.MultiValueDelimiter, LDAPSettings.MULTIVALUEDELIMITER_DEFAULT);
        }

        [Test]
        public void GetSettings_InputsValidSettingsString_ReturnedSettingsAreEqual()
        {
            string serializedSettings = JsonConvert.SerializeObject(_fullyFilledSettings);
            var provider = new LDAPSettingsReader(_helper);

            LDAPSettings result = provider.GetSettings(serializedSettings);

            Assert.AreEqual(result.AttributeScopeQuery, _fullyFilledSettings.AttributeScopeQuery);
            Assert.AreEqual(result.ConnectionAuthenticationType, _fullyFilledSettings.ConnectionAuthenticationType);
            Assert.AreEqual(result.ConnectionPath, _fullyFilledSettings.ConnectionPath);
            Assert.AreEqual(result.Filter, _fullyFilledSettings.Filter);
            Assert.AreEqual(result.GetPropertiesItemSearchLimit, _fullyFilledSettings.GetPropertiesItemSearchLimit);
            Assert.AreEqual(result.IgnorePathValidation, _fullyFilledSettings.IgnorePathValidation);
            Assert.AreEqual(result.ImportNested, _fullyFilledSettings.ImportNested);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
            Assert.AreEqual(result.PageSize, _fullyFilledSettings.PageSize);
            Assert.AreEqual(result.PropertyNamesOnly, _fullyFilledSettings.PropertyNamesOnly);
            Assert.AreEqual(result.ProviderExtendedDN, _fullyFilledSettings.ProviderExtendedDN);
            Assert.AreEqual(result.ProviderReferralChasing, _fullyFilledSettings.ProviderReferralChasing);
            Assert.AreEqual(result.SizeLimit, _fullyFilledSettings.SizeLimit);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
        }

        [Test]
        public void Deserialize_InputIsCorrect_ReturnsDeserializedSettings()
        {
            string serializedSettings = JsonConvert.SerializeObject(_fullyFilledSettings);
            var provider = new LDAPSettingsReader(_helper);

            LDAPSettings result = provider.Deserialize(serializedSettings);

            Assert.AreEqual(result.AttributeScopeQuery, _fullyFilledSettings.AttributeScopeQuery);
            Assert.AreEqual(result.ConnectionAuthenticationType, _fullyFilledSettings.ConnectionAuthenticationType);
            Assert.AreEqual(result.ConnectionPath, _fullyFilledSettings.ConnectionPath);
            Assert.AreEqual(result.Filter, _fullyFilledSettings.Filter);
            Assert.AreEqual(result.GetPropertiesItemSearchLimit, _fullyFilledSettings.GetPropertiesItemSearchLimit);
            Assert.AreEqual(result.IgnorePathValidation, _fullyFilledSettings.IgnorePathValidation);
            Assert.AreEqual(result.ImportNested, _fullyFilledSettings.ImportNested);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
            Assert.AreEqual(result.PageSize, _fullyFilledSettings.PageSize);
            Assert.AreEqual(result.PropertyNamesOnly, _fullyFilledSettings.PropertyNamesOnly);
            Assert.AreEqual(result.ProviderExtendedDN, _fullyFilledSettings.ProviderExtendedDN);
            Assert.AreEqual(result.ProviderReferralChasing, _fullyFilledSettings.ProviderReferralChasing);
            Assert.AreEqual(result.SizeLimit, _fullyFilledSettings.SizeLimit);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
            Assert.AreEqual(result.MultiValueDelimiter, _fullyFilledSettings.MultiValueDelimiter);
        }

        [Test]
        public void Deserialize_InputNotCorrect_ThrowsLDAPProviderException()
        {
            var provider = new LDAPSettingsReader(_helper);

            Assert.Throws<LDAPProviderException>(() => provider.Deserialize(_faultySettingsString));
        }
    }
}
