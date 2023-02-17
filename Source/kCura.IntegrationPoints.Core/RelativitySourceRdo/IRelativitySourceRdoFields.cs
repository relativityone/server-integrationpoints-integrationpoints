using System;
using System.Collections.Generic;
using Relativity.Services.Interfaces.Field.Models;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public interface IRelativitySourceRdoFields
    {
        void CreateFields(int workspaceId, IDictionary<Guid, BaseFieldRequest> fields);
    }
}
