using System;
using System.Linq;
using System.Text;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using Relativity.API;
using Artifact = kCura.EventHandler.Artifact;

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
			
			
			int sourceProvider = (int)this.ActiveArtifact.Fields[IntegrationPointFields.SourceProvider].Value.Value;
			// Integration Point Specific Error Handling 
			if (base.PageMode == EventHandler.Helper.PageMode.View && base.ServiceContext.RsapiService.SourceProviderLibrary.Read(Int32.Parse(sourceProvider.ToString())).Name == DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_NAME)
			{
				
				StringBuilder errorMessage = new StringBuilder("");

				string sourceConfiguration = this.ActiveArtifact.Fields[IntegrationPointFields.SourceConfiguration].Value.Value.ToString();
				ExportUsingSavedSearchSettings settings =
					JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(sourceConfiguration);
				Result<Workspace> sourceWorkspace;
				Result<Workspace> targetWorkspace;
				using (IRSAPIClient currentClient = GetRSAPIClient(-1))
				{
					QueryResultSet<Workspace> workspaces = new GetWorkspacesQuery(currentClient).ExecuteQuery();
					targetWorkspace =
						workspaces.Results.FirstOrDefault(x => x.Artifact.ArtifactID == settings.TargetWorkspaceArtifactId);
					sourceWorkspace =
						workspaces.Results.FirstOrDefault(x => x.Artifact.ArtifactID == settings.SourceWorkspaceArtifactId);
				}


				if (targetWorkspace == null)
				{
					errorMessage = errorMessage.Append("You do not have permissions to import to the Destination workspace. Please contact your system administrator.</br>");
					settings.TargetWorkspaceArtifactId = 0;
				}
				else
				{
					settings.TargetWorkspace = targetWorkspace.Artifact.Name;
					if (targetWorkspace.Artifact.Name.Contains(";"))
					{
						errorMessage = errorMessage.Append("Destination workspace name contains an invalid character. Please remove before continuing.</br>");
					}
				}

				settings.SourceWorkspace = sourceWorkspace.Artifact.Name;
				if (sourceWorkspace.Artifact.Name.Contains(";"))
				{
					errorMessage = errorMessage.Append(
								   "Source workspace name contains an invalid character. Please remove before continuing.</br>");
				}
				Relativity.Client.Artifact savedSearch;
				using (IRSAPIClient client = GetRSAPIClient(settings.SourceWorkspaceArtifactId))
				{
					QueryResult savedSearches = new GetSavedSearchesQuery(client).ExecuteQuery();
					savedSearch = savedSearches.QueryArtifacts.FirstOrDefault(x => x.ArtifactID == settings.SavedSearchArtifactId);
				}
				if (savedSearch == null)
				{
					// user does not have any access to the save search
					errorMessage = errorMessage.Append("You do not have permissions to the source saved search. Please contact your system administrator.");
					settings.SavedSearchArtifactId = 0;
				}
				else
				{
					settings.SavedSearch = savedSearch.getFieldByName("Text Identifier").ToString();
				}
				using (TagBuilder Relativityprovider = new TagBuilder("script"))
				{
					Relativityprovider.Attributes.Add("type", "text/javascript");
					Relativityprovider.InnerHtml = String.Format(@" var IP = IP || {{}};$(function(){{IP.errorMessage = '{0}';}});",
						errorMessage.ToString());
					scripts.Append(Relativityprovider);
				}
				response.Message = scripts.ToString();
				this.ActiveArtifact.Fields[IntegrationPointFields.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(settings);
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
				var tabID = ServiceContext.SqlContext.GetArtifactIDByGuid(Guid.Parse(Data.IntegrationPointTabGuids.IntegrationPoints));
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
