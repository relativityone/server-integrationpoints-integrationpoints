using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web.Models
{
	public class EditPoint
	{
		public int AppID { get; set; }
		public int ArtifactID { get; set; }
		public int UserID { get; set; }
		public int CaseUserID { get; set; }
		public string URL { get; set; }
		public bool ShowRelativityDataProvider { get; set; }
	}
}