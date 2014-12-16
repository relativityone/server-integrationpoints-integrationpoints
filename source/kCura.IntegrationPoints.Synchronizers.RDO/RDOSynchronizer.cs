using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Utility.Extensions;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizer : kCura.IntegrationPoints.Core.Services.Syncronizer.IDataSyncronizer
	{
		private RelativityFieldQuery _fieldQuery;

		public RdoSynchronizer(RelativityFieldQuery fieldQuery)
		{
			_fieldQuery = fieldQuery;
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

		public FieldEntry GetIdentifier(string options)
		{
			var json = JsonConvert.DeserializeObject<Core.Models.SyncConfiguration.RelativityConfiguration>(options);
			var fields = _fieldQuery.GetFieldsForRDO(json.ArtifactTypeID);
			var identifierField = new FieldEntry();
			foreach (var result in fields)
			{
				foreach (var items in result.Fields)
				{
					if (items.FieldCategory == FieldCategory.Identifier && !IgnoredList.Contains(result.Name))
					{
						identifierField.DisplayName = result.Name;
						identifierField.FieldIdentifier = result.ArtifactID.ToString();
						return identifierField;
					}
					}
				}
			return null;
		}
		public IEnumerable<FieldEntry> GetFields(string options)
		{
			var json = JsonConvert.DeserializeObject<Core.Models.SyncConfiguration.RelativityConfiguration>(options);
			var fields = _fieldQuery.GetFieldsForRDO(json.ArtifactTypeID);
			var allFieldsForRdo = new List<FieldEntry>();
			foreach (var result in fields)
			{
				if (!IgnoredList.Contains(result.Name))
				{
					allFieldsForRdo.Add(new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString()});
				}
			}
			return allFieldsForRdo;
		}


		public bool HasParent(string options)
		{
			int id = 0;
			Int32.TryParse(options, out id);
			return false;
		}


		private IImportService _importService;
		private bool _isJobComplete = false;
		private Exception _jobError;
		private List<KeyValuePair<string, string>> _rowErrors;
		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{

			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(options);
			Dictionary<string, int> importFieldMap = fieldMap.ToDictionary(x => x.SourceField.FieldIdentifier, x => int.Parse(x.DestinationField.FieldIdentifier));

			if (fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Parent))
			{
				settings.ParentObjectIdSourceFieldName =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Parent).Select(x => x.SourceField.FieldIdentifier).First();
			}
			if (fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Identifier))
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
