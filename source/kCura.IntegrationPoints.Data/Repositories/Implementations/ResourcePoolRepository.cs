using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Choice;
using Relativity.Services.ResourcePool;
using IResourcePoolManagerSvc = Relativity.Services.ResourcePool.IResourcePoolManager;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ResourcePoolRepository : IResourcePoolRepository
	{
		#region Fields

		private readonly IHelper _helper;

		#endregion //Fields

		#region Constructors

		public ResourcePoolRepository(IHelper helper)
		{
			_helper = helper;
		}

		#endregion //Constructors

		#region Methods

		public List<ProcessingSourceLocationDTO> GetProcessingSourceLocationsBy(int resourcePoolId)
		{
			List<ProcessingSourceLocationDTO> processingSourceLocations = new List<ProcessingSourceLocationDTO>();
			using (IResourcePoolManagerSvc resourcePoolManagerSvcProxy =
					_helper.GetServicesManager().CreateProxy<IResourcePoolManagerSvc>(ExecutionIdentity.CurrentUser))
			{
				List<ChoiceRef> choiceRefs = resourcePoolManagerSvcProxy.GetProcessingSourceLocationsAsync(
					new ResourcePoolRef(resourcePoolId)).Result;

				choiceRefs.ForEach(choiceRef => processingSourceLocations.Add(
					new ProcessingSourceLocationDTO
					{
						ArtifactId = choiceRef.ArtifactID,
						Location = choiceRef.Name
					}));
			}
			return processingSourceLocations;
		}

		#endregion Methods
	}
}
