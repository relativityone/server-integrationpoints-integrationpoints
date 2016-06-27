using System;
using System.Collections.Generic;
using System.Text;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Models
{
    public class SavedSearchModel
    {
        private SavedSearchModel()
        {
        }

        public int Value { get; private set; }
        public String DisplayName { get; private set; }

        public static IList<SavedSearchModel> GetAllPublicSavedSearches(IRSAPIClient context, IHtmlSanitizerManager htmlSanitizerManager)
        {
            const String identifier = "Text Identifier";
            const String owner = "Owner";

            GetSavedSearchesQuery query = new GetSavedSearchesQuery(context);
            QueryResult queryResult = query.ExecuteQuery();
            List<Artifact> artifacts = queryResult.QueryArtifacts;
            List<SavedSearchModel> result = new List<SavedSearchModel>(artifacts.Count);
            foreach (var artifact in artifacts)
            {
                // if the search doesn't have an Owner, then the search is public
                Field ownerField = artifact.getFieldByName(owner);
                byte[] fieldValue = ownerField.Value as byte[];
                if (fieldValue != null && fieldValue.Length > 0)
                {
                    continue;
                }

                Field textIdentifierField = artifact.getFieldByName(identifier);
                if (textIdentifierField != null && textIdentifierField.Value != null)
                {
                    String searchName = Encoding.Unicode.GetString((byte[])artifact.getFieldByName(identifier).Value);
                    searchName = htmlSanitizerManager.Sanitize(searchName).CleanHTML;
                    result.Add(new SavedSearchModel() { DisplayName = searchName, Value = artifact.ArtifactID });
                }
            }
            return result;
        }
    }
}