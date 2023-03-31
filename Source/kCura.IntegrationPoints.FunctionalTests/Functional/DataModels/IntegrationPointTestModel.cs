using System;
using kCura.Config;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Functional.DataModels
{
    public class IntegrationPointTestModel
    {
        public int ArtifactId { get; set; }

        public bool HasErrors { get; set; }

        public DateTime? LastRuntimeUTC { get; set; }

        public static IntegrationPointTestModel ConvertFrom(RelativityObject obj)
        {
            if (obj is null)
            {
                return null;
            }

            return new IntegrationPointTestModel
            {
                ArtifactId = obj.ArtifactID,
                HasErrors = (bool)obj[IntegrationPointFieldGuids.HasErrorsGuid].Value,
                LastRuntimeUTC = (DateTime?)obj[IntegrationPointFieldGuids.LastRuntimeUTCGuid].Value,
            };
        }
    }
}
