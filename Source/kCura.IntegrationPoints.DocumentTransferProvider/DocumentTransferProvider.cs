using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public class DestinationConfigurationModel
	{
		public int ArtifactId;
		public string ImportOverwriteMode;
		public int CaseArtifactId;
		public bool CustodianManagerFieldContainsLink;
		public bool ExtractedTextFieldContainsFilePath;
	}

	[Contracts.DataSourceProvider(Domain.Constants.RELATIVITY_PROVIDER_GUID)]
	public class DocumentTransferProvider : IInternalDataSourceProvider, IEmailBodyData
	{
		private readonly IDictionary<Type, object> _dependencies = new Dictionary<Type, object>();
		private readonly IAPILog _logger;

		public DocumentTransferProvider(IHelper helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<DocumentTransferProvider>();
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			LogRetrievingFields(options);
			var settings = JsonConvert.DeserializeObject<DocumentTransferSettings>(options);
			var fields = GetRelativityFields(settings.SourceWorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
			var fieldEntries = ParseFields(fields);

			return fieldEntries;
		}

		private ArtifactDTO[] GetRelativityFields(int workspaceId, int rdoTypeId)
		{
			IRepositoryFactory repositoryFactory = ResolveDependencies<IRepositoryFactory>();
			IFieldRepository fieldRepository = repositoryFactory.GetFieldRepository(workspaceId);

			ArtifactDTO[] fieldArtifacts = fieldRepository.RetrieveFields(
				rdoTypeId,
				new HashSet<string>(new[]
				{
					Constants.Fields.Name,
					Constants.Fields.Choices,
					Constants.Fields.ObjectTypeArtifactTypeId,
					Constants.Fields.FieldType,
					Constants.Fields.FieldTypeId,
					Constants.Fields.IsIdentifier,
					Constants.Fields.FieldTypeName,
				}));

			HashSet<string> ignoreFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
				Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
				DocumentFields.RelativityDestinationCase,
				DocumentFields.JobHistory
			};

			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportAPI().GetWorkspaceFields(workspaceId, rdoTypeId).Where(f => !ignoreFields.Contains(f.Name)).Select(x => x.ArtifactID));

			// Contains is 0(1) https://msdn.microsoft.com/en-us/library/kw5aaea4.aspx
			return fieldArtifacts.Where(x => mappableArtifactIds.Contains(x.ArtifactId)).ToArray();
		}

		private IEnumerable<FieldEntry> ParseFields(ArtifactDTO[] fieldArtifacts)
		{
			foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
			{
				var fieldName = string.Empty;
				var isIdentifierFieldValue = 0;

				foreach (var field in fieldArtifact.Fields)
				{
					if (field.Name == Constants.Fields.Name)
					{
						fieldName = field.Value as string;
					}
					else if (field.Name == Constants.Fields.IsIdentifier)
					{
						try
						{
							isIdentifierFieldValue = Convert.ToInt32(field.Value);
						}
						catch
						{
							LogReceivingParsedFieldsError(fieldArtifacts, new Exception("Invalid cast error."));
							// suppress error for invalid casts
						}
					}
				}

				bool isIdentifier = isIdentifierFieldValue > 0;
				if (isIdentifier)
				{
					fieldName += Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT;
				}

				yield return new FieldEntry()
				{
					DisplayName = fieldName,
					FieldIdentifier = fieldArtifact.ArtifactId.ToString(),
					IsIdentifier = isIdentifier,
					IsRequired = isIdentifier
				};
			}
		}
		
		private IImportAPI GetImportAPI()
		{
			const string username = "XxX_BearerTokenCredentials_XxX";
			var authToken = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;

			// TODO: we need to make IIntegrationPointsConfig a dependency or use a factory -- biedrzycki: Feb 16th, 2016
			IWebApiConfig config = new WebApiConfig();
			return new ExtendedImportAPI(username, authToken, config.GetWebApiUrl);
		}
		
		/// <summary>
		/// Gets all of the artifact ids that can be batched in reads
		/// </summary>
		/// <param name="identifier">The identifying field (Control Number)</param>
		/// <param name="options">The artifactId of the saved search in string format</param>
		/// <returns>An IDataReader containing all of the saved search's document artifact ids</returns>
		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			LogRetrievingBatchableIdsErrorWithDetails(options, identifier, new NotImplementedException());
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the RDO's who's artifact ids exist in the entryIds list
		/// (This method is called in batches of normally 1000 entryIds)
		/// </summary>
		/// <param name="fields">The fieldArtifacts the user mapped</param>
		/// <param name="entryIds">The artifact ids of the documents to copy (in string format)</param>
		/// <param name="options">The saved search artifact id (unused in this method)</param>
		/// <returns>An IDataReader that contains the Document RDO's for the entryIds</returns>
		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			LogRetrievingDataErrorWithDetails(options, entryIds, fields, new NotImplementedException());
			throw new NotImplementedException();
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			LogReceivingEmailBodyData(fields, options);
			var settings = GetSettings(options);

			var sourceWorkspace = GetWorkspace(settings.SourceWorkspaceArtifactId);

			var emailBody = new StringBuilder();
			if (sourceWorkspace != null)
			{
				emailBody.AppendLine(string.Empty);
				emailBody.Append(
					$"Source Workspace: {Utils.GetFormatForWorkspaceOrJobDisplay(sourceWorkspace.Name, sourceWorkspace.ArtifactId)}");
			}
			return emailBody.ToString();
		}

		protected virtual DocumentTransferSettings GetSettings(string options)
		{
			return JsonConvert.DeserializeObject<DocumentTransferSettings>(options);
		}

		protected virtual WorkspaceDTO GetWorkspace(int workspaceArtifactIds)
		{
			IRepositoryFactory repositoryFactory = ResolveDependencies<IRepositoryFactory>();
			IWorkspaceRepository workspaceRepository = repositoryFactory.GetWorkspaceRepository();
			return workspaceRepository.Retrieve(workspaceArtifactIds);
		}
		
		public void RegisterDependency<T>(T dependencies)
		{
			LogRegisterDependency();
			_dependencies.Add(typeof(T), dependencies);
		}
		
		private T ResolveDependencies<T>()
		{
			return (T)_dependencies[typeof(T)];
		}

		#region Logging

		private void LogRegisterDependency()
		{
			_logger.LogInformation("Attempting to register dependency in Document Transfer Provider.");
		}

		private void LogReceivingEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			var fieldIdentifiers = fields.Select(x => x.FieldIdentifier).ToList();
			_logger.LogInformation("Attempting to get email doby data in Document Transfer Provider (with {Options}) and fields {fields}.", options, string.Join(",", fieldIdentifiers));
		}

		private void LogRetrievingFields(string options)
		{
			_logger.LogInformation("Attempting to get fields in Document Transfer Provider (with {Options}).", options);
		}

		private void LogReceivingParsedFieldsError(IEnumerable<ArtifactDTO> fieldArtifacts, Exception exception)
		{
			var items = fieldArtifacts.Select(x => x.TextIdentifier).ToList();
			_logger.LogError(exception, "Failed to retrieve parsed fields in Document Transfer Provider (with {fieldArtifacts}).", string.Join(",", items));
		}

		private void LogRetrievingBatchableIdsErrorWithDetails(string options, FieldEntry identifier, Exception ex)
		{
			_logger.LogError(ex, "Failed to retrieve batchable ids in Document Transfer Provider (with {Options}) for field {FieldIdentifier}.", 
				options, identifier.FieldIdentifier);
		}

		private void LogRetrievingDataErrorWithDetails(string options, IEnumerable<string> entryIds, IEnumerable<FieldEntry> fields, Exception ex)
		{
			var fieldIdentifiers = fields.Select(x => x.FieldIdentifier).ToList();
			_logger.LogError(ex, "Failed to retrieve data in Document Transfer Provider (with {Options}) for ids {Ids} and fields {fields}.", options,
				string.Join(",", entryIds), string.Join(",", fieldIdentifiers));
		}

		#endregion
	}
}