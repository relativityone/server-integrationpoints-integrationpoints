using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public class IntegrationPointQuery
	{
		private readonly IRSAPIService _context;
		public IntegrationPointQuery(IRSAPIService context)
		{
			_context = context;
		}

		public List<IntegrationPoint> GetIntegrationPoints(List<int> allSourceProviderid)
		{
			var qry = new Query<Relativity.Client.DTOs.RDO>();
			qry.Fields = new List<FieldValue>()
				{
					new FieldValue(Guid.Parse(Data.IntegrationPointFieldGuids.Name))
				};
			qry.Condition = new WholeNumberCondition(IntegrationPointFields.SourceProvider, NumericConditionEnum.In, allSourceProviderid);
			var result = _context.IntegrationPointLibrary.Query(qry);

			return result.ToList();
		} 
	}
}
