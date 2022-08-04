using System.Collections.Generic;
using kCura.IntegrationPoints.Data.DTO;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Converters
{
    public static class RelativityObjectSlimExtensions
    {
        public static RelativityObjectSlimDto ToRelativityObjectSlimDto(
            this RelativityObjectSlim relativityObjectSlim,
            string[] fieldNames)
        {
            var columnValues = new Dictionary<string, object>();
            for (int i = 0; i < fieldNames.Length; i++)
            {
                columnValues[fieldNames[i]] = relativityObjectSlim.Values[i];
            }

            var relativityObjectSlimDto = new RelativityObjectSlimDto(relativityObjectSlim.ArtifactID, columnValues);
            return relativityObjectSlimDto;
        }
    }
}
