using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Choice;
using Relativity.Services.ChoiceQuery;

namespace kCura.IntegrationPoints.Data
{
    public class ChoiceQuery : IChoiceQuery
    {
        private readonly IServicesMgr _servicesMgr;

        public ChoiceQuery(IServicesMgr servicesMgr)
        {
            _servicesMgr = servicesMgr;
        }

        public List<ChoiceRef> GetChoicesOnField(int workspaceArtifactId, Guid fieldGuid)
        {
            List<Choice> choices = GetChoicesOnFieldAsync(workspaceArtifactId, fieldGuid).GetAwaiter().GetResult();
            return choices.Select(x => new ChoiceRef(x.ArtifactID)
            {
                Name = x.Name
            }).ToList();
        }

        private async Task<List<Choice>> GetChoicesOnFieldAsync(int workspaceArtifactId, Guid fieldGuid)
        {
            int fieldId = await ReadFieldIdByGuidAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);

            using (IChoiceQueryManager choiceManager = _servicesMgr.CreateProxy<IChoiceQueryManager>(ExecutionIdentity.System))
            {
                return await choiceManager.QueryAsync(workspaceArtifactId, fieldId).ConfigureAwait(false);
            }
        }

        private async Task<int> ReadFieldIdByGuidAsync(int workspaceArtifactId, Guid fieldGuid)
        {
            using (IArtifactGuidManager guidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                return await guidManager.ReadSingleArtifactIdAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);
            }
        }
    }
}
