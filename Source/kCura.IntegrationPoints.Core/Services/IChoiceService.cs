using System;
using System.Collections.Generic;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IChoiceService
    {
        List<FieldEntry> GetChoiceFields(int workspaceId, int rdoTypeId);
    }
}
