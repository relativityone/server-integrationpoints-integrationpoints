using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;
using Relativity.Data.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Data
{
	public class TempDocTableHelper : ITempDocTableHelper
	{
		private readonly IDBContext _caseContext;
		private readonly IFieldRepository _fieldRepository;
		private readonly IDocumentRepository _documentRepository;
		private readonly string _tableSuffix;
		private readonly int _sourceWorkspaceId;
		private string _docIdentifierField;
		private readonly IToggleProvider _toggleProvider;
		private string _database;
		private string _tempTableName;

		public TempDocTableHelper(IHelper helper, string tableSuffix, int sourceWorkspaceId, IFieldRepository fieldRepository, IDocumentRepository documentRepository, IToggleProvider toggleProvider)
			: this (helper,tableSuffix, sourceWorkspaceId, fieldRepository, documentRepository, null, toggleProvider)
		{
		}

		/// <summary>
		/// For internal testing only
		/// </summary>
		internal TempDocTableHelper(IHelper helper, string tableSuffix, int sourceWorkspaceId, IFieldRepository fieldRepository, IDocumentRepository documentRepository, string docIdField, IToggleProvider toggleProvider)
		{
			_caseContext = helper.GetDBContext(sourceWorkspaceId);
			_tableSuffix = tableSuffix;
			_sourceWorkspaceId = sourceWorkspaceId;
			_docIdentifierField = docIdField;
			_toggleProvider = toggleProvider;
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

				string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM {2}INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN
												CREATE TABLE {3}[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO {3}[{0}] ([ArtifactID]) VALUES {1}", fullTableName, artifactIdList, TargetDatabaseFormat, FullDatabaseFormat);

				_caseContext.ExecuteNonQuerySQLStatement(sql);
			}
		}

		public void RemoveErrorDocument(string tablePrefix, string docIdentifier)
		{
			int docId = GetErroredDocumentId(docIdentifier);
			string fullTableName = GetTempTableName(tablePrefix);
			string sql = String.Format(@"DELETE FROM {2}[{0}] WHERE [ArtifactID] = {1}", fullTableName, docId, FullDatabaseFormat);

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public IDataReader GetDocumentIdsDataReaderFromTable(string tablePrefix)
		{
			string fullTableName = GetTempTableName(tablePrefix);

			var sql = String.Format(@"IF EXISTS (SELECT * FROM {1}INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
										SELECT [ArtifactID] FROM {2}[{0}]", fullTableName, TargetDatabaseFormat, FullDatabaseFormat);

			return _caseContext.ExecuteSQLStatementAsReader(sql);
		}

		public void DeleteTable(string tablePrefix)
		{
			string fullTableName = GetTempTableName(tablePrefix);
			string sql = String.Format(@"IF EXISTS (SELECT * FROM {1}INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE {2}[{0}]", fullTableName, TargetDatabaseFormat, FullDatabaseFormat);
			
			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public string TargetDatabaseFormat
		{
			get
			{
				if (_database == null)
				{
					if (_toggleProvider.IsFeatureEnabled<AOAGToggle>())
					{
						_database = String.Empty;
					}
					else
					{
						_database = "[EDDSRESOURCE].";
					}
				}
				return _database;
			}
		}

		public string FullDatabaseFormat
		{
			get { return TargetDatabaseFormat == String.Empty ? "[eddsdbo]." : "[EDDSRESOURCE].."; }
		}

		public string GetTempTableName(string tablePrefix)
		{
			if (_tempTableName == null)
			{
				string prepend = String.Empty;
				if (_toggleProvider.IsFeatureEnabled<AOAGToggle>())
				{
					prepend = $"{ClaimsPrincipal.Current.GetSchemalessResourceDataBasePrepend(_sourceWorkspaceId)}_";
				}
				_tempTableName = $"{prepend}{tablePrefix}_{_tableSuffix}";
				if (_tempTableName.Length > 128)
				{
					throw new Exception($"Unable to create scratch table - {_tempTableName}. The name of the table is too long. Please contract the system administrator.");
				}
			}
			return _tempTableName;
		}

		public int GetTempTableCount(string tablePrefix)
		{
			string fullTableName = GetTempTableName(tablePrefix);
			string sql = String.Format(@"IF EXISTS (SELECT * FROM {1}.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										SELECT COUNT(*) FROM {2}[{0}]", fullTableName, TargetDatabaseFormat, FullDatabaseFormat);

			return _caseContext.ExecuteSqlStatementAsScalar<int>(sql);
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