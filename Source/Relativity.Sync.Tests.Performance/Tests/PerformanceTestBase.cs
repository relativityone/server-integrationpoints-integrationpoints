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
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
	public class PerformanceTestBase : SystemTest
	{
		protected int _sourceWorkspaceId;
		protected int _destinationWorkspaceId;

		protected readonly IDictionary<string, TimeSpan> _testTimes = new ConcurrentDictionary<string, TimeSpan>();

		private readonly WorkspaceType _workspaceType;

		public string ArmedSourceWorkspaceFileName { get; }
		public string ArmedDestinationWorkspaceFileName { get; }

		public string SourceWorkspaceName { get; }
		public string DestinationWorkspaceName { get; }

		public ApiComponent Component { get; }
		public ARMHelper ARMHelper { get; }
		public AzureStorageHelper StorageHelper { get; }

		public WorkspaceRef TargetWorkspace { get; set; }

		public WorkspaceRef SourceWorkspace { get; set; }

		internal ConfigurationStub Configuration { get; set; }

		public int ConfigurationRdoId { get; set; }

		public PerformanceTestBase(WorkspaceType workspaceType, string sourceWorkspace, string destinationWorkspace)
		{
			RelativityFacade.Instance.RelyOn<ApiComponent>();

			Component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			_workspaceType = workspaceType;
			if (_workspaceType == WorkspaceType.ARM)
			{
				ArmedSourceWorkspaceFileName = sourceWorkspace;
				ArmedDestinationWorkspaceFileName = destinationWorkspace;

				StorageHelper = AzureStorageHelper.CreateFromTestConfig();

				ARMHelper = ARMHelper.CreateInstance();
			}
			else
			{
				SourceWorkspaceName = sourceWorkspace;
				DestinationWorkspaceName = destinationWorkspace;
			}

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
			Configuration.SetEmailNotificationRecipients(string.Empty);
		}

		[OneTimeSetUp]
		public async Task SetUp()
		{
			if(_workspaceType == WorkspaceType.ARM)
			{
				ARMHelper.EnableAgents();

				_sourceWorkspaceId = await RestoreWorkspaceAsync(ArmedSourceWorkspaceFileName).ConfigureAwait(false);

				if (!string.IsNullOrEmpty(ArmedDestinationWorkspaceFileName))
				{
					_destinationWorkspaceId = await RestoreWorkspaceAsync(ArmedDestinationWorkspaceFileName).ConfigureAwait(false);
				}
			}
			else
			{
				_sourceWorkspaceId = await Environment.GetWorkspaceArtifactIdByName(SourceWorkspaceName).ConfigureAwait(false);

				if (!string.IsNullOrEmpty(DestinationWorkspaceName))
				{
					_destinationWorkspaceId = await Environment.GetWorkspaceArtifactIdByName(DestinationWorkspaceName).ConfigureAwait(false);
				}
			}
		}

		private async Task<int> RestoreWorkspaceAsync(string armedWorkspaceFileName)
		{
			string filePath = await StorageHelper
				.DownloadFileAsync(armedWorkspaceFileName, Path.GetTempPath()).ConfigureAwait(false);

			Logger.LogInformation($"ARMed workspace saved locally in {filePath}");
			return await ARMHelper.RestoreWorkspaceAsync(filePath, Environment).ConfigureAwait(false);
		}
		
		[OneTimeTearDown]
		public async Task OneTimeTearDown()
		{
			await CleanUpWorkspacesAsync().ConfigureAwait(false);

			if (!string.IsNullOrEmpty(AppSettings.PerformanceResultsFilePath))
			{
				File.WriteAllLines(AppSettings.PerformanceResultsFilePath,
					_testTimes.Select(pair => $"{pair.Key};{pair.Value.TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture)}\n"));
			}
		}

		protected async Task RunTestCaseAsync(PerformanceTestCase testCase)
		{
			Logger.LogInformation("In test case: " + testCase.TestCaseName);
			try
			{
				// Arrange
				Configuration.ImportOverwriteMode = testCase.OverwriteMode;
				Configuration.ImportNativeFileCopyMode = testCase.CopyMode;

				var destinationWorkspaceId = Configuration.ImportOverwriteMode == ImportOverwriteMode.AppendOnly ? null : (int?)_destinationWorkspaceId;
				await SetupConfigurationAsync(_sourceWorkspaceId, destinationWorkspaceId, testCase.TestCaseName).ConfigureAwait(false);

				IEnumerable<FieldMap> generatedFields =
					await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields: null)
						.ConfigureAwait(false);

				Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(generatedFields).ToList());

				if (testCase.MapExtractedText)
				{
					IEnumerable<FieldMap> extractedTextMapping =
						await GetExtractedTextMappingAsync(SourceWorkspace.ArtifactID, TargetWorkspace.ArtifactID).ConfigureAwait(false);
					Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(extractedTextMapping).ToArray());
				}

				Logger.LogInformation("Fields mapping ready");

				ConfigurationRdoId = await
					Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
						.ConfigureAwait(false);

				Logger.LogInformation("Configuration RDO created");

				SyncJobParameters args = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID,
					Configuration.JobHistoryArtifactId);

				SyncRunner syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
					new NullAPM(), TestLogHelper.GetLogger());

				Logger.LogInformation("Staring the job");

				// Act
				Stopwatch stopwatch = Stopwatch.StartNew();
				SyncJobState jobState = await syncRunner.RunAsync(args, User.ArtifactID).ConfigureAwait(false);

				stopwatch.Stop();
				var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
				_testTimes.Add(testCase.TestCaseName, elapsedTime);

				Logger.LogInformation("Elapsed time {0} s", elapsedTime.TotalSeconds.ToString("F", CultureInfo.InvariantCulture));

				RelativityObject jobHistory = await Rdos
					.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId)
					.ConfigureAwait(false);

				// Assert
				Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);

				int totalItems = (int)jobHistory["Total Items"].Value;
				int itemsTranferred = (int)jobHistory["Items Transferred"].Value;

				itemsTranferred.Should().Be(totalItems);
				itemsTranferred.Should().Be(testCase.ExpectedItemsTransferred);
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
		public async Task SetupConfigurationAsync(int? sourceWorkspaceId = null, int? targetWorkspaceId = null, string savedSearchName = "All Documents",
			IEnumerable<FieldMap> mapping = null, bool useRootWorkspaceFolder = true)
		{
			Logger.LogInformation("Setting up configuration");
			if (sourceWorkspaceId == null)
			{
				SourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			}
			else
			{
				SourceWorkspace = await Environment.GetWorkspaceAsync(sourceWorkspaceId.Value).ConfigureAwait(false);
				await Environment.CreateFieldsInWorkspaceAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);
			}

			if (targetWorkspaceId == null)
			{
				Logger.LogInformation("Creating target workspace");
				TargetWorkspace = await Environment.CreateWorkspaceWithFieldsAsync(templateWorkspaceName: SourceWorkspace.Name).ConfigureAwait(false);
			}
			else
			{
				TargetWorkspace = await Environment.GetWorkspaceAsync(targetWorkspaceId.Value).ConfigureAwait(false);
			}

			Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			Configuration.DestinationWorkspaceArtifactId = TargetWorkspace.ArtifactID;
			Configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).ConfigureAwait(false);
			Configuration.DataSourceArtifactId = Configuration.SavedSearchArtifactId;
			IEnumerable<FieldMap> fieldsMapping = mapping ?? await GetIdentifierMappingAsync(SourceWorkspace.ArtifactID, TargetWorkspace.ArtifactID).ConfigureAwait(false);
			Configuration.SetFieldMappings(fieldsMapping.ToList());
			Configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now.ToString("yyyy MMMM dd HH.mm.ss.fff")}").ConfigureAwait(false);

			if (useRootWorkspaceFolder)
			{
				Configuration.DestinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, TargetWorkspace.ArtifactID).ConfigureAwait(false);
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


			IEnumerable<RelativityObject> destinationFields = await GetFieldsFromSourceWorkspaceAsync(TargetWorkspace.ArtifactID).ConfigureAwait(false);
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

			var filteredfields = fields.Where(f => wasGeneratedRegex.IsMatch(f["Name"].Value.ToString()));

			if (numberOfMappedFields != null)
			{
				filteredfields = filteredfields.Take(numberOfMappedFields.Value);
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

		private async Task CleanUpWorkspacesAsync()
		{
			if(_workspaceType == WorkspaceType.ARM)
			{
				if (SourceWorkspace != null)
				{
					using (var manager = ServiceFactory.CreateProxy<IWorkspaceManager>())
					{
						// ReSharper disable once AccessToDisposedClosure - False positive. We're awaiting all tasks, so we can be sure dispose will be done after each call is handled
						await manager.DeleteAsync(SourceWorkspace).ConfigureAwait(false);
					}
				}

				if (TargetWorkspace != null)
				{
					using (var manager = ServiceFactory.CreateProxy<IWorkspaceManager>())
					{
						// ReSharper disable once AccessToDisposedClosure - False positive. We're awaiting all tasks, so we can be sure dispose will be done after each call is handled
						await manager.DeleteAsync(TargetWorkspace).ConfigureAwait(false);
					}
				}
			}
		}
	}
}
