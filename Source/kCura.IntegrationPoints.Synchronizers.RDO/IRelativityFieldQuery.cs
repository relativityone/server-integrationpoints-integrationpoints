using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public interface IRelativityFieldQuery
    {
        List<RelativityObject> GetFieldsForRdo(int rdoTypeId);
    }
}
