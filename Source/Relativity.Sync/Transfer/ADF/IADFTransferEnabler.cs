using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADF
{
	internal interface IADFTransferEnabler
	{
		Task<bool> ShouldUseADFTransferAsync();
	}
}