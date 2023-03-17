namespace kCura.IntegrationPoints.Common.RelativitySync
{
    public interface IRelativitySyncConstrainsChecker
    {
        /// <summary>
        /// Checks if Integration Point configuration allows running using Sync DLL flow
        /// </summary>
        /// <returns></returns>
        bool ShouldUseRelativitySync(int integrationPointId);
    }
}
