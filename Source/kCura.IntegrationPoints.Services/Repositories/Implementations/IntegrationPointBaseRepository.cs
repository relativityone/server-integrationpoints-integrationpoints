using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public abstract class IntegrationPointBaseRepository
	{
		private readonly IBackwardCompatibility _backwardCompatibility;

		protected IntegrationPointBaseRepository(IBackwardCompatibility backwardCompatibility)
		{
			_backwardCompatibility = backwardCompatibility;
		}

		protected int SaveIntegrationPoint(CreateIntegrationPointRequest request)
		{
			var overwriteFieldsName = GetOverwriteFieldsName(request.IntegrationPoint.OverwriteFieldsChoiceId);
			_backwardCompatibility.FixIncompatibilities(request.IntegrationPoint, overwriteFieldsName);
			return Save(request.IntegrationPoint, overwriteFieldsName);
		}

		public abstract int Save(IntegrationPointModel model, string overwriteFieldsName);
		
		private string GetOverwriteFieldsName(int overwriteFieldsId)
		{
			//TODO remove this hack when IntegrationPointModel will start using ChoiceId instead of ChoiceName
			return GetOverwriteFieldChoices().First(x => x.ArtifactId == overwriteFieldsId).Name;
		}

		public abstract IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
	}
}