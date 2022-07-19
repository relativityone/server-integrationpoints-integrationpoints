using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADF
{
	internal interface IAdlsMigrationStatus
	{
		Task<bool> IsTenantFullyMigratedAsync();
	}
}