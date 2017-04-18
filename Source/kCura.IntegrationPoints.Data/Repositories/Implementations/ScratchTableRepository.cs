using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ScratchTableRepository : IScratchTableRepository
	{
		private readonly IDBContext _caseContext;
		private readonly IDocumentRepository _documentRepository;
		private readonly IFieldQueryRepository _fieldQueryRepository;
		private readonly IResourceDbProvider _resourceDbProvider;
		private readonly string _tablePrefix;
		private readonly string _tableSuffix;
		private readonly int _workspaceId;
		private IDataReader _reader;
		private string _tempTableName;
		private string _docIdentifierFieldName;
		private int _count;

		public ScratchTableRepository(IHelper helper, IDocumentRepository documentRepository,
			IFieldQueryRepository fieldQueryRepository, IResourceDbProvider resourceDbProvider, string tablePrefix, string tableSuffix, int workspaceId) :
				this(helper.GetDBContext(workspaceId), documentRepository, fieldQueryRepository, resourceDbProvider,
					tablePrefix, tableSuffix, workspaceId)
		{

		}

		private ScratchTableRepository(IDBContext caseContext, IDocumentRepository documentRepository, IFieldQueryRepository fieldQueryRepository, IResourceDbProvider resourceDbProvider,
			string tablePrefix, string tableSuffix, int workspaceId)
		{
			_caseContext = caseContext;
			_documentRepository = documentRepository;
			_fieldQueryRepository = fieldQueryRepository;
			_resourceDbProvider = resourceDbProvider;
			_tablePrefix = tablePrefix;
			_tableSuffix = tableSuffix;
			_workspaceId = workspaceId;
			IgnoreErrorDocuments = false;
		}

		public bool IgnoreErrorDocuments { get; set; }

		public int Count
		{
			get
			{
				return _count;
			}
		}

		public void RemoveErrorDocuments(ICollection<string> docIdentifiers)
		{
			_count -= docIdentifiers.Count;

			ICollection<int> docIds = GetErroredDocumentId(docIdentifiers);

			if (docIds.Count == 0)
			{
				return;
			}
			string documentList = "(" + String.Join(",", docIds) + ")";

			string fullTableName = GetTempTableName();
			string sql = String.Format(@"DELETE FROM {2}.[{0}] WHERE [ArtifactID] in {1}", fullTableName, documentList, GetResourceDBPrepend());

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public IDataReader GetDocumentIdsDataReaderFromTable()
		{
			if (_reader == null)
			{
				string fullTableName = GetTempTableName();

				var sql = String.Format(@"IF EXISTS (SELECT * FROM {1}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}') SELECT [ArtifactID] FROM {2}.[{0}]", fullTableName, GetSchemalessResourceDataBasePrepend(), GetResourceDBPrepend());

				_reader = _caseContext.ExecuteSQLStatementAsReader(sql);
			}
			return _reader;
		}

		public void Dispose()
		{
			try
			{
				if (_reader != null)
				{
					_reader.Close();
					_reader = null;
				}
				DeleteTable();
			}
			catch (Exception)
			{
				// trying to delete temp tables early, don't have worry about failing
			}
		}

		public void AddArtifactIdsIntoTempTable(ICollection<int> artifactIds)
		{
			if (!artifactIds.IsNullOrEmpty())
			{
				_count += artifactIds.Count;

				string fullTableName = GetTempTableName();

				ConnectionData connectionData = ConnectionData.GetConnectionDataWithCurrentCredentials(_caseContext.ServerName, GetSchemalessResourceDataBasePrepend());
				string connectionString = $"data source={connectionData.Server};initial catalog={connectionData.Database};persist security info=False;user id={connectionData.Username};password={connectionData.Password}; workstation id=localhost;packet size=4096;connect timeout=30;";
				Context context = new Context(connectionString);

				string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM {1}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN
												CREATE TABLE {2}.[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END", fullTableName, GetSchemalessResourceDataBasePrepend(), GetResourceDBPrepend());
				_caseContext.ExecuteNonQuerySQLStatement(sql);

				using (DataTable artifactIdTable = new DataTable())
				{
					artifactIdTable.Columns.Add();
					foreach (int artifactId in artifactIds)
					{
						artifactIdTable.Rows.Add(artifactId);
					}

					SqlBulkCopyParameters bulkParameters = new SqlBulkCopyParameters
					{
						DestinationTableName = $"{GetResourceDBPrepend()}.[{fullTableName}]"
					};
					context.ExecuteBulkCopy(artifactIdTable, bulkParameters);
				}
			}
		}

		public IScratchTableRepository CopyTempTable(string newTempTablePrefix)
		{
			ScratchTableRepository copiedScratchTableRepository = new ScratchTableRepository(_caseContext, _documentRepository, _fieldQueryRepository, _resourceDbProvider, newTempTablePrefix, _tableSuffix, _workspaceId);
			string sourceTableName = GetTempTableName();
			string newTableName = copiedScratchTableRepository.GetTempTableName();

			string sql = String.Format(@"SELECT * INTO {2}.[{0}] FROM {2}.[{1}]", newTableName, sourceTableName, GetResourceDBPrepend());

			_caseContext.ExecuteNonQuerySQLStatement(sql);

			copiedScratchTableRepository._count = _count;

			return copiedScratchTableRepository;
		}

		public void DeleteTable()
		{
			string fullTableName = GetTempTableName();
			string sql = String.Format(@"IF EXISTS (SELECT * FROM {1}.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}') DROP TABLE {2}.[{0}]", 
				fullTableName, GetSchemalessResourceDataBasePrepend(), GetResourceDBPrepend());

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public string GetTempTableName()
		{
			if (_tempTableName == null)
			{
				_tempTableName = $"{_tablePrefix}_{_tableSuffix}";
				if (_tempTableName.Length > 128)
				{
					throw new Exception($"Unable to create scratch table - {_tempTableName}. The name of the table is too long. Please contact the system administrator.");
				}
			}
			return _tempTableName;
		}

		public string GetSchemalessResourceDataBasePrepend()
		{
			return _resourceDbProvider.GetSchemalessResourceDataBasePrepend(_workspaceId);
		}

		public string GetResourceDBPrepend()
		{
			return _resourceDbProvider.GetResourceDbPrepend(_workspaceId);
		}

		private ICollection<int> GetErroredDocumentId(ICollection<string> docIdentifiers)
		{
			if (String.IsNullOrEmpty(_docIdentifierFieldName))
			{
				_docIdentifierFieldName = GetDocumentIdentifierField();
			}

			ICollection<int> documentIds = QueryForDocumentArtifactId(docIdentifiers);
			return documentIds;
		}

		internal string GetDocumentIdentifierField()
		{
			ArtifactDTO[] fieldArtifacts = _fieldQueryRepository.RetrieveFieldsAsync(
				10,
				new HashSet<string>(new[]
				{
					Fields.Name,
					Fields.IsIdentifier
				})).ConfigureAwait(false).GetAwaiter().GetResult();

			string fieldName = String.Empty;
			foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
			{
				int isIdentifierFieldValue = 0;
				foreach (ArtifactFieldDTO field in fieldArtifact.Fields)
				{
					if (field.Name == Fields.Name)
					{
						fieldName = field.Value.ToString();
					}
					if (field.Name == Fields.IsIdentifier)
					{
						try
						{
							isIdentifierFieldValue = Convert.ToInt32(field.Value);
						}
						catch
						{
							// suppress error for invalid casts
						}
					}
				}
				if (isIdentifierFieldValue == 1)
				{
					break;
				}
			}
			return fieldName;
		}

		internal List<int> QueryForDocumentArtifactId(ICollection<string> docIdentifiers)
		{
			ArtifactDTO[] documents;
			string queryFailureMessage = "Unable to retrieve Document Artifact ID. Object Query failed.";
			try
			{
				Task<ArtifactDTO[]> documentResult = _documentRepository.RetrieveDocumentsAsync(_docIdentifierFieldName, docIdentifiers);
				documents = documentResult.ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				throw new Exception(queryFailureMessage, ex);
			}

			if (documents == null)
			{
				throw new Exception(queryFailureMessage);
			}

			return documents.Select(x => x.ArtifactId).ToList();
		}

		internal static class Fields //MNG: similar to class used in DocumentTransferProvider, probably find a better way to reference these
		{
			internal static string Name = "Name";
			internal static string IsIdentifier = "Is Identifier";
		}
	}
}