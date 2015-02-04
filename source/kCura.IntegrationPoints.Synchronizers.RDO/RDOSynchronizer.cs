using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using kCura.Apps.Common.Config;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.DataReaderClient;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizer : kCura.IntegrationPoints.Contracts.Syncronizer.IDataSyncronizer
	{
		protected readonly RelativityFieldQuery FieldQuery;
		protected readonly RelativityRdoQuery RdoQuery;

		public RdoSynchronizer(RelativityFieldQuery fieldQuery, RelativityRdoQuery rdoQuery)
		{
			FieldQuery = fieldQuery;
			RdoQuery = rdoQuery;
		}

		private List<string> IgnoredList
		{
			get
			{
				// fields don't have any space in between words 
				var list = new List<string>
			    {
					"Is System Artifact",
					"System Created By",
					"System Created On",
					"System Last Modified By",
					"System Last Modified On",
					"Artifact ID"
			    };
				return list;
			}
		}



		public virtual IEnumerable<FieldEntry> GetFields(string options)
		{
			ImportSettings settings = GetSettings(options);
			var fields = FieldQuery.GetFieldsForRDO(settings.ArtifactTypeId);
			return ParseFields(fields);
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
					}
					yield return new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString(), IsIdentifier = isIdentifier };
				}
			}
		}


		private IImportService _importService;
		private bool _isJobComplete = false;
		private Exception _jobError;
		private List<KeyValuePair<string, string>> _rowErrors;

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			ImportSettings settings = GetSettings(options);

			Dictionary<string, int> importFieldMap = null;

			try
			{
				importFieldMap = fieldMap.Where(x => x.FieldMapType != FieldMapTypeEnum.Parent)
						.ToDictionary(x => x.SourceField.FieldIdentifier, x => int.Parse(x.DestinationField.FieldIdentifier));
			}
			catch (Exception ex)
			{
				throw new Exception("Field Map is invalid.", ex);
			}

			if (string.IsNullOrWhiteSpace(settings.ParentObjectIdSourceFieldName) && fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Parent))
			{
				settings.ParentObjectIdSourceFieldName =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Parent).Select(x => x.SourceField.FieldIdentifier).First();
			}
			if (settings.IdentityFieldId < 1 && fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Identifier))
			{
				settings.IdentityFieldId =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Identifier).Select(x => int.Parse(x.DestinationField.FieldIdentifier)).First();
			}

			_importService = new ImportService(settings, importFieldMap, new BatchManager());
			_importService.OnBatchComplete += new BatchCompleted(Finish);
			_importService.OnDocumentError += new RowError(ItemError);
			_importService.OnJobError += new JobError(JobError);


			_isJobComplete = false;
			_jobError = null;
			_rowErrors = new List<KeyValuePair<string, string>>();

			_importService.Initialize();

			foreach (var row in data)
			{
				Dictionary<string, object> importRow = row.ToDictionary(x => x.Key.FieldIdentifier, x => x.Value);
				_importService.AddRow(importRow);
			}
			_importService.PushBatchIfFull(true);

			bool isJobDone = false;
			do
			{
				lock (_importService)
				{
					isJobDone = bool.Parse(_isJobComplete.ToString());
				}
				Thread.Sleep(1000);
			} while (!isJobDone);

			ProcessExceptions(settings);
		}

		private string _webAPIPath;

		public string WebAPIPath
		{
			get
			{
				if (string.IsNullOrEmpty(_webAPIPath))
				{
					_webAPIPath = kCura.Apps.Common.Config.Sections.EddsDbmtConfig.WebAPIPath;
				}
				return _webAPIPath;
			}
			private set { _webAPIPath = value; }
		}

		protected ImportSettings GetSettings(string options)
		{
			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(options);

			if (string.IsNullOrEmpty(settings.WebServiceURL))
			{
				settings.WebServiceURL = this.WebAPIPath;
			}
			return settings;
		}

		private void ProcessExceptions(ImportSettings settings)
		{
			if (_jobError != null)
			{
				throw _jobError;
			}
			else
			{
				string errorMessage = string.Empty;
				foreach (var rowError in _rowErrors)
				{
					if (settings.OverwriteMode == OverwriteModeEnum.Overlay
						&& rowError.Value.Contains("no document to overwrite"))
					{
						//skip
					}
					else if (settings.OverwriteMode == OverwriteModeEnum.Append
						&& rowError.Value.Contains("document with identifier")
						&& rowError.Value.Contains("already exists in the workspace"))
					{
						//skip
					}
					else
					{
						errorMessage = string.Format("{0}{1}(Id: {2}){3}", errorMessage, Environment.NewLine, rowError.Key, rowError.Value);
					}
				}
				if (!string.IsNullOrEmpty(errorMessage))
				{
					throw new Exception(errorMessage);
				}
			}
		}

		private void Finish(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			lock (_importService)
			{
				_isJobComplete = true;
			}
		}

		private void JobError(Exception ex)
		{
			lock (_importService)
			{
				_isJobComplete = true;
				_jobError = ex;
			}
		}

		private void ItemError(string documentIdentifier, string errorMessage)
		{
			_rowErrors.Add(new KeyValuePair<string, string>(documentIdentifier, errorMessage));
		}



	}
}
