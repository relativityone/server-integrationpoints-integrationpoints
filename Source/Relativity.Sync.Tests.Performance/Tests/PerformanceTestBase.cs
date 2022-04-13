using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.Performance.PreConditions;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Extensions;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
	internal class PerformanceTestBase : SystemTest
	{
		private WorkspaceType _workspaceType;
		private bool _wasDestinationForTestCaseCreated;

		protected readonly IDictionary<string, TimeSpan> TestTimes = new ConcurrentDictionary<string, TimeSpan>();

		public WorkspaceRef SourceWorkspace { get; private set; }

		public WorkspaceRef DestinationWorkspace { get; private set; }

		public ARMHelper ARMHelper { get; private set; }
		public AzureStorageHelper StorageHelper { get; private set; }

		public ConfigurationStub Configuration { get; set; }

		public int ConfigurationRdoId { get; set; }

		public PerformanceTestBase()
		{
			Configuration = new ConfigurationStub()
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				CreateSavedSearchForTags = false,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				FolderPathSourceFieldName = "Document Folder Path",
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				MoveExistingDocuments = false,
			};
		}

		public async Task UseArmWorkspaceAsync(string sourceWorkspaceArmFile, string destinationWorkspaceArmFile)
		{
			_workspaceType = WorkspaceType.ARM;
			
			StorageHelper = AzureStorageHelper.CreateFromTestConfig();

			ARMHelper = ARMHelper.CreateInstance();

			ARMHelper.EnableAgents();

			SourceWorkspace = await RestoreWorkspaceAsync(sourceWorkspaceArmFile).ConfigureAwait(false);

			if (!string.IsNullOrEmpty(destinationWorkspaceArmFile))
			{
				DestinationWorkspace = await RestoreWorkspaceAsync(destinationWorkspaceArmFile).ConfigureAwait(false);
			}
		}

		public async Task UseExistingWorkspaceAsync(string sourceWorkspaceName, string destinationWorkspaceName)
		{
			_workspaceType = WorkspaceType.Relativity;

			SourceWorkspace = await Environment.GetWorkspaceAsync(sourceWorkspaceName).ConfigureAwait(false);
			
			if (!string.IsNullOrEmpty(destinationWorkspaceName))
			{
				DestinationWorkspace = await Environment.GetWorkspaceAsync(destinationWorkspaceName).ConfigureAwait(false);
			}
		}

		private async Task<WorkspaceRef> RestoreWorkspaceAsync(string armedWorkspaceFileName)
		{
			string filePath = "";
			try
			{
				filePath = await StorageHelper
					.DownloadFileAsync(armedWorkspaceFileName, Path.GetTempPath()).ConfigureAwait(false);

				Logger.LogInformation($"ARMed workspace saved locally in {filePath}");
				int workspaceArtifactId =
					await ARMHelper.RestoreWorkspaceAsync(filePath, Environment).ConfigureAwait(false);

				await Environment.CreateFieldsInWorkspaceAsync(workspaceArtifactId).ConfigureAwait(false);

				return await Environment.GetWorkspaceAsync(workspaceArtifactId).ConfigureAwait(false);
			}
			finally
			{
				File.Delete(filePath);
			}

		}

		protected override async Task ChildSuiteTeardown()
		{
			await CleanUpWorkspacesAsync().ConfigureAwait(false);

			if (!string.IsNullOrEmpty(AppSettings.PerformanceResultsFilePath))
			{
				File.WriteAllLines(AppSettings.PerformanceResultsFilePath,
					TestTimes.Select(pair => $"{pair.Key};{pair.Value.TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture)}\n"));
			}

			await base.ChildSuiteTeardown().ConfigureAwait(false);
		}

		[SetUp]
		public async Task SetUp()
		{
			if (DestinationWorkspace == null)
			{
				Logger.LogInformation("Creating destination workspace");
				
				DestinationWorkspace = await Environment
					.CreateWorkspaceWithFieldsAsync(templateWorkspaceName: SourceWorkspace.Name).ConfigureAwait(false);
				_wasDestinationForTestCaseCreated = true;

				Logger.LogInformation($"Destination workspace was created: {DestinationWorkspace.ArtifactID}");
			}
		}

		[TearDown]
		public async Task TearDown()
		{
			if (DestinationWorkspace != null && _wasDestinationForTestCaseCreated)
			{
				using (var manager = ServiceFactory.CreateProxy<IWorkspaceManager>())
				{
					await manager.DeleteAsync(DestinationWorkspace).ConfigureAwait(false);
				}
			}
		}

		protected async Task RunTestCaseAsync(PerformanceTestCase testCase)
		{
			PreConditionsCheckAndFix(testCase);

			Logger.LogInformation("In test case: " + testCase.TestCaseName);
			try
			{
				// Arrange
				Configuration.ImportOverwriteMode = testCase.OverwriteMode;
				Configuration.ImportNativeFileCopyMode = testCase.CopyMode;

				await SetupConfigurationAsync(testCase.TestCaseName).ConfigureAwait(false);

				IEnumerable<FieldMap> generatedFields =
					await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields: testCase.NumberOfMappedFields)
						.ConfigureAwait(false);

				Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(generatedFields).ToList());

				if (testCase.MapExtractedText)
				{
					IEnumerable<FieldMap> extractedTextMapping =
						await GetExtractedTextMappingAsync(SourceWorkspace.ArtifactID, DestinationWorkspace.ArtifactID).ConfigureAwait(false);
					Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(extractedTextMapping).ToArray());
				}

				Logger.LogInformation("Fields mapping ready");

				ConfigurationRdoId = await
					Rdos.CreateSyncConfigurationRdoAsync(SourceWorkspace.ArtifactID, Configuration, SyncLog)
						.ConfigureAwait(false);

				Logger.LogInformation("Configuration RDO created");

				SyncJobParameters args = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID,
					Guid.NewGuid());

				SyncRunner syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
					new NullAPM(), Logger, new TestSyncToggleProvider());

				Logger.LogInformation("Staring the job");

				// Act
				Stopwatch stopwatch = Stopwatch.StartNew();
				SyncJobState jobState = await syncRunner.RunAsync(args, User.ArtifactID).ConfigureAwait(false);

				stopwatch.Stop();
				var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
				TestTimes.Add(testCase.TestCaseName, elapsedTime);

				Logger.LogInformation("Elapsed time {0} s", elapsedTime.TotalSeconds.ToString("F", CultureInfo.InvariantCulture));

				RelativityObject jobHistory = await Rdos
					.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId, Configuration.JobHistory.TypeGuid)
					.ConfigureAwait(false);

				string aggregatedJobHistoryErrors = null;
				using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
				{
					aggregatedJobHistoryErrors =
						await objectManager.AggregateJobHistoryErrorMessagesAsync(SourceWorkspace.ArtifactID, jobHistory.ArtifactID).ConfigureAwait(false);

					aggregatedJobHistoryErrors.Should().BeNullOrEmpty("There should be no item level errors");
				}

				// Assert
				Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);

				int totalItems = (int)jobHistory["Total Items"].Value;
				int itemsTransferred = (int)jobHistory["Items Transferred"].Value;

				aggregatedJobHistoryErrors.Should().BeNullOrEmpty();
				itemsTransferred.Should().Be(totalItems);
				itemsTransferred.Should().Be(testCase.ExpectedItemsTransferred);
			}
			catch (Exception e)
			{
				Debugger.Break();
				Assert.Fail(e.Message);
			}
		}

		/// <summary>
		///	Creates needed objects in Relativity
		/// </summary>
		/// <returns></returns>
		public async Task SetupConfigurationAsync(string savedSearchName = "All Documents",
			IEnumerable<FieldMap> mapping = null, bool useRootWorkspaceFolder = true)
		{
			Logger.LogInformation("Setting up configuration");

			Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;
			Configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).ConfigureAwait(false);
			Configuration.DataSourceArtifactId = Configuration.SavedSearchArtifactId;
			IEnumerable<FieldMap> fieldsMapping = mapping ?? await GetDocumentIdentifierMappingAsync(SourceWorkspace.ArtifactID, DestinationWorkspace.ArtifactID).ConfigureAwait(false);
			Configuration.SetFieldMappings(fieldsMapping.ToList());

			Logger.LogInformation("Create Job History...");
			Configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now.ToString("yyyy MMMM dd HH.mm.ss.fff")}").ConfigureAwait(false);

			if (useRootWorkspaceFolder)
			{
				Configuration.DestinationFolderArtifactId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, DestinationWorkspace.ArtifactID).ConfigureAwait(false);
			}

			Logger.LogInformation("Configuration done");
		}

		/// <summary>
		/// Generates mapping with fields
		/// </summary>
		/// <param name="numberOfMappedFields">Limits the number of mapped fields. 0 means maps all fields</param>
		/// <returns>Mapping with generated fields</returns>
		public async Task<IEnumerable<FieldMap>> GetMappingAndCreateFieldsInDestinationWorkspaceAsync(int? numberOfMappedFields)
		{
			var sourceFields = await GetFieldsFromSourceWorkspaceAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);
			sourceFields = FilterTestCaseFields(sourceFields, numberOfMappedFields);

			IEnumerable<RelativityObject> destinationFields = await GetFieldsFromSourceWorkspaceAsync(DestinationWorkspace.ArtifactID).ConfigureAwait(false);
			destinationFields = FilterTestCaseFields(destinationFields, numberOfMappedFields);

			return sourceFields.Zip(destinationFields, (sourceField, destinationField) => new FieldMap
			{
				FieldMapType = FieldMapType.None,
				SourceField = new FieldEntry
				{
					DisplayName = sourceField["Name"].Value.ToString(),
					FieldIdentifier = sourceField.ArtifactID,
					IsIdentifier = false
				},
				DestinationField = new FieldEntry
				{
					DisplayName = sourceField["Name"].Value.ToString(),
					FieldIdentifier = destinationField.ArtifactID,
					IsIdentifier = false
				}
			});
		}

		private static IEnumerable<RelativityObject> FilterTestCaseFields(IEnumerable<RelativityObject> fields, int? numberOfMappedFields)
		{
			Regex wasGeneratedRegex = new Regex("^([0-9]+-)");

			var filteredfields = fields.Where(f => wasGeneratedRegex.IsMatch(f["Name"].Value.ToString())).OrderBy(x => x.Name).ToList();

			if (numberOfMappedFields != null)
			{
				filteredfields = filteredfields.Take(numberOfMappedFields.Value).ToList();
			}

			return filteredfields.ToList();
		}

		private async Task<IEnumerable<RelativityObject>> GetFieldsFromSourceWorkspaceAsync(int sourceWorkspaceArtifactId)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				List<RelativityObject> result = new List<RelativityObject>();

				QueryRequest query = PrepareGeneratedFieldsQueryRequest();
				int start = 0;
				QueryResult queryResult = null;

				do
				{
					const int batchSize = 100;
					queryResult = await objectManager
						.QueryAsync(sourceWorkspaceArtifactId, query, start, batchSize)
						.ConfigureAwait(false);

					result.AddRange(queryResult.Objects);
					start += queryResult.ResultCount;
				}
				while (result.Count < queryResult.TotalCount);

				return result;
			}
		}

		private QueryRequest PrepareGeneratedFieldsQueryRequest()
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID}",
				Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Field type" }, },
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}

		private void PreConditionsCheckAndFix(PerformanceTestCase testCase)
		{
			Logger.LogInformation("Pre-Condition checks started...");

			IList<FixResult> fixResults = new List<FixResult>();

			IEnumerable<IPreCondition> preConditions = new List<IPreCondition>()
			{
				new MassImportToggleOnPreCondition(),
				new IndexEnabledPreCondition(SourceWorkspace.ArtifactID),
				new IndexEnabledPreCondition(DestinationWorkspace.ArtifactID),
				new DataGridEnabledPreCondition(ServiceFactory, SourceWorkspace.ArtifactID),
				new DataGridEnabledPreCondition(ServiceFactory, DestinationWorkspace.ArtifactID),
				new WorkspaceDocCountPreCondition(ServiceFactory, SourceWorkspace.ArtifactID,
					testCase.ExpectedItemsTransferred),
				new WorkspaceDocCountPreCondition(ServiceFactory, DestinationWorkspace.ArtifactID,
					_wasDestinationForTestCaseCreated ? 0 : testCase.ExpectedItemsTransferred)
			};
			foreach (var preCondition in preConditions)
			{
				var isOk = preCondition.Check();
				Logger.LogInformation("Pre-Condition check: {name} is valid - {status}", preCondition.Name, isOk);
				if (!isOk)
				{
					Logger.LogInformation("Pre-Condition check {name} is invalid. Trying to fix...", preCondition.Name);
					fixResults.Add(preCondition.TryFix());
				}
			}

			IList<FixResult> fixErrors = fixResults.Where(x => !x.IsFixed).ToList();
			if (fixErrors.Any())
			{
				LogPreConditionChecksErrors(fixErrors);
				throw new Exception("Some of Pre-Condition checks failed. Check logs.");
			}

			Logger.LogInformation("Pre-Condition checks completed successfully...");
		}

		private void LogPreConditionChecksErrors(IList<FixResult> fixErrors)
		{
			foreach (var error in fixErrors)
			{
				Logger.LogError(error.Exception, "Pre-Condition: {name} fix failed.", error.PreConditionName);
			}
		}

		private async Task CleanUpWorkspacesAsync()
		{
			if(_workspaceType == WorkspaceType.ARM)
			{
				if (SourceWorkspace != null)
				{
					using (var manager = ServiceFactory.CreateProxy<IWorkspaceManager>())
					{
						await manager.DeleteAsync(SourceWorkspace).ConfigureAwait(false);
					}
				}

				if (DestinationWorkspace != null)
				{
					using (var manager = ServiceFactory.CreateProxy<IWorkspaceManager>())
					{
						await manager.DeleteAsync(DestinationWorkspace).ConfigureAwait(false);
					}
				}
			}
		}
	}
}
