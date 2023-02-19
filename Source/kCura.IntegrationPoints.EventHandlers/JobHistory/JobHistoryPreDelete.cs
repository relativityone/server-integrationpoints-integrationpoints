using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.EventHandlers.JobHistory
{
    [kCura.EventHandler.CustomAttributes.Description("A description of the event handler.")]
    [System.Runtime.InteropServices.Guid("4ee07342-b4a0-45a3-a6a3-b56cc739feb7")]
    public class JobHistoryPreDelete : kCura.EventHandler.PreMassDeleteEventHandler
    {
        public override void Commit()
        {}

        public override EventHandler.Response Execute()
        {
            return null;
        }

        public override EventHandler.FieldCollection RequiredFields
        {
            get { return new EventHandler.FieldCollection(); }
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
