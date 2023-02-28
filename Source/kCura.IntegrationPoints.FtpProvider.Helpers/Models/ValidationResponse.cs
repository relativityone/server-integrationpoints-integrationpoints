using System;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models
{
    public class ValidationResponse
    {
        public Boolean Success { get; set; }

        public string Message { get; set; }

        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
