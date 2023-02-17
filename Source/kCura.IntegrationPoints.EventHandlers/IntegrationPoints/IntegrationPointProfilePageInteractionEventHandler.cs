using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Guid("41C98534-E559-4935-8BA0-6A0C8FE1AA90")]
    [Description("This is a details page interaction event handler for Integration Point Profile RDO")]
    public class IntegrationPointProfilePageInteractionEventHandler : PageInteractionEventHandler
    {
        private ICommonScriptsFactory _commonScriptsFactory;

        public override ICommonScriptsFactory CommonScriptsFactory
        {
            get
            {
                var apiControllerName = Constants.IntegrationPointProfiles.API_CONTROLLER_NAME;
                return _commonScriptsFactory ??
                        (_commonScriptsFactory =
                            new CommonScriptsFactory(Helper, new IntegrationPointProfileFieldGuidsConstants(), new IntegrationPointProfileFieldsConstants(), apiControllerName));
            }
            set { _commonScriptsFactory = value; }
        }

        public override string[] ScriptFileNames => new[] { "integration-point-profile-event-handler.js" };
    }
}
