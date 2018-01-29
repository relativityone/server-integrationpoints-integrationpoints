using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.UtilityDTO
{
	public class ResultSet<T> where T: BaseRdo
	{
		public int ResultCount { get; set; }
		public int TotalCount { get; set; }
		public List<T> Items { get; set; }
	}
}
