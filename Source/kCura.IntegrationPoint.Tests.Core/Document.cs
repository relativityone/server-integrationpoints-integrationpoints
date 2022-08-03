using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class Document
    {
        private readonly Dictionary<string, object> _fieldValues;

        public Document(RelativityObject obj)
        {
            _fieldValues = obj.FieldValues.ToDictionary(x => x.Field.Name, x => x.Value);
            ArtifactId = obj.ArtifactID;
            ParentArtifactId = obj.ParentObject.ArtifactID;
        }

        public Document(Dictionary<string, object> fieldValues)
        {
            _fieldValues = fieldValues;
            ArtifactId = ReadAsInt(TestConstants.FieldNames.ARTIFACT_ID);
            ParentArtifactId = ReadAsInt(TestConstants.FieldNames.PARENT_ARTIFACT_ID);
        }

        public int ArtifactId { get; }

        public int ParentArtifactId { get; }

        public string ControlNumber => ReadAsString(TestConstants.FieldNames.CONTROL_NUMBER);

        public Choice HasImages => (Choice) this[TestConstants.FieldNames.HAS_IMAGES];

        public bool? HasNatives => ReadAsBool(TestConstants.FieldNames.HAS_NATIVES);

        public int? ImageCount => ReadAsInt(TestConstants.FieldNames.IMAGE_COUNT);

        public string FolderName => ReadAsString(TestConstants.FieldNames.FOLDER_NAME);

        public string ExtractedText => ReadAsString(TestConstants.FieldNames.EXTRACTED_TEXT);

        public object this[string field]
        {
            get
            {
                object value;
                if (_fieldValues == null || !_fieldValues.TryGetValue(field, out value))
                {
                    return null;
                }

                return value;
            }
        }

        public string ReadAsString(string field) => this[field]?.ToString();

        public bool ReadAsBool(string field) => this[field] != null && (bool) this[field];

        public int ReadAsInt(string field) => this[field] != null ? (int) this[field] : default(int);
    }
}
