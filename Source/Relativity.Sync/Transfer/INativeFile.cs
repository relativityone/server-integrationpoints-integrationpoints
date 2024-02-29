using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface INativeFile : IFile
    {
        bool IsDuplicated { get; set; }

        Task ValidateMalwareAsync(IAntiMalwareHandler malwareHandler);
    }
}
