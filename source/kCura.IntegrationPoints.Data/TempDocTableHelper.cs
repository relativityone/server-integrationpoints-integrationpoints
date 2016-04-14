using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class TempDocTableHelper : ITempDocTableHelper
	{
		private readonly IDBContext _caseContext;
		private readonly IFieldRepository _fieldRepository;
		private readonly IDocumentRepository _documentRepository;
		private readonly string _tableSuffix;
		private string _docIdentifierField;

		public TempDocTableHelper(IHelper helper, string tableSuffix, int sourceWorkspaceId, IFieldRepository fieldRepository, IDocumentRepository documentRepository)
		{
			_caseContext = helper.GetDBContext(sourceWorkspaceId);
			_tableSuffix = tableSuffix;
			_fieldRepository = fieldRepository;
			_documentRepository = documentRepository;
		}

		/// <summary>
		/// For internal testing only
		/// </summary>
		internal TempDocTableHelper(IHelper helper, string tableSuffix, int sourceWorkspaceId, string docIdField, IFieldRepository fieldRepository, IDocumentRepository documentRepository)
		{
			_caseContext = helper.GetDBContext(sourceWorkspaceId);
			_tableSuffix = tableSuffix;
			_docIdentifierField = docIdField;
			_fieldRepository = fieldRepository;
			_documentRepository = documentRepository;
		}

		public void AddArtifactIdsIntoTempTable(List<int> artifactIds, string tablePrefix)
		{
			if (!artifactIds.IsNullOrEmpty())
			{
				string fullTableName = GetTempTableName(tablePrefix);
				string artifactIdList = String.Join("),(", artifactIds.Select(x => x.ToString()));
				artifactIdList = $"({artifactIdList})";

				string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", fullTableName, artifactIdList);

				_caseContext.ExecuteNonQuerySQLStatement(sql);
			}
		}

		public void RemoveErrorDocument(string tablePrefix, string docIdentifier)
		{
			int docId = GetErroredDocumentId(docIdentifier);
			string fullTableName = GetTempTableName(tablePrefix);
			string sql = String.Format(@"DELETE FROM EDDSRESOURCE..[{0}] WHERE [ArtifactID] = {1}", fullTableName, docId);

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public IDataReader GetDocumentIdsDataReaderFromTable(string tablePrefix)
		{
			string fullTableName = GetTempTableName(tablePrefix);

			var sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
										SELECT [ArtifactID] FROM [EDDSRESOURCE]..[{0}]", fullTableName);

			return _caseContext.ExecuteSQLStatementAsReader(sql);
		}

		public void DeleteTable(string tablePrefix)
		{
			string fullTableName = GetTempTableName(tablePrefix);
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]", fullTableName);

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public string GetTempTableName(string tablePrefix)
		{
			return $"{tablePrefix}_{_tableSuffix}";
		}

		private int GetErroredDocumentId(string docIdentifier)
		{
			if (String.IsNullOrEmpty(_docIdentifierField))
			{
				_docIdentifierField = GetDocumentIdentifierField();
			}

			int documentId = QueryForDocumentArtifactId(docIdentifier);
			return documentId;
		}

		internal string GetDocumentIdentifierField()
		{
			ArtifactDTO[] fieldArtifacts = _fieldRepository.RetrieveFieldsAsync(
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

		internal int QueryForDocumentArtifactId(string docIdentifier)
		{
			ArtifactDTO document;
			try
			{
				Task<ArtifactDTO> documentResult = _documentRepository.RetrieveDocumentAsync(_docIdentifierField, docIdentifier);
				document = documentResult.ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to retrieve Document Artifact ID. Object Query failed.", ex);
			}
			
			return document.ArtifactId;
		}

		internal static class Fields //MNG: similar to class used in DocumentTransferProvider, probably find a better way to reference these
		{
			internal static string Name = "Name";
			internal static string IsIdentifier = "Is Identifier";
		}
	}
}