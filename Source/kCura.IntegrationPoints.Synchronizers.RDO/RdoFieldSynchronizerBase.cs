using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public abstract class RdoFieldSynchronizerBase : IFieldProvider
	{
		private readonly IImportApiFactory _factory;
		private readonly IAPILog _logger;
		protected readonly IRelativityFieldQuery FieldQuery;

		private IImportAPI _api;

		private HashSet<string> _ignoredList;

		private string _webApiPath;

		protected RdoFieldSynchronizerBase(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IHelper helper)
		{
			FieldQuery = fieldQuery;
			_factory = factory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoFieldSynchronizerBase>();
		}

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

		public string WebAPIPath
		{
			get
			{
				if (string.IsNullOrEmpty(_webApiPath))
				{
					_webApiPath = Config.Config.Instance.WebApiPath;
				}
				return _webApiPath;
			}
			protected set { _webApiPath = value; }
		}

		public virtual IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			LogRetrievingFields();
			ImportSettings settings = GetSettings(providerConfiguration.Configuration);
			var fields = GetRelativityFields(settings);
			return ParseFields(fields);
		}

		protected ImportSettings GetSettings(string options)
		{
			var settings = DeserializeImportSettings(options);

			if (string.IsNullOrEmpty(settings.WebServiceURL))
			{
				settings.WebServiceURL = WebAPIPath;
				if (string.IsNullOrEmpty(settings.WebServiceURL))
				{
					LogMissingWebApiPath();
					throw new Exception("No WebAPI path set for integration points.");
				}
			}
			return settings;
		}

		private ImportSettings DeserializeImportSettings(string options)
		{
			try
			{
				return JsonConvert.DeserializeObject<ImportSettings>(options);
			}
			catch (Exception e)
			{
				LogImportSettingsDeserializationError(e);
				throw;
			}
		}

		protected List<Artifact> GetRelativityFields(ImportSettings settings)
		{
			LogRetrievingRelativityFields();
			try
			{
				List<Artifact> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId);
				HashSet<int> mappableArtifactIds =
					new HashSet<int>(GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId).Select(x => x.ArtifactID));
				return fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
			}
			catch (Exception e)
			{
				LogRetrievingRelativityFieldsError(e);
				throw;
			}
		}

		protected IEnumerable<FieldEntry> ParseFields(List<Artifact> fields)
		{
			foreach (var result in fields)
			{
				if (!IgnoredList.Contains(result.Name))
				{
					Field idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
					bool isIdentifier = Convert.ToInt32(idField?.Value) == 1;
				
					if (isIdentifier)
					{
						result.Name += " [Object Identifier]";
					}
				
					yield return new FieldEntry {DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString(), IsIdentifier = isIdentifier, IsRequired = false};
				}
			}
		}

		protected IImportAPI GetImportApi(ImportSettings settings)
		{
			return _api ?? (_api = _factory.GetImportAPI(settings));
		}

		#region Logging

		private void LogRetrievingFields()
		{
			_logger.LogInformation("Attempting to retrieve fields.");
		}

		private void LogMissingWebApiPath()
		{
			_logger.LogError("No WebAPI path set for integration points.");
		}

		private void LogImportSettingsDeserializationError(Exception e)
		{
			_logger.LogError(e, "Failed to deserialize Import Settings.");
		}

		private void LogRetrievingRelativityFieldsError(Exception e)
		{
			_logger.LogError(e, "Failed to retrieve Relativity fields.");
		}

		private void LogRetrievingRelativityFields()
		{
			_logger.LogVerbose("Attempting to retrieve Relativity fields.");
		}

		#endregion
	}
}