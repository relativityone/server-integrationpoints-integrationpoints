using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Progress
{
    internal sealed class SyncJobProgress : IProgress<SyncJobState>
    {
        private readonly int _sourceWorkspaceArtifactId;
        private readonly int _syncConfigurationArtifactId;
        private readonly IProgressRepository _progressRepository;
        private readonly IProgressStateCounter _counter;
        private readonly IAPILog _logger;

        public SyncJobProgress(
            SyncJobParameters jobParameters,
            IProgressRepository progressRepository,
            IProgressStateCounter counter,
            IAPILog logger)
        {
            _sourceWorkspaceArtifactId = jobParameters.WorkspaceId;
            _syncConfigurationArtifactId = jobParameters.SyncConfigurationArtifactId;
            _progressRepository = progressRepository;
            _counter = counter;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Report(SyncJobState value)
        {
            ReportAsync(value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task ReportAsync(SyncJobState value)
        {
            try
            {
                _logger.LogVerbose($"Reporting {nameof(SyncJobState)}: {{value}}", value);

                IProgress progress = await _progressRepository.QueryAsync(
                    _sourceWorkspaceArtifactId,
                    _syncConfigurationArtifactId,
                    value.Id).ConfigureAwait(false);

                if (progress != null)
                {
                    await UpdateAsync(progress, value).ConfigureAwait(false);
                }
                else
                {
                    await CreateAsync(value).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while attempting to report Sync job progress: {progress}", value);
            }
        }

        private async Task CreateAsync(SyncJobState value)
        {
            int order = _counter.GetOrderForGroup(value.ParallelGroupId);

            IProgress progressObject = await _progressRepository.CreateAsync(
                _sourceWorkspaceArtifactId,
                _syncConfigurationArtifactId,
                value.Id,
                order,
                value.Status).ConfigureAwait(false);

            await progressObject.SetMessageAsync(value.Message).ConfigureAwait(false);
            await progressObject.SetExceptionAsync(value.Exception).ConfigureAwait(false);
        }

        private static async Task UpdateAsync(IProgress progress, SyncJobState value)
        {
            await progress.SetStatusAsync(value.Status).ConfigureAwait(false);
            await progress.SetMessageAsync(value.Message).ConfigureAwait(false);
            await progress.SetExceptionAsync(value.Exception).ConfigureAwait(false);
        }
    }
}
