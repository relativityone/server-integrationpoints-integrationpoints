using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Extensions;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class ChoiceRepository : IChoiceRepository
	{
		private readonly IChoiceQuery _choiceQuery;

		public ChoiceRepository(IChoiceQuery choiceQuery)
		{
			_choiceQuery = choiceQuery;
		}

		public IList<OverwriteFieldsModel> GetOverwriteFieldChoices()
		{
			var choices = _choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
			return choices.Select(x => x.ToModel()).ToList();
		}
	}
}