using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.DataStructures;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class GetAvailableEncodingsController : ApiController
    {
        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve encoding type list.")]
        public HttpResponseMessage Get()
        {
            List<AvailableEncodingInfo> encodings = new List<AvailableEncodingInfo>();

            foreach (EncodingInfo info in Encoding.GetEncodings())
            {
                encodings.Add(new AvailableEncodingInfo()
                {
                    DisplayName = info.DisplayName,
                    Name = info.Name
                });
            }

            encodings.Sort((x, y) => string.CompareOrdinal(x.DisplayName, y.DisplayName));
            return Request.CreateResponse(HttpStatusCode.OK, encodings);
        }
    }
}
