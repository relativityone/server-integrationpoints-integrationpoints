using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class Fields
    {
        public static string GetDocumentIdentifierFieldName(IFieldQueryRepository fieldQueryRepository)
        {
            ArtifactDTO[] fieldArtifacts = fieldQueryRepository.RetrieveFieldsAsync(
                10,
                new HashSet<string>(new[]
                {
                    "Name",
                    "Is Identifier"
                })).ConfigureAwait(false).GetAwaiter().GetResult();

            string fieldName = string.Empty;
            foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
            {
                int isIdentifierFieldValue = 0;
                foreach (ArtifactFieldDTO field in fieldArtifact.Fields)
                {
                    if (field.Name == "Name")
                    {
                        fieldName = field.Value.ToString();
                    }
                    if (field.Name == "Is Identifier")
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
    }
}
