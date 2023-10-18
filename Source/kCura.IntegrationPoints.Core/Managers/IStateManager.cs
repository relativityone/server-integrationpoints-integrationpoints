using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Choice;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IStateManager
    {
        /// <summary>
        ///     Returns a set of booleans that convey the button state of the console buttons for all providers.
        /// </summary>
        /// <param name="exportType">Type of export</param>
        /// <param name="providerType">Type of Integration Point Provider</param>
        /// <param name="hasErrorViewPermissions">If the user can view Job History and Job History Error objects</param>
        /// <param name="hasProfileAddPermission">If the user can add Integration Point Profile objects</param>
        /// <param name="calculationInProgress">If the job statistics calculation is in progress</param>
        /// <param name="lastJobHistoryStatus">The status of latest job history</param>
        /// <param name="isIApiV2CustomProviderWorkflow">Enables stop button when running Custom provider workflow with IAPI 2.0</param>
        /// <returns>
        ///     A collection of booleans which explain the button state of the buttons on the console for the Relativity
        ///     Provider.
        /// </returns>
        ButtonStateDTO GetButtonState(
            ExportType exportType,
            ProviderType providerType,
            bool hasErrorViewPermissions,
            bool hasProfileAddPermission,
            bool calculationInProgress,
            ChoiceRef lastJobHistoryStatus,
            bool isIApiV2CustomProviderWorkflow);
    }
}
