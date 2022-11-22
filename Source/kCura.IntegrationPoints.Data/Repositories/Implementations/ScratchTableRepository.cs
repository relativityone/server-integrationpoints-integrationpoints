using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class ScratchTableRepository : IScratchTableRepository
    {
        private const int _SCRATCH_TABLE_NAME_LENGTH_LIMIT = 128;
        private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "ArtifactID";

        private readonly IDocumentRepository _documentRepository;
        private readonly IFieldQueryRepository _fieldQueryRepository;
        private readonly int _workspaceId;
        private readonly IResourceDbProvider _resourceDbProvider;

        private readonly IWorkspaceDBContext _caseContext;
        private readonly string _tablePrefix;
        private readonly string _tableSuffix;

        private IDataReader _reader;
        private string _docIdentifierFieldName;
        private string _tempTableName;

        public ScratchTableRepository(
            IWorkspaceDBContext caseContext,
            IDocumentRepository documentRepository,
            IFieldQueryRepository fieldQueryRepository,
            IResourceDbProvider resourceDbProvider,
            string tablePrefix,
            string tableSuffix,
            int workspaceId)
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

        public int GetCount()
        {
            string fullTableName = GetTempTableName();
            string resourceDBPrepend = GetResourceDBPrepend();
            string schemalessResourceDataBasePrepend = GetSchemalessResourceDataBasePrepend();
            string sql =
            $@"
            IF EXISTS
                (SELECT *
                FROM {schemalessResourceDataBasePrepend}.INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = '{fullTableName}')
            SELECT COUNT(*)
            FROM {resourceDBPrepend}.[{fullTableName}]
            ";
            return _caseContext.ExecuteSqlStatementAsScalar<int>(sql, Enumerable.Empty<SqlParameter>());
        }

        public void RemoveErrorDocuments(ICollection<string> documentControlNumbers)
        {
            ICollection<int> docIds = GetErroredDocumentId(documentControlNumbers);

            if (docIds.Count == 0)
            {
                return;
            }

            string documentList = $"({string.Join(",", docIds)})";

            string fullTableName = GetTempTableName();
            string resourceDBPrepend = GetResourceDBPrepend();
            string sql = $@"DELETE FROM {resourceDBPrepend}.[{fullTableName}] WHERE [{_DOCUMENT_ARTIFACT_ID_COLUMN_NAME}] in {documentList}";

            _caseContext.ExecuteNonQuerySQLStatement(sql);
        }

        public IDataReader GetDocumentIDsDataReaderFromTable()
        {
            if (_reader == null)
            {
                _reader = CreateDocumentIDsReader();
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
            if (artifactIds.IsNullOrEmpty())
            {
                return;
            }

            string fullTableName = GetTempTableName();
            string schemalessResourceDataBasePrepend = GetSchemalessResourceDataBasePrepend();
            string resourceDBPrepend = GetResourceDBPrepend();

            ConnectionData connectionData = ConnectionData.GetConnectionDataWithCurrentCredentials(_caseContext.ServerName, GetSchemalessResourceDataBasePrepend());
            string connectionString =
            $@"
                data source={connectionData.Server};
                initial catalog={connectionData.Database};
                persist security info=False;
                user id={connectionData.Username};
                password={connectionData.Password};
                workstation id=localhost;
                packet size=4096;
                connect timeout=30;
                ";
            Context context = new Context(connectionString);

            string sql =
            $@"
                IF NOT EXISTS (SELECT * FROM {schemalessResourceDataBasePrepend}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{fullTableName}')
                BEGIN
                    CREATE TABLE {resourceDBPrepend}.[{fullTableName}] ([{_DOCUMENT_ARTIFACT_ID_COLUMN_NAME}] INT PRIMARY KEY CLUSTERED)
                END";

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

        public IScratchTableRepository CopyTempTable(string newTempTablePrefix)
        {
            ScratchTableRepository copiedScratchTableRepository = new ScratchTableRepository(_caseContext, _documentRepository, _fieldQueryRepository, _resourceDbProvider, newTempTablePrefix, _tableSuffix, _workspaceId);
            string sourceTableName = GetTempTableName();
            string newTableName = copiedScratchTableRepository.GetTempTableName();
            string resourceDBPrepend = GetResourceDBPrepend();

            string sql = $"SELECT * INTO {resourceDBPrepend}.[{newTableName}] FROM {resourceDBPrepend}.[{sourceTableName}]";

            _caseContext.ExecuteNonQuerySQLStatement(sql);

            return copiedScratchTableRepository;
        }

        public void DeleteTable()
        {
            string fullTableName = GetTempTableName();
            string schemalessResourceDataBasePrepend = GetSchemalessResourceDataBasePrepend();
            string resourceDBPrepend = GetResourceDBPrepend();
            string sql =
            $@"
            IF EXISTS (SELECT * FROM {schemalessResourceDataBasePrepend}.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{fullTableName}') 
            DROP TABLE {resourceDBPrepend}.[{fullTableName}]
            ";

            _caseContext.ExecuteNonQuerySQLStatement(sql);
        }

        public IEnumerable<int> ReadArtifactIDs(int offset, int size)
        {
            return ReadArtifactIDsInternal(offset, size).ToList();
        }

        public string GetTempTableName()
        {
            if (_tempTableName == null)
            {
                _tempTableName = $"{_tablePrefix}_{_tableSuffix}";
                if (_tempTableName.Length > _SCRATCH_TABLE_NAME_LENGTH_LIMIT)
                {
                    throw new IntegrationPointsException($"Unable to create scratch table - {_tempTableName}. The name of the table is too long (limit: {_SCRATCH_TABLE_NAME_LENGTH_LIMIT}). Please contact the system administrator.");
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

        private IEnumerable<int> ReadArtifactIDsInternal(int offset, int size)
        {
            using (IDataReader reader = CreateBatchOfDocumentIdReader(offset, size))
            {
                while (reader.Read())
                {
                    yield return reader.GetInt32(0);
                }
            }
        }

        private ICollection<int> GetErroredDocumentId(ICollection<string> documentControlNumbers)
        {
            if (string.IsNullOrEmpty(_docIdentifierFieldName))
            {
                _docIdentifierFieldName = GetDocumentIdentifierField();
            }

            ICollection<int> documentIds = QueryForDocumentArtifactId(documentControlNumbers);
            return documentIds;
        }

        private string GetDocumentIdentifierField()
        {
            ArtifactDTO[] fieldArtifacts = _fieldQueryRepository.RetrieveFieldsAsync(
                    rdoTypeID: 10,
                    fieldNames: new HashSet<string>(
                        new[]
                        {
                            Fields.Name,
                            Fields.IsIdentifier
                        }))
                .GetAwaiter()
                .GetResult();

            string fieldName = string.Empty;
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

        private int[] QueryForDocumentArtifactId(ICollection<string> docIdentifiers)
        {
            string queryFailureMessage = "Unable to retrieve Document Artifact ID. Object Query failed.";

            int[] documents;
            try
            {
                documents = _documentRepository.RetrieveDocumentsAsync(_docIdentifierFieldName, docIdentifiers)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                throw new Exception(queryFailureMessage, ex);
            }

            if (documents == null)
            {
                throw new Exception(queryFailureMessage);
            }

            return documents;
        }

        private IDataReader CreateDocumentIDsReader()
        {
            string sql = CreateSQLForGettingDocumentIDsReader();
            return _caseContext.ExecuteSQLStatementAsReader(sql);
        }

        private IDataReader CreateBatchOfDocumentIdReader(int offset, int size)
        {
            string sql = CreateSQLForGettingDocumentIDsReader() + $"ORDER BY [{_DOCUMENT_ARTIFACT_ID_COLUMN_NAME}] OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY";

            return _caseContext.ExecuteSQLStatementAsReader(sql);
        }

        private string CreateSQLForGettingDocumentIDsReader()
        {
            string fullTableName = GetTempTableName();
            string schemalessResourceDataBasePrepend = GetSchemalessResourceDataBasePrepend();
            string resourceDBPrepend = GetResourceDBPrepend();
            string sql =
            $@"
            IF EXISTS (SELECT * FROM {schemalessResourceDataBasePrepend}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{fullTableName}') 
            SELECT [{_DOCUMENT_ARTIFACT_ID_COLUMN_NAME}] FROM {resourceDBPrepend}.[{fullTableName}]
            ";
            return sql;
        }

        private static class Fields
        {
            internal static readonly string Name = "Name";
            internal static readonly string IsIdentifier = "Is Identifier";
        }
    }
}
