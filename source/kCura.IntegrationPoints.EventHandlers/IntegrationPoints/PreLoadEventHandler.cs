using System;
using System.Linq;
using System.Text;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Relativity.API;

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

			var sourceProvider = this.ActiveArtifact.Fields["Source Provider"].Value.Value;
			// Integration Point Specific Error Handling 
			if (base.PageMode == EventHandler.Helper.PageMode.View && base.ServiceContext.RsapiService.SourceProviderLibrary.Read(Int32.Parse(sourceProvider.ToString())).Name == "Relativity")
			{
				string errorMessage = "";

				string sourceConfiguration = this.ActiveArtifact.Fields["Source Configuration"].Value.Value.ToString();
				ExportUsingSavedSearchSettings settings =
					JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(sourceConfiguration);
				var currentClient = GetRSAPIClient(-1);
				var workspaces = new GetWorkspacesQuery(currentClient).ExecuteQuery();
				var targetWorkspace =
					workspaces.Results.FirstOrDefault(x => x.Artifact.ArtifactID == settings.TargetWorkspaceArtifactId);
				var sourceWorkspace =
					workspaces.Results.FirstOrDefault(x => x.Artifact.ArtifactID == settings.SourceWorkspaceArtifactId);

				if (targetWorkspace == null)
				{
					errorMessage = errorMessage +
					               "You do not have permissions to import to the Destination workspace. Please contact your system administrator.</br>";
					settings.TargetWorkspaceArtifactId = 0;
				}
				else
				{
					settings.TargetWorkspace = targetWorkspace.Artifact.Name;
					if (targetWorkspace.Artifact.Name.Contains(";"))
					{
						errorMessage = errorMessage +
						               "Destination workspace name contains an invalid character. Please remove before continuing.</br>";
					}
				}

				settings.SourceWorkspace = sourceWorkspace.Artifact.Name;
				if (sourceWorkspace.Artifact.Name.Contains(";"))
				{
					errorMessage = errorMessage +
					               "Source workspace name contains an invalid character. Please remove before continuing.</br>";
				}
				var client = GetRSAPIClient(settings.SourceWorkspaceArtifactId);
				var savedSearchs =
					new GetSavedSearchesQuery(client).ExecuteQuery();
				var savedSearch = savedSearchs .QueryArtifacts.FirstOrDefault(x => x.ArtifactID == settings.SavedSearchArtifactId);
				if (savedSearch  == null)
				{
					// user does not have any access to the save search
					errorMessage = errorMessage +
					               "You do not have permissions to the source saved search. Please contact your system administrator.";
					settings.SavedSearchArtifactId = 0;
				}
				else
				{
					settings.SavedSearch = savedSearch.getFieldByName("Text Identifier").ToString();
				}
				using (var Relativityprovider = new TagBuilder("script"))
				{
					Relativityprovider.Attributes.Add("type", "text/javascript");
					Relativityprovider.InnerHtml = String.Format(@" var IP = IP || {{}};$(function(){{IP.errorMessage = '{0}';}});",
						errorMessage);
					scripts.Append(Relativityprovider.ToString());
				}
				response.Message = scripts.ToString();
				this.ActiveArtifact.Fields["Source Configuration"].Value.Value = JsonConvert.SerializeObject(settings);
			}


			string action = string.Empty;
			if (base.PageMode == EventHandler.Helper.PageMode.Edit)
			{
				action = Constant.URL_FOR_INTEGRATIONPOINTS_EDIT;
				var id = ActiveArtifact.ArtifactID != 0 ? ActiveArtifact.ArtifactID.ToString() : string.Empty;
				var url = String.Format(@"{0}/{1}/{2}/{3}?StandardsCompliance=true", Constant.URL_FOR_WEB,
					Constant.URL_FOR_INTEGRATIONPOINTSCONTROLLER,
					action,
					id);
				var tabID =
					ServiceContext.SqlContext.GetArtifactIDByGuid(Guid.Parse(Data.IntegrationPointTabGuids.IntegrationPoints));
				location = Service.EncodeRelativityURL(url, this.Application.ArtifactID, tabID, false);

				using (var questionnaireBuilderScriptBlock = new TagBuilder("script"))
				{
					questionnaireBuilderScriptBlock.Attributes.Add("type", "text/javascript");
					questionnaireBuilderScriptBlock.InnerHtml = String.Format(@"$(function(){{window.location=""{0}"";}});", location);
					scripts.Append(questionnaireBuilderScriptBlock.ToString());
				}
				response.Message = scripts.ToString();
			}


			return response;
		}

		protected virtual IRSAPIClient GetRSAPIClient(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			return rsapiClient;
		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}
	}
}
