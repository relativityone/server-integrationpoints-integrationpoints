using System;
using System.Collections.Generic;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Data
{
    public interface IChoiceQuery
    {
        List<ChoiceRef> GetChoicesOnField(int workspaceArtifactId, Guid fieldGuid);
    }
}
