using System;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services
{
	public class CustodianService
	{
		private readonly IObjectTypeRepository _objectTypeRepository;
		public CustodianService(IObjectTypeRepository objectTypeRepository)
		{
			_objectTypeRepository = objectTypeRepository;
		}

		public bool IsCustodian(int id)
		{
			var guids = _objectTypeRepository.GetObjectType(id).Guids;
			return guids.Any(x => x.Equals(Guid.Parse(GlobalConst.Custodian)));
		}
	}
}
