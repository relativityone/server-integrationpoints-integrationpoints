using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using System.Collections.Generic;
using System.Data;
using System;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
    public class TempTableReader : RelativityReaderBase
    {
        private const int _BATCH_SIZE = 500;
        private readonly IDocumentRepository _documentRepository;
        private IDataReader _scratchTableReader;
        private readonly DataColumn[] _columns;
        private readonly int _identifierFieldId;
        private bool _containsData;

        public TempTableReader(
            IDocumentRepository documentRepository,
            IScratchTableRepository scratchTable,
            DataColumn[] columns,
            int identifierFieldId)
            : base(columns)
        {
            _documentRepository = documentRepository;
            _scratchTableReader = scratchTable.GetDocumentIDsDataReaderFromTable();
            _columns = columns;
            _identifierFieldId = identifierFieldId;
            _containsData = true;
        }

        public override string GetDataTypeName(int i)
        {
            return GetFieldType(i).ToString();
        }

        public override Type GetFieldType(int i)
        {
            if (i < _columns.Length)
            {
                return typeof(string);
            }
            throw new IndexOutOfRangeException();
        }

        public override object GetValue(int i)
        {
            DataColumnWithValue dataColumn = _columns[i] as DataColumnWithValue;
            if (dataColumn != null)
            {
                return dataColumn.Value;
            }
            return CurrentArtifact.Fields[i].Value;
        }

        protected override ArtifactDTO[] FetchArtifactDTOs()
        {
            List<int> documents = new List<int>();
            while (documents.Count < _BATCH_SIZE && (_containsData = _scratchTableReader.Read()))
            {
                int artifactId = _scratchTableReader.GetInt32(0);
                documents.Add(artifactId);
            }
            ArtifactDTO[] artifacts = null;
            if (documents.Count > 0)
            {
                artifacts = _documentRepository.RetrieveDocumentsAsync(documents,
                    new HashSet<int>(new[] { _identifierFieldId })).GetAwaiter().GetResult();
            }
            else
            {
                artifacts = new ArtifactDTO[0];
            }
            return artifacts;
        }

        protected override bool AllArtifactsFetched()
        {
            return !_containsData;
        }

        public override void Close()
        {
            if (ReaderOpen)
            {
                _scratchTableReader.Close();
                _scratchTableReader = null;
                base.Close();
            }
        }
    }
}
