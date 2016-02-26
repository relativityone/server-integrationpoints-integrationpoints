﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class GetAvailableEncodingsController : ApiController
	{
		[HttpGet]
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

			encodings.Sort((x, y) => String.CompareOrdinal(x.DisplayName, y.DisplayName));
			return Request.CreateResponse(HttpStatusCode.OK, encodings);
		}

		private class AvailableEncodingInfo
		{
			public string DisplayName { get; set; }
			public string Name { get; set; }
		}
	}
}