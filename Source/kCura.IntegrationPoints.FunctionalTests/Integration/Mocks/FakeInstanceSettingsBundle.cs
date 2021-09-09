using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeInstanceSettingsBundle : IInstanceSettingsBundle
    {
        public string GetString(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetStringAsync(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public uint? GetUInt(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<uint?> GetUIntAsync(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public int? GetInt(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<int?> GetIntAsync(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public long? GetLong(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<long?> GetLongAsync(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public ulong? GetULong(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<ulong?> GetULongAsync(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public bool? GetBool(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool?> GetBoolAsync(string section, string name)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> GetRawValues()
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>> GetRawValuesAsync()
        {
            throw new System.NotImplementedException();
        }

        public void ForceRefresh()
        {
            throw new System.NotImplementedException();
        }
    }
}
