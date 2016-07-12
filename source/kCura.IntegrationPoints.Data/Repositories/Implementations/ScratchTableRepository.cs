using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Toggle;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class ScratchTableRepository : IScratchTableRepository
    {
        private readonly IDBContext _caseContext;
		private readonly IDocumentRepository _documentRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly string _tablePrefix;
        private readonly string _tableSuffix;
        private readonly int _workspaceId;
        private IDataReader _reader;
        private string _database;
        private string _tempTableName;
        private string _docIdentifierFieldName;
        private int _count;
        private readonly bool _isAOAGEnabled;

        public ScratchTableRepository(IHelper helper, IExtendedRelativityToggle toggleProvider, IDocumentRepository documentRepository,
            IFieldRepository fieldRepository, string tablePrefix, string tableSuffix, int workspaceId)
        {
			_caseContext = helper.GetDBContext(workspaceId);
            _documentRepository = documentRepository;
            _fieldRepository = fieldRepository;
            _tablePrefix = tablePrefix;
            _tableSuffix = tableSuffix;
            _workspaceId = workspaceId;
            IgnoreErrorDocuments = false;
            _isAOAGEnabled = toggleProvider.IsAOAGFeatureEnabled();
        }

	    private ScratchTableRepository(IDBContext caseContext, IDocumentRepository documentRepository, IFieldRepository fieldRepository, 
			string tablePrefix, string tableSuffix, int workspaceId, bool isAOAGEnabled)
	    {
			_caseContext = caseContext;
			_documentRepository = documentRepository;
			_fieldRepository = fieldRepository;
			_tablePrefix = tablePrefix;
			_tableSuffix = tableSuffix;
			_workspaceId = workspaceId;
			IgnoreErrorDocuments = false;
			_isAOAGEnabled = isAOAGEnabled;
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
            string sql = String.Format(@"DELETE FROM {2}[{0}] WHERE [ArtifactID] in {1}", fullTableName, documentList, FullDatabaseFormat);

            _caseContext.ExecuteNonQuerySQLStatement(sql);
        }

        public IDataReader GetDocumentIdsDataReaderFromTable()
        {
            if (_reader == null)
            {
                string fullTableName = GetTempTableName();

                var sql = String.Format(@"IF EXISTS (SELECT * FROM {1}INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											SELECT [ArtifactID] FROM {2}[{0}]", fullTableName, TargetDatabaseFormat, FullDatabaseFormat);

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
			_count += artifactIds.Count;

            if (!artifactIds.IsNullOrEmpty())
            {
                string fullTableName = GetTempTableName();

				string database = _isAOAGEnabled ? _caseContext.Database : "EDDSRESOURCE";
				ConnectionData connectionData = ConnectionData.GetConnectionDataWithCurrentCredentials(_caseContext.ServerName, database);
				Context context = new Context(connectionData);

				DataTable artifactIdTable = new DataTable();
				artifactIdTable.Columns.Add();
				foreach (int artifactId in artifactIds)
				{
					artifactIdTable.Rows.Add(artifactId);
				}

				string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM {1}INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN
												CREATE TABLE {2}[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END", fullTableName, TargetDatabaseFormat, FullDatabaseFormat);
				_caseContext.ExecuteNonQuerySQLStatement(sql);

				context.ExecuteBulkCopy(artifactIdTable, new SqlBulkCopyParameters(fullTableName));
            }
        }

	    public IScratchTableRepository CopyTempTable(string newTempTablePrefix)
	    {
		    IScratchTableRepository copiedScratchTableRepository = new ScratchTableRepository(_caseContext, _documentRepository, _fieldRepository, newTempTablePrefix, _tableSuffix, _workspaceId, _isAOAGEnabled);
		    string sourceTableName = GetTempTableName();
		    string newTableName = copiedScratchTableRepository.GetTempTableName();

			string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM {2}INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
										BEGIN
											CREATE TABLE {3}[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
										END
										SELECT * INTO {3}[{0}] ([ArtifactID]) FROM {3}[{1}]", newTableName, sourceTableName, TargetDatabaseFormat, FullDatabaseFormat);
			
			_caseContext.ExecuteNonQuerySQLStatement(sql);

		    return copiedScratchTableRepository;
	    }

        private List<int> RetrieveBatchFromList(List<int> source)
        {
            // The INSERT statement can only have the maximum allowed number of 1000 row values. 
            const int maxBatchSize = 1000;
            int sizeToGet = source.Count < maxBatchSize ? source.Count : maxBatchSize;
            List<int> result = source.GetRange(0, sizeToGet);
            source.RemoveRange(0, sizeToGet);
            return result;
        }

        public void DeleteTable()
        {
            string fullTableName = GetTempTableName();
            string sql = String.Format(@"IF EXISTS (SELECT * FROM {1}INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE {2}[{0}]", fullTableName, TargetDatabaseFormat, FullDatabaseFormat);

            _caseContext.ExecuteNonQuerySQLStatement(sql);
        }

        private string TargetDatabaseFormat
        {
            get
            {
                if (_database == null)
                {
                    _database = _isAOAGEnabled ? String.Empty : "[EDDSRESOURCE].";
                }
                return _database;
            }
        }

        private string FullDatabaseFormat
        {
            get { return TargetDatabaseFormat == String.Empty ? "[Resource]." : "[EDDSRESOURCE].."; }
        }

        public string GetTempTableName()
        {
            if (_tempTableName == null)
            {
                string prepend = String.Empty;
                if (_isAOAGEnabled)
                {
                    prepend = $"{ClaimsPrincipal.Current.GetSchemalessResourceDataBasePrepend(_workspaceId)}_";
                }
                _tempTableName = $"{prepend}{_tablePrefix}_{_tableSuffix}";
                if (_tempTableName.Length > 128)
                {
                    throw new Exception($"Unable to create scratch table - {_tempTableName}. The name of the table is too long. Please contact the system administrator.");
                }
            }
            return _tempTableName;
        }

        public string GetSchemalessResourceDataBasePrepend()
        {
            return ClaimsPrincipal.Current.GetSchemalessResourceDataBasePrepend(_workspaceId);
        }

        public string GetResourceDBPrepend()
        {
            return ClaimsPrincipal.Current.ResourceDBPrepend(_workspaceId);
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