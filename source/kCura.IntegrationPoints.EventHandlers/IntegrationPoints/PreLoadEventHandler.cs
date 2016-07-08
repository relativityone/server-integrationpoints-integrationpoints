using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class PreLoadEventHandler : PreLoadEventHandlerBase
	{
		#region Fields

		private ExternalTabURLService _service;

		#endregion //Fields

		#region Properties

		public ExternalTabURLService Service => _service ?? (_service = new ExternalTabURLService());

		public override FieldCollection RequiredFields => new FieldCollection();

		#endregion Properties

		#region Methods

		public override Response Execute()
		{
			var response = new Response
			{
				Success = true,
				Message = ""
			};

			var scripts = new StringBuilder();

			if (PageMode == EventHandler.Helper.PageMode.View)
			{
				int sourceProvider = (int) ActiveArtifact.Fields[IntegrationPointFields.SourceProvider].Value.Value;
				// Integration Point Specific Error Handling 
				if (ServiceContext.RsapiService.SourceProviderLibrary.Read(Int32.Parse(sourceProvider.ToString())).Name ==
					Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
				{

					StringBuilder errorMessage = new StringBuilder("");
					StringBuilder folderPathInformation = new StringBuilder("");
					string destinationConfiguration =
						(string) ActiveArtifact.Fields[IntegrationPointFields.DestinationConfiguration].Value.Value;
					ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(destinationConfiguration);
					IntegrationPointDestinationConfiguration integrationPointDestinationConfiguration =
						JsonConvert.DeserializeObject<IntegrationPointDestinationConfiguration>(destinationConfiguration);
					if (importSettings.ImportOverwriteMode == ImportOverwriteModeEnum.AppendOnly &&
						integrationPointDestinationConfiguration.UseFolderPathInformation)
					{
						var sqlString =
							$"SELECT TextIdentifier FROM Artifact WHERE ArtifactID = {integrationPointDestinationConfiguration.FolderPathSourceField}";
						folderPathInformation.Append(
							ServiceContext.SqlContext.ExecuteSqlStatementAsDataTable(sqlString).Rows[0].ItemArray[0]);
					}
					string sourceConfiguration = ActiveArtifact.Fields[IntegrationPointFields.SourceConfiguration].Value.Value.ToString();

					IDictionary<string, object> settings = JsonConvert.DeserializeObject<ExpandoObject>(sourceConfiguration);

					Result<Workspace> sourceWorkspace;
					Result<Workspace> targetWorkspace;

					using (IRSAPIClient currentClient = GetRsapiClient(-1))
					{
						QueryResultSet<Workspace> workspaces = new GetWorkspacesQuery(currentClient).ExecuteQuery();
						targetWorkspace =
							workspaces.Results.FirstOrDefault(x => x.Artifact.ArtifactID == ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId)));
						sourceWorkspace =
							workspaces.Results.FirstOrDefault(x => x.Artifact.ArtifactID == ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId)));
					}

					if (targetWorkspace == null)
					{
						settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspaceArtifactId)] = 0;
					}
					else
					{
						settings[nameof(ExportUsingSavedSearchSettings.TargetWorkspace)] = targetWorkspace.Artifact.Name;
						if (targetWorkspace.Artifact.Name.Contains(";"))
						{
							errorMessage =
								errorMessage.Append(
									"Destination workspace name contains an invalid character. Please remove before continuing.</br>");
						}
					}

					settings[nameof(ExportUsingSavedSearchSettings.SourceWorkspace)] = sourceWorkspace.Artifact.Name;
					if (sourceWorkspace.Artifact.Name.Contains(";"))
					{
						errorMessage = errorMessage.Append(
							"Source workspace name contains an invalid character. Please remove before continuing.</br>");
					}
					Relativity.Client.Artifact savedSearch;
					using (IRSAPIClient client = GetRsapiClient(ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SourceWorkspaceArtifactId))))
					{
						QueryResult savedSearches = new GetSavedSearchesQuery(client).ExecuteQuery();
						savedSearch = savedSearches.QueryArtifacts.FirstOrDefault(x => x.ArtifactID == ParseValue<int>(settings, nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId)));
					}
					if (savedSearch == null)
					{
						settings[nameof(ExportUsingSavedSearchSettings.SavedSearchArtifactId)] = 0;
					}
					else
					{
						settings[nameof(ExportUsingSavedSearchSettings.SavedSearch)] = savedSearch.getFieldByName("Text Identifier").ToString();
					}
					using (TagBuilder relativityprovider = new TagBuilder("script"))
					{
						relativityprovider.Attributes.Add("type", "text/javascript");
						relativityprovider.InnerHtml =
							$" var IP = IP || {{}};$(function(){{IP.errorMessage = '{errorMessage}';IP.fieldName ='{folderPathInformation}';}});";
						scripts.Append(relativityprovider);
					}
					response.Message = scripts.ToString();
					ActiveArtifact.Fields[IntegrationPointFields.SourceConfiguration].Value.Value =
						JsonConvert.SerializeObject(settings);
				}
			}

			if (PageMode == EventHandler.Helper.PageMode.Edit)
			{
				var action = Constant.URL_FOR_INTEGRATIONPOINTS_EDIT;
				var id = ActiveArtifact.ArtifactID != 0 ? ActiveArtifact.ArtifactID.ToString() : string.Empty;
				var url = $@"{Constant.URL_FOR_WEB}/{Constant.URL_FOR_INTEGRATIONPOINTSCONTROLLER}/{action}/{id}?StandardsCompliance=true";
				var tabId =
					ServiceContext.SqlContext.GetArtifactIDByGuid(Guid.Parse(IntegrationPointTabGuids.IntegrationPoints));
				var location = Service.EncodeRelativityURL(url, Application.ArtifactID, tabId, false);

				using (var questionnaireBuilderScriptBlock = new TagBuilder("script"))
				{
					questionnaireBuilderScriptBlock.Attributes.Add("type", "text/javascript");
					questionnaireBuilderScriptBlock.InnerHtml = $@"$(function(){{window.location=""{location}"";}});";
					scripts.Append(questionnaireBuilderScriptBlock);
				}
				response.Message = scripts.ToString();
			}
			
			return response;
		}

		#endregion Methods

		protected virtual IRSAPIClient GetRsapiClient(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			return rsapiClient;
		}

		private T ParseValue<T>(IDictionary<string, object> settings, string parameterName)
		{
			if (!settings.ContainsKey(parameterName))
			{
				return default(T);
			}
			return (T)(Convert.ChangeType(settings[parameterName], typeof(T)));
		}
	}

	internal class IntegrationPointDestinationConfiguration
	{
		public bool UseFolderPathInformation;
		public int FolderPathSourceField;
	}
}
