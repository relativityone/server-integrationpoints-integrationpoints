using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADF
{
	internal interface IMigrationStatus
	{
		Task<bool> IsTenantFullyMigratedAsync();
	}
}