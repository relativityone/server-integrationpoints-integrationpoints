using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.LDAPProvider.Tests
{
    [TestFixture, Category("Unit")]
    public class LDAPProviderTests : TestBase
    {
        private LDAPSettings _fullyFilledSettings;
        private ILDAPServiceFactory _serviceFactory;
        private IAPILog _logger;
        private ISerializer _serializer;
        private ILDAPService _ldapService;
        private IHelper _helper;
        private List<string> _fieldProperties;
        private List<FieldEntry> _fieldEntries;
        private ILDAPSettingsReader _ldapSettingsReader;
        private List<string> _entryIds;
        private string _optionsString = "options are mocked anyway";
        private LDAPSecuredConfiguration _securedConfiguration;
        private string _securedConfigurationString = "secured options too";

        public override void SetUp()
        {
            _ldapService = Substitute.For<ILDAPService>();
            _serviceFactory = Substitute.For<ILDAPServiceFactory>();
            _serializer = Substitute.For<ISerializer>();

            _serviceFactory.Create(_logger, _serializer, _fullyFilledSettings, _securedConfiguration).ReturnsForAnyArgs(_ldapService);

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

            _securedConfiguration = new LDAPSecuredConfiguration()
            {
                UserName = "username",
                Password = "password"
            };

            _fieldProperties = new List<string> { "1prop", "2prop", "3prop", "4prop", "5prop", };
            _fieldEntries = new List<FieldEntry>
            {
                new FieldEntry(),
                new FieldEntry(),
                new FieldEntry()
            };
            _entryIds = new List<string>();
        }

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _ldapSettingsReader = Substitute.For<ILDAPSettingsReader>();
            _ldapSettingsReader.GetSettings(Arg.Any<string>()).ReturnsForAnyArgs(_fullyFilledSettings);
            _helper = Substitute.For<IHelper>();
            _logger = Substitute.For<IAPILog>();
       }

        [Test]
        public void GetData_FieldsListHasIdentifierElement_CallsFetchItemsAndReturnsProperDataReader()
        {
            _fieldEntries[1].IsIdentifier = true;
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);

            IDataReader reader = provider.GetData(_fieldEntries, _entryIds, new DataSourceProviderConfiguration(_optionsString, _securedConfigurationString));

            _ldapService.ReceivedWithAnyArgs().FetchItems();
            Assert.IsInstanceOf<LDAPServiceDataReader>(reader);
        }

        [Test]
        public void GetData_FieldsListDoesNotHaveIdentifierElement_Throws()
        {
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);

            Assert.Catch(() => provider.GetData(_fieldEntries, _entryIds, new DataSourceProviderConfiguration(_optionsString, _securedConfigurationString)));
        }

        [Test]
        public void GetData_FieldsCollectionEmpty_Throws()
        {
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);
            var emptyFieldEntriesList = new List<FieldEntry>();

            Assert.Catch(() => provider.GetData(emptyFieldEntriesList, _entryIds, new DataSourceProviderConfiguration(_optionsString, _securedConfigurationString)));
        }

        [Test]
        public void GetBatchableIds_CallsFetchItemsAndReturnsProperDataReader()
        {
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);

            IDataReader reader = provider.GetBatchableIds(new FieldEntry(), new DataSourceProviderConfiguration(_optionsString, _securedConfigurationString));

            _ldapService.ReceivedWithAnyArgs().FetchItems();
            Assert.IsInstanceOf<LDAPDataReader>(reader);
        }

        [Test]
        public void GetBatchableIds_IdentifierIsNull_ThrowsArgumentNullException()
        {
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);

            Assert.Throws<ArgumentNullException>(() => provider.GetBatchableIds(null, new DataSourceProviderConfiguration(_optionsString, _securedConfigurationString)));
        }

        [Test]
        public void GetFields_LdapServiceReturnsEmptyList_ReturnsEmptyCollection()
        {
            _ldapService.FetchAllProperties(Arg.Any<int?>()).ReturnsForAnyArgs(new List<string>());
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);

            IEnumerable<FieldEntry> result = provider.GetFields(new DataSourceProviderConfiguration("", _securedConfigurationString));

            Assert.AreEqual(result.Any(), false);
        }

        [Test]
        public void GetFields_LdapServiceReturnsProperList_ReturnsCollectionWithProperCount()
        {
            _ldapService.FetchAllProperties(Arg.Any<int?>()).ReturnsForAnyArgs(_fieldProperties);
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);

            IEnumerable<FieldEntry> result = provider.GetFields(new DataSourceProviderConfiguration("", _securedConfigurationString));

            Assert.AreEqual(result.Count(), _fieldProperties.Count);
        }

        [TestCase("FirstParticularProperty")]
        [TestCase("SecondParticularProperty")]
        public void GetFields_LdapServiceReturnsOneElement_ReturnsProperElement(string testString)
        {
            _ldapService.FetchAllProperties(Arg.Any<int?>()).ReturnsForAnyArgs(new List<string> {testString});
            var provider = new LDAPProvider(_ldapSettingsReader, _serviceFactory, _helper, _serializer);

            IEnumerable<FieldEntry> result = provider.GetFields(new DataSourceProviderConfiguration("", _securedConfigurationString));
            FieldEntry firstFromCollection = result.First();

            Assert.AreEqual(firstFromCollection.DisplayName, testString);
            Assert.AreEqual(firstFromCollection.FieldIdentifier, testString);
        }
    }
}
