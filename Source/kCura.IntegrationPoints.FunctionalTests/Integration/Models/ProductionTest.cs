using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class ProductionTest : RdoTestBase
    {
        private const string PRODUCTION_NAME = "Production";

        public int SavedSearchId { get; set; }

        public ProductionTest() : base(PRODUCTION_NAME)
        {
        }

        public ProductionTest(int savedSearchId) : base(PRODUCTION_NAME)
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
