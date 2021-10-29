using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity;

namespace Relativity.IntegrationPoints.Tests.Integration.Utils
{
    public abstract class PermissionPermutator : IEnumerable<PermissionSetup[]>
    {
        protected abstract IEnumerable<PermissionSetup> NeededPermissions { get; }

        public IEnumerator<PermissionSetup[]> GetEnumerator()
        {
            foreach (var p in GenerateSinglePermissionMissing())
            {
                yield return p;
            }
            
            yield return AllPermissionsNotGranted();
        }

        private IEnumerable<PermissionSetup[]> GenerateSinglePermissionMissing()
        {
            for (int i = 0; i < NeededPermissions.Length(); i++)
            {
                PermissionSetup[] result = NeededPermissions.ToArray();
                result[i].Granted = false;
                yield return result;
            }
        }

        private PermissionSetup[] AllPermissionsNotGranted()
        {
            return NeededPermissions.Select(p =>
            {
                p.Granted = false;
                return p;
            }).ToArray();
        }

        public PermissionSetup[] AllPermissionsGranted()
        {
            return NeededPermissions.Select(p =>
            {
                p.Granted = true;
                return p;
            }).ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}