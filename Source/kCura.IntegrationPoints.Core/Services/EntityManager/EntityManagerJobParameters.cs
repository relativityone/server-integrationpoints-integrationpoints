using System;
using System.Collections.Generic;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.EntityManager
{
    [Serializable]
    public class EntityManagerJobParameters
    {
        public IDictionary<string, string> EntityManagerMap { get; set; }

        public IEnumerable<FieldMap> EntityManagerFieldMap { get; set; }

        public bool ManagerFieldIdIsBinary { get; set; }

        public IEnumerable<FieldMap> ManagerFieldMap { get; set; }
    }
}
