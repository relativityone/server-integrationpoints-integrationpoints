using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using ObjectTypeGuids = kCura.IntegrationPoints.Core.Contracts.Entity.ObjectTypeGuids;

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
            IEnumerable<Guid> guids = _objectTypeRepository.GetObjectType(id).Guids;
            return guids.Any(x => x.Equals(ObjectTypeGuids.Entity));
        }
    }
}
