using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class PreLoadEventHandler : PreLoadEventHandlerBase
	{
		public override Response Execute()
		{
			if (base.PageMode == EventHandler.Helper.PageMode.Edit)
			{
				//TODO: redirect to Edit custom Page
			}
			return new Response
			{
				Success = true
			};
		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}
	}
}
