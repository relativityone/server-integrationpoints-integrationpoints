using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Guid("D903B3C5-DEA4-483C-A5A7-55089A096CE4")]
    [Description("This is a details page interaction event handler for Integration Point RDO")]
    public class IntegrationPointPageInteractionEventHandler : PageInteractionEventHandler
    {
        private ICommonScriptsFactory _commonScriptsFactory;

        public override ICommonScriptsFactory CommonScriptsFactory
        {
            get
            {
                var apiControllerName = Constants.IntegrationPoints.API_CONTROLLER_NAME;
                return _commonScriptsFactory ??
                        (_commonScriptsFactory = new CommonScriptsFactory(Helper, new IntegrationPointFieldGuidsConstants(), new IntegrationPointFieldsConstants(), apiControllerName));
            }
            set { _commonScriptsFactory = value; }
        }

        public override string[] ScriptFileNames => new[] { "integration-point-event-handler.js" };
    }
}