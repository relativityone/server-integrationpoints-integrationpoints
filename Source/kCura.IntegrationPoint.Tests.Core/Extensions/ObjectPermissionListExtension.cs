using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
    public static class ObjectPermissionListExtension
    {
        public static ObjectPermission FindPermission(this List<ObjectPermission> instance, string name)
        {
            ObjectPermission genericPermission = instance.First(node => node.Name.Equals(name));
            return genericPermission;
        }
    }
}
