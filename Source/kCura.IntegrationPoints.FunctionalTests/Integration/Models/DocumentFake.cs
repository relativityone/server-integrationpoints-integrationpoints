using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class DocumentFake : RdoFakeBase
    {
        private const string DOCUMENT_NAME = "Document";
        private readonly Dictionary<string, object> _fieldValues;

        public override List<Guid> Guids => new List<Guid>();

        public bool HasImages { get; set; }

        public bool HasNatives { get; set; }

        public bool HasFields => _fieldValues.Any();

        public int? ImageCount { get; set; }

        public string FolderName { get; set; }

        public DocumentFake() : base(DOCUMENT_NAME)
        {
            _fieldValues = new Dictionary<string, object>();
        }

        public DocumentFake(IList<FieldFake> fields) : base(DOCUMENT_NAME)
        {
            _fieldValues = new Dictionary<string, object>();
            foreach (FieldFake field in fields)
            {
                _fieldValues.Add(field.Name, field);
            }
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

            RelativityObject relativityObject = new RelativityObject
            {
                ArtifactID = ArtifactId,
                FieldValues = fieldValues
            };

            return relativityObject;
        }
    }
}
