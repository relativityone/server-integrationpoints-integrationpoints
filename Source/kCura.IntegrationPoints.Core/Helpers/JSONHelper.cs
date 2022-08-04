using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public static class JSONHelper
    {
        public static JsonSerializerSettings SetupDefaults(this JsonSerializerSettings @this)
        {
            if (@this != null)
            {
                @this.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            return @this;
        }

        public static JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings().SetupDefaults();
        }
    }
}