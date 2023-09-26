using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class ProductionFake : RdoFakeBase
    {
        private const string PRODUCTION_NAME = "Production";

        public int SavedSearchId { get; set; }

        public ProductionFake() : base(PRODUCTION_NAME)
        {
        }

        public ProductionFake(int savedSearchId) : base(PRODUCTION_NAME)
        {
            SavedSearchId = savedSearchId;
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            RelativityObject relativityObject = new RelativityObject
            {
                ArtifactID = ArtifactId,
            };

            return relativityObject;
        }
    }
}
