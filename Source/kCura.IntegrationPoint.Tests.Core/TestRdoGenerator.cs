using kCura.IntegrationPoints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class TestRdoGenerator
    {
        public static TRdo GetDefault<TRdo>(int artifactId)
            where TRdo : BaseRdo
        {
            TRdo rdo = (TRdo)Activator.CreateInstance(typeof(TRdo));
            IEnumerable<PropertyInfo> properties = rdo.GetType()
                .GetProperties()
                .Where(p => p.GetSetMethod() != null);
            foreach (var property in properties)
            {
                property.SetValue(rdo, null, null);
            }
            rdo.ArtifactId = artifactId;

            return rdo;
        }
    }
}
