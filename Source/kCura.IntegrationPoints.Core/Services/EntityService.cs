using System;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services
{
	public class EntityService
	{
		private readonly IObjectTypeRepository _objectTypeRepository;
		public EntityService(IObjectTypeRepository objectTypeRepository)
		{
			_objectTypeRepository = objectTypeRepository;
		}

		public bool IsEntity(int id)
		{
			var guids = _objectTypeRepository.GetObjectType(id).Guids;
			return guids.Any(x => x.Equals(Guid.Parse(GlobalConst.Custodian)));
		}
	}
}
