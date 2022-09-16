using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADLS
{
    internal interface IAdlsMigrationStatus
    {
        Task<bool> IsTenantFullyMigratedAsync();
    }
}
