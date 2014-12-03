using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using kCura.IntegrationPoints.Core.Models;
using Relativity.DragonGrid.Core.Grid;

namespace kCura.IntegrationPoints.Web.Models
{
	public class IpDetailModel
	{
		public IntegrationModel DataModel { get; set; }
		public GridModel Grid { get; set; }
	}
}