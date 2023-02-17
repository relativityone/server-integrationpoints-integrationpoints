using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
    public interface IConsumeScratchTableBatchStatus : IBatchStatus
    {
        IScratchTableRepository ScratchTableRepository { get; }
    }
}
