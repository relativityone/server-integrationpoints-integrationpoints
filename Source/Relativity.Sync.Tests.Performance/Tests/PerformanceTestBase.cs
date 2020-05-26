using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;

namespace Relativity.Sync.Tests.Performance.Tests
{
	public class PerformanceTestBase : SystemTest
	{
		private readonly int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		private readonly ObjectTypeIdentifier _documentObjectTypeIdentifier = new ObjectTypeIdentifier()
		{
			ArtifactTypeID = (int)ArtifactType.Document
		};


		public ApiComponent Component { get; }
		public ARMHelper ARMHelper { get; }
		public AzureStorageHelper StorageHelper { get; }

		public WorkspaceRef TargetWorkspace { get; set; }

		public WorkspaceRef SourceWorkspace { get; set; }

		public FullSyncJobConfiguration Configuration { get; set; }

		public PerformanceTestBase()
		{
			RelativityFacade.Instance.RelyOn<ApiComponent>();

			Component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			StorageHelper = AzureStorageHelper.CreateFromTestConfig();

			ARMHelper = ARMHelper.CreateInstance();

			Configuration = new FullSyncJobConfiguration()
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				CreateSavedSearchForTagging = false,
				EmailNotificationRecipients = "",
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				FolderPathSourceFieldName = "Document Folder Path",
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				MoveExistingDocuments = false,
			};
		}

		/// <summary>
		///	Creates needed objects in Relativity
		/// </summary>
		/// <returns></returns>
		public async Task SetupConfigurationAsync(int? sourceWorkspaceId = null, int? targetWorkspaceId = null, string savedSearchName = "All Documents",
			IEnumerable<FieldMap> mapping = null, bool useRootWorkspaceFolder = true)
		{
			if (sourceWorkspaceId == null)
			{
				SourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			}
			else
			{
				SourceWorkspace = await Environment.GetWorkspaceAsync(sourceWorkspaceId.Value).ConfigureAwait(false);
				await Environment.CreateFieldsInWorkspaceAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);
			}

			if (targetWorkspaceId == null)
			{
				TargetWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			}
			else
			{
				TargetWorkspace = await Environment.GetWorkspaceAsync(targetWorkspaceId.Value).ConfigureAwait(false);
			}

			Configuration.TargetWorkspaceArtifactId = TargetWorkspace.ArtifactID;

			Configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).ConfigureAwait(false);


			Configuration.FieldsMapping =
				mapping ?? await GetIdentifierMappingAsync().ConfigureAwait(false);

			Configuration.JobHistoryId =
				await Rdos.CreateJobHistoryInstance(ServiceFactory, SourceWorkspace.ArtifactID)
					.ConfigureAwait(false);

			if (useRootWorkspaceFolder)
			{
				Configuration.DestinationFolderArtifactId =
					await Rdos.GetRootFolderInstance(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Generates mapping with fields
		/// </summary>
		/// <param name="numberOfMappedFields">Limits the number of mapped fields. 0 means maps all fields</param>
		/// <returns>Mapping with generated fields</returns>
		public async Task<IEnumerable<FieldMap>> GetMappingAndCreateFieldsInDestinationWorkspaceAsync(int? numberOfMappedFields)
		{
			List<RelativityObject> sourceFields = (await GetFieldsFromSourceWorkspaceAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false)).ToList();

			Regex wasGeneratedRegex = new Regex("^([0-9]+-)");

			sourceFields = sourceFields.Where(f => wasGeneratedRegex.IsMatch(f["Name"].Value.ToString())).ToList();

			if (numberOfMappedFields != null)
			{
				sourceFields = sourceFields.Take(numberOfMappedFields.Value).ToList();
			}

			sourceFields = sourceFields.ToList();

			IEnumerable<int> createdArtifactIds = await CreateFieldsInDesitnationWorkspaceAsync(sourceFields).ConfigureAwait(false);

			return sourceFields.Zip(createdArtifactIds, (sourceField, createdId) => new FieldMap
			{
				FieldMapType = FieldMapType.None,
				SourceField = new FieldEntry
				{
					DisplayName = sourceField["Name"].Value.ToString(),
					FieldIdentifier = sourceField.ArtifactID,
					IsIdentifier = false
				},
				DestinationField = new FieldEntry
				{
					DisplayName = sourceField["Name"].Value.ToString(),
					FieldIdentifier = createdId,
					IsIdentifier = false
				}
			});
		}

		private Task<IEnumerable<int>> CreateFieldsInDesitnationWorkspaceAsync(IEnumerable<RelativityObject> sourceFields)
		{
			return CreateFieldsAsync(TargetWorkspace.ArtifactID, sourceFields);
		}

		private async Task<IEnumerable<RelativityObject>> GetFieldsFromSourceWorkspaceAsync(int sourceWorkspaceArtifactId)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				List<RelativityObject> result = new List<RelativityObject>();

				QueryRequest query = PrepareGeneratedFieldsQueryRequest();
				int start = 0;
				QueryResult queryResult = null;

				do
				{
					const int batchSize = 100;
					queryResult = await objectManager
						.QueryAsync(sourceWorkspaceArtifactId, query, start, batchSize)
						.ConfigureAwait(false);

					result.AddRange(queryResult.Objects);
					start += queryResult.ResultCount;
				}
				while (result.Count < queryResult.TotalCount);

				return result;
			}
		}


		protected async Task<IEnumerable<FieldMap>> GetIdentifierMappingAsync()
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest query = PrepareIdentifierFieldsQueryRequest();
				QueryResult sourceQueryResult = await objectManager.QueryAsync(SourceWorkspace.ArtifactID, query, 0, 1).ConfigureAwait(false);
				QueryResult destinationQueryResult = await objectManager.QueryAsync(TargetWorkspace.ArtifactID, query, 0, 1).ConfigureAwait(false);

				return new FieldMap[]
				{
					new FieldMap
					{
						SourceField = new FieldEntry
						{
							DisplayName = sourceQueryResult.Objects.First()["Name"].Value.ToString(),
							FieldIdentifier =  sourceQueryResult.Objects.First().ArtifactID,
							IsIdentifier = true
						},
						DestinationField = new FieldEntry
						{
							DisplayName = destinationQueryResult.Objects.First()["Name"].Value.ToString(),
							FieldIdentifier =  destinationQueryResult.Objects.First().ArtifactID,
							IsIdentifier = true
						},
						FieldMapType = FieldMapType.Identifier
					}
				};

			}
		}

		protected  async Task<IEnumerable<FieldMap>> GetGetExtractedTextMappingAsync()
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest query = PrepareExtractedTextFieldsQueryRequest();
				QueryResult sourceQueryResult = await objectManager.QueryAsync(SourceWorkspace.ArtifactID, query, 0, 1).ConfigureAwait(false);
				QueryResult destinationQueryResult = await objectManager.QueryAsync(TargetWorkspace.ArtifactID, query, 0, 1).ConfigureAwait(false);

				return new FieldMap[]
				{
					new FieldMap
					{
						SourceField = new FieldEntry
						{
							DisplayName = sourceQueryResult.Objects.First()["Name"].Value.ToString(),
							FieldIdentifier =  sourceQueryResult.Objects.First().ArtifactID,
							IsIdentifier = false
						},
						DestinationField = new FieldEntry
						{
							DisplayName = destinationQueryResult.Objects.First()["Name"].Value.ToString(),
							FieldIdentifier =  destinationQueryResult.Objects.First().ArtifactID,
							IsIdentifier = false
						},
						FieldMapType = FieldMapType.None
					}
				};

			}
		}

		private QueryRequest PrepareIdentifierFieldsQueryRequest()
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} and 'Is Identifier' == true",
				Fields = new[] { new FieldRef { Name = "Name" }},
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}

		private QueryRequest PrepareExtractedTextFieldsQueryRequest()
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} and 'Name' == 'Extracted Text'",
				Fields = new[] { new FieldRef { Name = "Name" } },
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}

		private QueryRequest PrepareGeneratedFieldsQueryRequest()
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID}",
				Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Field type" }, },
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}
		
		public async Task<IEnumerable<int>> CreateFieldsAsync(int workspaceID, IEnumerable<RelativityObject> fields)
		{
			const string typeFieldName = "Field Type";
			
			var artifactIds = new List<int>();

			using (IFieldManager fieldManager = ServiceFactory.CreateProxy<IFieldManager>())
			{
				foreach (RelativityObject field in fields)
				{
					string fieldType = field[typeFieldName].Value.ToString();
					Debug.WriteLine($"Creating field in workspace. Field name: '{field.Name}'\t\tType: '{fieldType}'");

					int createdFieldArtifactId = 0;

					switch (fieldType)
					{
						case "Fixed-Length Text":
							createdFieldArtifactId = await fieldManager
								.CreateFixedLengthFieldAsync(workspaceID, CreateFixedLengthFieldRequest(field.Name))
								.ConfigureAwait(false);
							break;
						case "Long Text":
							createdFieldArtifactId = await fieldManager
								.CreateLongTextFieldAsync(workspaceID, CreateLongTextFieldRequest(field.Name))
								.ConfigureAwait(false);
							break;
						case "Date":
							Formatting formatting = (await fieldManager
								.ReadAsync(workspaceID, field.ArtifactID).ConfigureAwait(false)).Formatting;
							createdFieldArtifactId = await fieldManager
								.CreateDateFieldAsync(workspaceID, CreateDateFieldRequest(field.Name, formatting))
								.ConfigureAwait(false);
							break;
						case "Decimal":
							createdFieldArtifactId = await fieldManager
								.CreateDecimalFieldAsync(workspaceID, CreateDecimalFieldRequest(field.Name))
								.ConfigureAwait(false);
							break;
						case "Currency":
							createdFieldArtifactId = await fieldManager
								.CreateCurrencyFieldAsync(workspaceID, CreateCurrencyFieldRequest(field.Name))
								.ConfigureAwait(false);
							break;
						case "Whole Number":
							createdFieldArtifactId = await fieldManager
								.CreateWholeNumberFieldAsync(workspaceID, CreateWholeNumberFieldRequest(field.Name))
								.ConfigureAwait(false);
							break;
						case "Yes/No":
							createdFieldArtifactId = await fieldManager
								.CreateYesNoFieldAsync(workspaceID, CreateYesNoFieldRequest(field.Name))
								.ConfigureAwait(false);
							break;
					}

					artifactIds.Add(createdFieldArtifactId);
				}

				return artifactIds;
			}
		}

		private FixedLengthFieldRequest CreateFixedLengthFieldRequest(string fieldName)
		{
			const int length = 255;
			return new FixedLengthFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = fieldName,
				Length = length
			};
		}

		private LongTextFieldRequest CreateLongTextFieldRequest(string fieldName)
		{
			return new LongTextFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = fieldName,
				EnableDataGrid = false
			};
		}

		private DateFieldRequest CreateDateFieldRequest(string fieldName, Formatting formatting)
		{
			return new DateFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = fieldName,
				Formatting = formatting
			};
		}

		private DecimalFieldRequest CreateDecimalFieldRequest(string fieldName)
		{
			return new DecimalFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = fieldName
			};
		}

		private CurrencyFieldRequest CreateCurrencyFieldRequest(string fieldName)
		{
			return new CurrencyFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = fieldName
			};
		}

		private WholeNumberFieldRequest CreateWholeNumberFieldRequest(string fieldName)
		{
			return new WholeNumberFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = fieldName
			};
		}

		private YesNoFieldRequest CreateYesNoFieldRequest(string fieldName)
		{
			return new YesNoFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = fieldName
			};
		}
	}
}
