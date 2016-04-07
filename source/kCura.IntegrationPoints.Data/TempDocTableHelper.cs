using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Managers.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data
{
	public class TempDocTableHelper : ITempDocTableHelper
	{
		private readonly IDBContext _caseContext;
		private readonly IHelper _helper;
		private readonly string _tableSuffix;
		private string _docIdentifierField;
		private readonly int _sourceWorkspaceId;

		public TempDocTableHelper(IHelper helper, string tableSuffix, int sourceWorkspaceId)
		{
			_sourceWorkspaceId = sourceWorkspaceId;
			_helper = helper;
			_caseContext = _helper.GetDBContext(_sourceWorkspaceId);
			_tableSuffix = tableSuffix;
		}

		/// <summary>
		/// For internal testing only
		/// </summary>
		internal TempDocTableHelper(IHelper helper, string tableSuffix, int sourceWorkspaceId, string docIdField)
		{
			_sourceWorkspaceId = sourceWorkspaceId;
			_helper = helper;
			_caseContext = _helper.GetDBContext(_sourceWorkspaceId);
			_tableSuffix = tableSuffix;
			_docIdentifierField = docIdField;
		}

		public void CreateTemporaryDocTable(List<int> artifactIds, ScratchTables rdoTable)
		{
			if (!artifactIds.IsNullOrEmpty())
			{
				string fullTableName;
				if (rdoTable == ScratchTables.DestinationWorkspace)
				{
					fullTableName = String.Format("{0}_{1}", Constants.TEMPORARY_DOC_TABLE_DEST_WS, _tableSuffix);
				}
				else
				{
					fullTableName = String.Format("{0}_{1}", Constants.TEMPORARY_DOC_TABLE_JOB_HIST, _tableSuffix);
				}
				string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

				string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN 
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", fullTableName, artifactIdList);

				_caseContext.ExecuteNonQuerySQLStatement(sql);
			}
		}

		public void RemoveErrorDocument(string docIdentifier)
		{
			int docId = GetDocumentId(docIdentifier);
			string fullTableName = String.Format("{0}_{1}", Constants.TEMPORARY_DOC_TABLE_DEST_WS, _tableSuffix);
			string sql = String.Format(@"DELETE FROM EDDSRESOURCE..[{0}] WHERE [ArtifactID] = {1}", fullTableName, docId);

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public List<int> GetDocumentIdsFromTable(ScratchTables rdoTable)
		{
			string fullTableName;
			if (rdoTable == ScratchTables.DestinationWorkspace)
			{
				fullTableName = String.Format("{0}_{1}", Constants.TEMPORARY_DOC_TABLE_DEST_WS, _tableSuffix);
			}
			else
			{
				fullTableName = String.Format("{0}_{1}", Constants.TEMPORARY_DOC_TABLE_JOB_HIST, _tableSuffix);
			}

			var sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
										SELECT [ArtifactID] FROM [EDDSRESOURCE]..[{0}]", fullTableName);

			var documentIds = new List<int>();

			using (SqlDataReader docIdReader = _caseContext.ExecuteSQLStatementAsReader(sql))
			{
				while (docIdReader.Read())
				{
					int docId = Convert.ToInt32(docIdReader["ArtifactID"]);
					documentIds.Add(docId);
				}
			}

			return documentIds;
		}

		public void DeleteTable(ScratchTables rdoTable)
		{
			string fullTableName;
			if (rdoTable == ScratchTables.DestinationWorkspace)
			{
				fullTableName = String.Format("{0}_{1}", Constants.TEMPORARY_DOC_TABLE_DEST_WS, _tableSuffix);
			}
			else
			{
				fullTableName = String.Format("{0}_{1}", Constants.TEMPORARY_DOC_TABLE_JOB_HIST, _tableSuffix);
			}
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]", fullTableName);

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		private int GetDocumentId(string docIdentifier)
		{
			if (String.IsNullOrEmpty(_docIdentifierField))
			{
				SetDocumentIdentifierField(_helper, _sourceWorkspaceId);
			}

			string sql = String.Format(@"Select [ArtifactId] FROM [Document] WHERE [{0}] = '{1}'", _docIdentifierField, docIdentifier);

			int documentId = _caseContext.ExecuteSqlStatementAsScalar<int>(sql);
			return documentId;
		}

		private void SetDocumentIdentifierField(IHelper helper, int sourceWorkspaceId)
		{
			IObjectQueryManagerAdaptor rdoRepository = new ObjectQueryManagerAdaptor(helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System), sourceWorkspaceId, Convert.ToInt32(ArtifactType.Field));
			IFieldRepository fieldManager = new KeplerFieldRepository(rdoRepository);
			ArtifactDTO[] fieldArtifacts = fieldManager.RetrieveFieldsAsync(
				10,
				new HashSet<string>(new[]
				{
					Fields.Name,
					Fields.IsIdentifier
				})).ConfigureAwait(false).GetAwaiter().GetResult();

			foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
			{
				string fieldName = String.Empty;
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
							if (isIdentifierFieldValue == 1)
							{
								_docIdentifierField = fieldName.Replace(" ", string.Empty);
							}
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
		}

		internal static class Fields //MNG: similar to class used in DocumentTransferProvider, probably find a better way to reference these
		{
			internal static string Name = "Name";
			internal static string IsIdentifier = "Is Identifier";
		}	
	}
}
