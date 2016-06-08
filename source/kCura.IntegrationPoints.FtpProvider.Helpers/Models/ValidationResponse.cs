using System;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models
{
    public class ValidationResponse
    {
        public Boolean Success { get; set; }
        public String Message { get; set; }

        public String ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
