using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Models
{
	public class RdoFilter
	{
		private RelativityRdoQuery _rdoQuery;

		public RdoFilter(RelativityRdoQuery rdoQuery)
		{
			_rdoQuery = rdoQuery;
		}

		private List<string> systemRdo {
			get
			{
				return new List<string>
				{
					"History","Event Handler","Install Event Handler","Source Provider","Integration Point"
				};
			}
		}


		public List<ObjectType> FilterRdo()
		{
			var list = _rdoQuery.GetAllRdo();
			for (var i = 0; i < list.Count; i++)
			{
				if (systemRdo.Contains(list[i].Name))
				{
					list.Remove(list[i]);
				}
			}
			return list ; 
		}

	}
}
