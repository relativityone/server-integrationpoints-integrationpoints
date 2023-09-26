using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class FolderFake : RdoFakeBase
    {
        public string Name { get; set; }

        public FolderFake() : base("Folder")
        {
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            throw new System.NotImplementedException();
        }
    }
}
