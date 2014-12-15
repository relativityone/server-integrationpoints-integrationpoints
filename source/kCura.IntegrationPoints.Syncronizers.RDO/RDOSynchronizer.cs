using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizer : kCura.IntegrationPoints.Core.Services.Syncronizer.IDataSyncronizer
	{
		private RelativityFieldQuery _fieldQuery;
		private readonly IRSAPIClient _rsapiClient;

		public RdoSynchronizer(IRSAPIClient rsapiClient, RelativityFieldQuery fieldQuery)
		{
			_rsapiClient = rsapiClient;
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
					"System Generated",
					"System Last Modified By",
					"System Last Modified On",
					"Artifact ID"
			    };
				return list;
			}
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
					allFieldsForRdo.Add(new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString() });
				}
			}
			return allFieldsForRdo;
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

			ProcessExceptions();
		}

		private void ProcessExceptions()
		{
			if (_jobError != null)
			{
				throw _jobError;
			}
			else
			{
				foreach (var _rowError in _rowErrors)
				{
					//TODO:					
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
