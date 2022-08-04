using Newtonsoft.Json;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Data
{
    public partial class SourceProvider : BaseRdo
    {
        public SourceProviderConfiguration Config
        {
            get
            {
                string config = Configuration;
                return config == null ? new SourceProviderConfiguration() : JsonConvert.DeserializeObject<SourceProviderConfiguration>(config);
            }
            set
            {
                string val = value == null ? JsonConvert.SerializeObject(new SourceProviderConfiguration()) : JsonConvert.SerializeObject(value);
                Configuration = val;
            }
        }
    }
}
