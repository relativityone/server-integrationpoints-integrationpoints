using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ChoiceService
	{
		private readonly ChoiceQuery _query;
		public ChoiceService(ChoiceQuery query)
		{
			_query = query;
		}

		public List<Relativity.Client.DTOs.Choice> GetChoicesOnField(int fieldArtifactID)
		{
			return _query.GetChoicesOnField(fieldArtifactID);
		}
		public List<Relativity.Client.DTOs.Choice> GetChoicesOnField(Guid fieldGuid)
		{
			return _query.GetChoicesOnField(fieldGuid);
		} 

	}
}
