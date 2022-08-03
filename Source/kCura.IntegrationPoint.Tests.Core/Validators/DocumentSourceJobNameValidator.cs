using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentSourceJobNameValidator : IDocumentValidator
    {
        private readonly IRelativityObjectManager _objectManager;
        private readonly string _integrationPointName;

        public DocumentSourceJobNameValidator(IRelativityObjectManager objectManager, string integrationPointName)
        {
            _objectManager = objectManager;
            _integrationPointName = integrationPointName;
        }

        public void ValidateDocument(Document destinationDocument, Document sourceDocument)
        {
            int relativitySourceJobArtifactId = RetrieveDocumentFieldArtifactId("Relativity Source Job", FieldTypes.MultipleObject);

            int destinationDocumentArtifactId = destinationDocument.ArtifactId;
            Assert.That(relativitySourceJobArtifactId, Is.Not.Zero, $"Could not find field 'Relativity Source Job' in document {destinationDocumentArtifactId}");

            object fieldValue = GetFieldValue(destinationDocumentArtifactId, relativitySourceJobArtifactId);

            Assert.That(fieldValue, Is.Not.Null, $"Value of field 'Relativity Source Job' in document {destinationDocumentArtifactId} is null");

            string actualFieldValue = ((IEnumerable<RelativityObjectValue>) fieldValue).First().Name;

            StringAssert.StartsWith(_integrationPointName, actualFieldValue);
        }

        private int RetrieveDocumentFieldArtifactId(string displayName, string fieldType)
        {
            var fieldQuery = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                Fields = new []{ new FieldRef { Name = "ArtifactID" } },
                Condition = $"'Object Type Artifact Type ID' == OBJECT {(int)ArtifactType.Document} AND 'DisplayName' == '{displayName}' AND 'Field Type' == '{fieldType}'"
            };

            try
            {
                RelativityObject result = _objectManager.Query(fieldQuery).FirstOrDefault();
                return result?.ArtifactID ?? 0;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to retrieve field '{displayName}' artifact id", e);
            }
        }

        public object GetFieldValue(int documentId, int fieldId)
        {
            var qr = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                Condition = $"'ArtifactID' in [{string.Join(",", documentId)}]",
                Fields = new []{new FieldRef { ArtifactID = fieldId }}
            };

            try
            {
                RelativityObject document = _objectManager.Query(qr).FirstOrDefault();
                FieldValuePair fieldValue = document?.FieldValues.FirstOrDefault();

                return fieldValue?.Value;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to retrieve field {fieldId} for document {documentId}", e);
            }
        }
    }
}