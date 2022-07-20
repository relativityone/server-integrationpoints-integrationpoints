using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADF
{
	internal interface IADLSMigrationStatus
	{
		Task<bool> IsTenantFullyMigratedAsync();
	}
}