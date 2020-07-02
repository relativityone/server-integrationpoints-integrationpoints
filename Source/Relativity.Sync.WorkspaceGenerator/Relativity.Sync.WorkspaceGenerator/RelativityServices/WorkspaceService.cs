using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Folder;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.WorkspaceGenerator.Fields;
using FieldType = Relativity.Services.FieldType;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
	public class WorkspaceService : IWorkspaceService
	{
		private readonly IServiceFactory _serviceFactory;
		private readonly Random _random = new Random();
		
		private readonly ObjectTypeIdentifier _documentObjectTypeIdentifier = new ObjectTypeIdentifier()
		{
			ArtifactTypeID = (int)ArtifactType.Document
		};

		public WorkspaceService(IServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<IEnumerable<WorkspaceRef>> GetAllActiveAsync()
		{
			using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				return await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
			}
		}

		public async Task<WorkspaceRef> GetWorkspaceAsync(int workspaceID)
		{
			Console.WriteLine($"Reading workspace {workspaceID}");
			IEnumerable<WorkspaceRef> activeWorkspaces = await GetAllActiveAsync().ConfigureAwait(false);
			return activeWorkspaces.Single(x => x.ArtifactID == workspaceID);
		}

		public async Task<WorkspaceRef> CreateWorkspaceAsync(string name, string templateWorkspaceName)
		{
			using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				Console.WriteLine($"Gathering all active workspaces");
				IEnumerable<WorkspaceRef> activeWorkspaces = await GetAllActiveAsync().ConfigureAwait(false);
				WorkspaceRef template = activeWorkspaces.FirstOrDefault(x => x.Name == templateWorkspaceName);

				if (template == null)
				{
					throw new Exception($"Cannot find workspace name: '{templateWorkspaceName}'");
				}

				Console.WriteLine($"Template workspace Artifact ID: {template.ArtifactID}");

				Console.WriteLine($"Creating new workspace: '{name}'");
				WorkspaceRef workspaceRef = await workspaceManager.CreateWorkspaceAsync(new WorkspaceSetttings()
				{
					Name = name,
					TemplateArtifactId = template.ArtifactID,
					EnableDataGrid = true
				}).ConfigureAwait(false);

				return workspaceRef;
			}
		}

		public async Task<int> GetRootFolderArtifactIDAsync(int workspaceID)
		{
			using (IFolderManager folderManager = _serviceFactory.CreateProxy<IFolderManager>())
			{
				Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceID).ConfigureAwait(false);
				return rootFolder.ArtifactID;
			}
		}

		public async Task EnableExtractedTextFieldForDataGridAsync(int workspaceID)
		{
			int _fieldArtifactId;
			string fieldName = Consts.ExtractedTextFieldName;

			using (IObjectManager objectManager = _serviceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest fieldRequest = CreateObjectManagerArtifactIdQueryRequest(fieldName);
				QueryResult fieldQueryResult= await objectManager.QueryAsync(workspaceID,fieldRequest,0,1).ConfigureAwait(false);
				_fieldArtifactId = fieldQueryResult.Objects.FirstOrDefault().ArtifactID;
			}

			using (IFieldManager fieldManager = _serviceFactory.CreateProxy<IFieldManager>())
			{
				var longTextFieldRequest = new LongTextFieldRequest()
				{
					ObjectType = new ObjectTypeIdentifier()
					{
						ArtifactTypeID = (int) ArtifactType.Document
					},
					Name = $"{fieldName}",
					EnableDataGrid = true,
					IncludeInTextIndex = false,
					FilterType = FilterType.None,
					AvailableInViewer = true,
					HasUnicode = true
				};
				await fieldManager.UpdateLongTextFieldAsync(workspaceID, _fieldArtifactId, longTextFieldRequest).ConfigureAwait(false);
			}
		}
		
		public async Task CreateFieldsAsync(int workspaceID, IEnumerable<CustomField> fields)
		{
			using (IFieldManager fieldManager = _serviceFactory.CreateProxy<IFieldManager>())
			{
				foreach (CustomField field in fields)
				{
					Console.WriteLine($"Creating field in workspace. Field name: '{field.Name}'\t\tType: '{field.Type}'");

					switch (field.Type)
					{
						case FieldType.FixedLengthText:
							await fieldManager
								.CreateFixedLengthFieldAsync(workspaceID, CreateFixedLengthFieldRequest(field))
								.ConfigureAwait(false);
							break;
						case FieldType.LongText:
							await fieldManager
								.CreateLongTextFieldAsync(workspaceID, CreateLongTextFieldRequest(field))
								.ConfigureAwait(false);
							break;
						case FieldType.Date:
							await fieldManager
								.CreateDateFieldAsync(workspaceID, CreateDateFieldRequest(field))
								.ConfigureAwait(false);
							break;
						case FieldType.Decimal:
							await fieldManager
								.CreateDecimalFieldAsync(workspaceID, CreateDecimalFieldRequest(field))
								.ConfigureAwait(false);
							break;
						case FieldType.Currency:
							await fieldManager
								.CreateCurrencyFieldAsync(workspaceID, CreateCurrencyFieldRequest(field))
								.ConfigureAwait(false);
							break;
						case FieldType.WholeNumber:
							await fieldManager
								.CreateWholeNumberFieldAsync(workspaceID, CreateWholeNumberFieldRequest(field))
								.ConfigureAwait(false);
							break;
						case FieldType.YesNo:
							await fieldManager
								.CreateYesNoFieldAsync(workspaceID, CreateYesNoFieldRequest(field))
								.ConfigureAwait(false);
							break;
						default:
							throw new Exception($"Field type is not supported by this tool: {field.Type}");
					}
				}
			}
		}

		private DateFieldRequest CreateDateFieldRequest(CustomField field)
		{
			Formatting formatting = _random.Next(0, int.MaxValue) % 2 == 0 ? Formatting.Date : Formatting.DateTime;

			return new DateFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = field.Name,
				Formatting = formatting
			};
		}

		private FixedLengthFieldRequest CreateFixedLengthFieldRequest(CustomField field)
		{
			return new FixedLengthFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = field.Name,
				Length = 255
			};
		}

		private LongTextFieldRequest CreateLongTextFieldRequest(CustomField field)
		{
			return new LongTextFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = field.Name,
				EnableDataGrid = false
			};
		}

		private DecimalFieldRequest CreateDecimalFieldRequest(CustomField field)
		{
			return new DecimalFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = field.Name
			};
		}

		private CurrencyFieldRequest CreateCurrencyFieldRequest(CustomField field)
		{
			return new CurrencyFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = field.Name
			};
		}

		private WholeNumberFieldRequest CreateWholeNumberFieldRequest(CustomField field)
		{
			return new WholeNumberFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = field.Name
			};
		}

		private YesNoFieldRequest CreateYesNoFieldRequest(CustomField field)
		{
			return new YesNoFieldRequest()
			{
				ObjectType = _documentObjectTypeIdentifier,
				Name = field.Name
			};
		}

		private QueryRequest CreateObjectManagerArtifactIdQueryRequest(string fieldName)
		{
			QueryRequest artifactIdRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Condition = $"'Object Type Artifact Type ID' == 10 AND 'Name' == '{fieldName}'",
				Fields = new[]
				{
					new FieldRef {Name = "ArtifactID"}
				}
			};
			return artifactIdRequest;
		}
	}
}