using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	public class SmallJobsTests : PerformanceTestBase
	{
		private long _elapsedTime;
		private string _armedWorkspaceFilename;

		[SetUp]
		public void SetUp()
		{
			ARMHelper.EnableAgents();
			Configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			Configuration.ImportNativeFileCopyMode = ImportNativeFileCopyMode.SetFileLinks;
		}

		[TestCase("SmallSaltPepperWorkspace.zip", true, null, 5)]
		public async Task RunTestCase(string armedWorkspaceFileName, bool mapExtractedText,
			int? numberOfMappedFields, int expectedTransferredItems)
		{
			_armedWorkspaceFilename = armedWorkspaceFileName;
			try
			{
				// Arrange
				string filePath = await StorageHelper
					.DownloadFileAsync(armedWorkspaceFileName, Path.GetTempPath()).ConfigureAwait(false);
//
				int sourceWorkspaceIdArm =
					await ARMHelper.RestoreWorkspaceAsync(filePath, Environment).ConfigureAwait(false);

//				const int sourceWorkspaceId = 1021707;

				await SetupConfigurationAsync(sourceWorkspaceIdArm).ConfigureAwait(false);

				ConfigurationRdoId = await
					Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
						.ConfigureAwait(false);

				IEnumerable<FieldMap> generatedFields =
					await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields)
						.ConfigureAwait(false);

				Configuration.FieldsMapping = Configuration.FieldsMapping.Concat(generatedFields).ToArray();

				if (mapExtractedText)
				{
					IEnumerable<FieldMap> extractedTextMapping =
						await GetGetExtractedTextMapping().ConfigureAwait(false);
					Configuration.FieldsMapping = Configuration.FieldsMapping.Concat(extractedTextMapping).ToArray();
				}

				ConfigurationRdoId = await
					Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
						.ConfigureAwait(false);


				SyncJobParameters args = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID,
					Configuration.JobHistoryId);


				SyncRunner syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
					new NullAPM(), new ConsoleLogger());

				// Act
				Stopwatch stopwatch = Stopwatch.StartNew();
				SyncJobState jobState = await syncRunner.RunAsync(args, User.ArtifactID).ConfigureAwait(false);

				stopwatch.Stop();
				_elapsedTime = stopwatch.ElapsedMilliseconds;

				RelativityObject jobHistory = await Rdos
					.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryId)
					.ConfigureAwait(false);

				// Assert
				Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);

				int totalItems = (int)jobHistory["Total Items"].Value;
				int itemsTranferred = (int)jobHistory["Items Transferred"].Value;

				itemsTranferred.Should().Be(totalItems);
				itemsTranferred.Should().Be(expectedTransferredItems);
			}
			catch (Exception e)
			{
				Debugger.Break();
				Assert.Fail(e.Message);
			}
		}

		[TearDown]
		public async Task TearDown()
		{
			if (SourceWorkspace != null)
			{
				using (var manager = ServiceFactory.CreateProxy<IWorkspaceManager>())
				{
					// ReSharper disable once AccessToDisposedClosure - False positive. We're awaiting all tasks, so we can be sure dispose will be done after each call is handled
					await manager.DeleteAsync(SourceWorkspace).ConfigureAwait(false);
				}
			}

			File.AppendAllText(AppSettings.PerformanceResultsFilePath, $"{_armedWorkspaceFilename};{_elapsedTime}\n");
		}

		public int ConfigurationRdoId { get; set; }
	}
}
