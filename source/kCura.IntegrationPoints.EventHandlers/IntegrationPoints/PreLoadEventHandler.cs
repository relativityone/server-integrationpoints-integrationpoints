using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class PreLoadEventHandler : PreLoadEventHandlerBase
	{
		private ExternalTabURLService _service;
		public ExternalTabURLService Service
		{
			get { return _service ?? (_service = new ExternalTabURLService()); }
		}
		public override Response Execute()
		{
			var response = new Response
			{
				Success = true,
				Message = ""
			};

			var scripts = new StringBuilder();
			var location = "";
			string action = string.Empty;
			if (base.PageMode == EventHandler.Helper.PageMode.Edit)
			{
				action = Constant.URL_FOR_INTEGRATIONPOINTS_EDIT;
			}
			if (base.PageMode == EventHandler.Helper.PageMode.View)
			{
				action = Constant.URL_FOR_INTEGRATIONPOINTS_VIEW;
			}

			var id = ActiveArtifact.ArtifactID == 0 ? ActiveArtifact.ArtifactID.ToString() : string.Empty;
			var url = String.Format(@"{0}/{1}/{2}/{3}?StandardsCompliance=true", Constant.URL_FOR_WEB,
										Constant.URL_FOR_INTEGRATIONPOINTSCONTROLLER,
										action,
										id);
			var tabID = ServiceContext.SqlContext.GetArtifactIDByGuid(Guid.Parse(Data.IntegrationPointTabGuids.IntegrationPoints));
			location = Service.EncodeRelativityURL(url, this.Application.ArtifactID, tabID, false);

			using (var questionnaireBuilderScriptBlock = new TagBuilder("script"))
			{
				questionnaireBuilderScriptBlock.Attributes.Add("type", "text/javascript");
				questionnaireBuilderScriptBlock.InnerHtml = String.Format(@"$(function(){{window.location=""{0}"";}});", location);
				scripts.Append(questionnaireBuilderScriptBlock.ToString());
			}
			response.Message = scripts.ToString();
			return response;
		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}
	}
}
