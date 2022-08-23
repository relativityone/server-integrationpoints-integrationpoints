using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADLS
{
    internal interface IADLSMigrationStatus
    {
        Task<bool> IsTenantFullyMigratedAsync();
    }
}
