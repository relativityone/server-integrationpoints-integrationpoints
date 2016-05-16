using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ChoiceService
	{
		private readonly IChoiceQuery _query;
		public ChoiceService(IChoiceQuery query)
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
