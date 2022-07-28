using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADF
{
	internal interface IADFTransferEnabler
	{
        bool IsAdfTransferEnabled { get; }
    }
}