using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Guid("54E65983-C59F-42CA-89CC-9AC30F447619")]
    [Description("This is a details pre load event handler for Integration Point RDO")]
    public class IntegrationPointPreLoadEventHandler : PreLoadEventHandler
    {
        private IIntegrationPointViewPreLoad _integrationPointViewPreLoad;

        public override IIntegrationPointViewPreLoad IntegrationPointViewPreLoad
        {
            get
            {
                return _integrationPointViewPreLoad ??
                        (_integrationPointViewPreLoad = IntegrationPointViewPreLoadFactory.Create(Helper, new IntegrationPointFieldsConstants()));
            }
            set { _integrationPointViewPreLoad = value; }
        }
    }
}
