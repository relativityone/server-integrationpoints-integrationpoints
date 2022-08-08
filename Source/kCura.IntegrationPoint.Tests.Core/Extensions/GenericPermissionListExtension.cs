using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
    public static class GenericPermissionListExtension
    {
        public static GenericPermission FindPermission(this List<GenericPermission> instance, string name)
        {
            GenericPermission genericPermission = instance.First(node => node.Name.Equals(name));
            return genericPermission;
        }
    }
}