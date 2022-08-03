using System;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LDAPSettingsReader : ILDAPSettingsReader
    {
        private readonly IAPILog _logger;

        public LDAPSettingsReader(IHelper helper)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<LDAPSettingsReader>();
        }

        public LDAPSettings GetSettings(string sourceConfiguration)
        {
            if (string.IsNullOrWhiteSpace(sourceConfiguration))
            {
                throw new ArgumentException($"Parameter named '{nameof(sourceConfiguration)}' cannot be empty");
            }

            string unwoundSourceConfiguration = UnwindIfNecessary(sourceConfiguration);

            LDAPSettings settings = Deserialize(unwoundSourceConfiguration);
            SetDefaultValues(settings);

            return settings;
        }

        public LDAPSettings Deserialize(string sourceConfiguration)
        {
            try
            {
                return JsonConvert.DeserializeObject<LDAPSettings>(sourceConfiguration);
            }
            catch (Exception ex)
            {
                throw new LDAPProviderException("Could not deserialize LDAP settings.", ex);
            }
        }

        private string UnwindIfNecessary(string options)
        {
            try
            {
                options = JsonConvert.DeserializeObject<string>(options);
            }
            catch (JsonReaderException)
            {
                // We're just checking if options aren't sent as JSON string inside JSON. 
                // If deserialization throws it only means this wasn't the case and we're returning original options string as a result.
            }

            return options;
        }

        private void SetDefaultValues(LDAPSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Filter))
            {
                settings.Filter = LDAPSettings.FILTER_DEFAULT;
            }

            if (settings.PageSize < 1)
            {
                settings.PageSize = LDAPSettings.PAGESIZE_DEFAULT;
            }

            if (settings.GetPropertiesItemSearchLimit < 1)
            {
                settings.GetPropertiesItemSearchLimit = LDAPSettings.GETPROPERTIESITEMSEARCHLIMIT_DEFAULT;
            }

            if (!settings.MultiValueDelimiter.HasValue || settings.MultiValueDelimiter.ToString() == string.Empty)
            {
                //not knowing what data can look like we will assume 
                //blank entry (" ") is possible user entry as legit delimiter
                LogUsageOfDefaultMultiValueDelimiter();
                settings.MultiValueDelimiter = LDAPSettings.MULTIVALUEDELIMITER_DEFAULT;
            }
        }

        #region logging
        
        private void LogUsageOfDefaultMultiValueDelimiter()
        {
            _logger.LogWarning(
                "LDAPSettings does not contain Multivalue delimiter. Using default delimiter: ({DefaultDelimiter})",
                LDAPSettings.MULTIVALUEDELIMITER_DEFAULT);
        }

        #endregion
    }
}