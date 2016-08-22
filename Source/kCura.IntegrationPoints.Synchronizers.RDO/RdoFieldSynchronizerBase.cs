using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Field = kCura.Relativity.ImportAPI.Data.Field;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public abstract class RdoFieldSynchronizerBase : IFieldProvider
    {
        protected readonly IRelativityFieldQuery FieldQuery;

        private Relativity.ImportAPI.IImportAPI _api;
        private readonly IImportApiFactory _factory;

        protected RdoFieldSynchronizerBase(IRelativityFieldQuery fieldQuery, IImportApiFactory factory)
        {
            FieldQuery = fieldQuery;
            _factory = factory;
        }

        public virtual IEnumerable<FieldEntry> GetFields(string options)
        {
            ImportSettings settings = GetSettings(options);
            var fields = GetRelativityFields(settings);
            return ParseFields(fields);
        }

        protected ImportSettings GetSettings(string options)
        {
            ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(options);

            if (string.IsNullOrEmpty(settings.WebServiceURL))
            {
                settings.WebServiceURL = this.WebAPIPath;
                if (string.IsNullOrEmpty(settings.WebServiceURL))
                {
                    throw new Exception("No WebAPI path set for integration points.");
                }
            }
            return settings;
        }

        protected List<Relativity.Client.Artifact> GetRelativityFields(ImportSettings settings)
        {
            List<Artifact> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId);
			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId).Select(x => x.ArtifactID));
            return fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
        }

        protected IEnumerable<FieldEntry> ParseFields(List<Relativity.Client.Artifact> fields)
        {
            foreach (var result in fields)
            {
                if (!IgnoredList.Contains(result.Name))
                {
                    var idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
                    bool isIdentifier = false;
                    if (idField != null)
                    {
                        isIdentifier = Convert.ToInt32(idField.Value) == 1;
                        if (isIdentifier)
                        {
                            result.Name += " [Object Identifier]";
                        }
                    }
                    yield return new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString(), IsIdentifier = isIdentifier, IsRequired = false };
                }
            }
        }

        private HashSet<string> _ignoredList;
        private HashSet<string> IgnoredList
        {
            get
            {
                // fields don't have any space in between words 
                if (_ignoredList == null)
                {
                    _ignoredList = new HashSet<string>
                    {
                        "Is System Artifact",
                        "System Created By",
                        "System Created On",
                        "System Last Modified By",
                        "System Last Modified On",
                        "Artifact ID"
                    };
                }
                return _ignoredList;
            }
        }

        private string _webAPIPath;
        public string WebAPIPath
        {
            get
            {
                if (string.IsNullOrEmpty(_webAPIPath))
                {
					_webAPIPath = Config.Config.Instance.WebApiPath;
                }
                return _webAPIPath;
            }
            protected set { _webAPIPath = value; }
        }

        protected Relativity.ImportAPI.IImportAPI GetImportApi(ImportSettings settings)
        {
            return _api ?? (_api = _factory.GetImportAPI(settings));
        }
    }
}