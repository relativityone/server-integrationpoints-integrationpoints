using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class DocumentTest : RdoTestBase
    {
        private const string DOCUMENT_NAME = "Document";

        private readonly Dictionary<string, object> _fieldValues;
        
        public override List<Guid> Guids => new List<Guid>();

        public string ControlNumber { get; set; }

        public bool HasImages { get; set; }

        public bool HasNatives { get; set; }

        public bool HasFields => _fieldValues.Any();

        public int? ImageCount { get; set; }

        public string FolderName { get; set; }

        public string ExtractedText { get; set; }

        public DocumentTest(RelativityObject relativityObject) : base(DOCUMENT_NAME)
        {
            _fieldValues = relativityObject.FieldValues.ToDictionary(x => x.Field.Name, x => x.Value);
        }

        public DocumentTest() : base(DOCUMENT_NAME)
        {
            _fieldValues = new Dictionary<string, object>();
        }

        public DocumentTest(IList<FieldTest> fields) : base(DOCUMENT_NAME)
        {
            _fieldValues = new Dictionary<string, object>();
            foreach (FieldTest field in fields)
            {
                _fieldValues.Add(field.Name, field);
            }
        }

        public DocumentTest(Dictionary<string, object> fieldValues) : base(DOCUMENT_NAME)
        {
            _fieldValues = fieldValues;
        }

        public void AddField(FieldValuePair field)
        {
            _fieldValues.Add(field.Field.Name, field.Value);
        }

        public override RelativityObject ToRelativityObject()
        {
            List<FieldValuePair> fieldValues = new List<FieldValuePair>();
            foreach (KeyValuePair<string, object> fieldValue in _fieldValues)
            {
                fieldValues.Add(new FieldValuePair
                {
                    Field = new Field
                    {
                        Name = fieldValue.Key,
                    },
                    Value = fieldValue.Value
                }
                );
            }

            var relativityObject = new RelativityObject
            {
                ArtifactID = ArtifactId,
                FieldValues = fieldValues
            };

            return relativityObject;
        }
    }
}
