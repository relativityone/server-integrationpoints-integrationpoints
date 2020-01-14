using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class AutomapRequest
	{
		public DocumentFieldInfo[] SourceFields { get; set; }
		public DocumentFieldInfo[] DestinationFields { get; set; }
		public bool MatchOnlyIdentifiers { get; set; }
	}
}