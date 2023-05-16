using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeInstanceSettingsBundle : IInstanceSettingsBundle
    {
        public IDictionary<string, object> SmtpConfigurationSettings { get; set; }

        public FakeInstanceSettingsBundle()
        {
            SmtpConfigurationSettings = new Dictionary<string, object>();
        }

        public Task<string> GetStringAsync(string section, string name)
        {
            return Task.FromResult(GetString(section, name));
        }

        public string GetString(string section, string name)
        {
            var success = SmtpConfigurationSettings.TryGetValue(name, out object value);

            return success ? value.ToString() : string.Empty;
        }

        public Task<uint?> GetUIntAsync(string section, string name)
        {
            return Task.FromResult(GetUInt(section, name));
        }

        public uint? GetUInt(string section, string name)
        {
            var success = SmtpConfigurationSettings.TryGetValue(name, out object value);

            return success ? (uint?)value : null;
        }

        public Task<int?> GetIntAsync(string section, string name)
        {
            return Task.FromResult(GetInt(section, name));
        }

        public int? GetInt(string section, string name)
        {
            var success = SmtpConfigurationSettings.TryGetValue(name, out object value);

            return success ? (int?)value : null;
        }

        public Task<long?> GetLongAsync(string section, string name)
        {
            return Task.FromResult(GetLong(section, name));
        }

        public long? GetLong(string section, string name)
        {
            var success = SmtpConfigurationSettings.TryGetValue(name, out object value);

            return success ? (long?)value : null;
        }

        public Task<ulong?> GetULongAsync(string section, string name)
        {
            return Task.FromResult(GetULong(section, name));
        }

        public ulong? GetULong(string section, string name)
        {
            var success = SmtpConfigurationSettings.TryGetValue(name, out object value);

            return success ? (ulong?)value : null;
        }

        public Task<bool?> GetBoolAsync(string section, string name)
        {
            return Task.FromResult(GetBool(section, name));
        }

        public bool? GetBool(string section, string name)
        {
            var success = SmtpConfigurationSettings.TryGetValue(name, out object value);

            return success ? (bool?)value : null;
        }

        public Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>> GetRawValuesAsync()
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> GetRawValues()
        {
            throw new System.NotImplementedException();
        }

        public void ForceRefresh()
        {
            throw new System.NotImplementedException();
        }
    }
}
