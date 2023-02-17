using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.LDAPProvider
{
    [DataSourceProvider("5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232")]
    public class LDAPProvider : IDataSourceProvider
    {
        private readonly IAPILog _logger;
        private readonly ILDAPSettingsReader _reader;
        private readonly IHelper _helper;
        private readonly ILDAPServiceFactory _ldapServiceFactory;
        private readonly ISerializer _serializer;

        public LDAPProvider(ILDAPSettingsReader reader, ILDAPServiceFactory ldapServiceFactory, IHelper helper, ISerializer serializer)
        {
            _reader = reader;
            _ldapServiceFactory = ldapServiceFactory;
            _helper = helper;
            _serializer = serializer;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<LDAPProvider>();
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
        {
            IList<string> entryIdsList = entryIds.ToList();

            LogRetrievingData(entryIdsList);

            LDAPSettings settings = _reader.GetSettings(providerConfiguration.Configuration);
            List<string> fieldsToLoad = fields.Select(f => f.FieldIdentifier).ToList();
            string identifier = fields.Where(f => f.IsIdentifier).Select(f => f.FieldIdentifier).First();

            LDAPSecuredConfiguration securedConfiguration =
                _serializer.Deserialize<LDAPSecuredConfiguration>(providerConfiguration.SecuredConfiguration);

            ILDAPService ldapService = _ldapServiceFactory.Create(_logger, _serializer, settings, securedConfiguration, fieldsToLoad);
            ldapService.InitializeConnection();
            ldapService.FetchItems();
            return new LDAPServiceDataReader(ldapService, entryIdsList, identifier, fieldsToLoad,
                new LDAPDataFormatterDefault(settings, _helper));
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException($"Argument named '{nameof(identifier)}' cannot be null.");
            }
            LogRetrievingBatchableIds(identifier);

            LDAPSettings settings = _reader.GetSettings(providerConfiguration.Configuration);
            var fieldsToLoad = new List<string> {identifier.FieldIdentifier};

            LDAPSecuredConfiguration securedConfiguration =
                _serializer.Deserialize<LDAPSecuredConfiguration>(providerConfiguration.SecuredConfiguration);

            ILDAPService ldapService = _ldapServiceFactory.Create(_logger, _serializer, settings, securedConfiguration, fieldsToLoad);
            ldapService.InitializeConnection();
            IEnumerable<SearchResult> items = ldapService.FetchItems();
            return new LDAPDataReader(items, fieldsToLoad, new LDAPDataFormatterForBatchableIDs(settings, _helper));
        }

        public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            LogRetrievingFields();

            LDAPSettings settings = _reader.GetSettings(providerConfiguration.Configuration);
            settings.PropertyNamesOnly = true;

            LDAPSecuredConfiguration securedConfiguration =
                _serializer.Deserialize<LDAPSecuredConfiguration>(providerConfiguration.SecuredConfiguration);

            ILDAPService ldapService = _ldapServiceFactory.Create(_logger, _serializer, settings, securedConfiguration);
            ldapService.InitializeConnection();
            List<string> fields = ldapService.FetchAllProperties(settings.GetPropertiesItemSearchLimit);

            return fields.Select(f => new FieldEntry {DisplayName = f, FieldIdentifier = f});
        }

        #region Logging

        private void LogRetrievingFields()
        {
            _logger.LogInformation("Attempting to retrieve fields in LDAP Provider.");
        }

        private void LogRetrievingData(IList<string> entryIds )
        {
            _logger.LogInformation("Attempting to retrieve data in LDAP Provider for IDs count {count}.", entryIds.Count);
        }

        private void LogRetrievingBatchableIds(FieldEntry entry)
        {
            _logger.LogInformation("Attempting to retrieve batchable ids in LDAP Provider for field {FieldIdentifier}",
                entry.FieldIdentifier);
        }

        #endregion
    }
}
