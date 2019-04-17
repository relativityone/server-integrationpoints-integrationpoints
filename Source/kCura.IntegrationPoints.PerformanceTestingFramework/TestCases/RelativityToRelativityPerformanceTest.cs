﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.PerformanceTestingFramework.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.PerformanceTestingFramework.TestCases
{
	[TestFixture]
	public class RelativityToRelativityPerformanceTest : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private const int _ADMIN_USER_ID = 9;

		private readonly string _fieldMappingsJson;
		private readonly bool _enableDataGrid;

		public RelativityToRelativityPerformanceTest() : base(
			Convert.ToInt32(TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("SourceWorkspaceArtifactID")),
			TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("TargetWorkspaceName"),
			TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("TemplateWorkspaceName"))
		{
			_enableDataGrid = Convert.ToBoolean(TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("EnableDataGrid"));
			_fieldMappingsJson = File.ReadAllText(TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("FieldMappingsJSONPath"));
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
		}

		/// <summary>
		/// This very test is meant to be run via NUnit-Console runner version 3+. It is expecting a number of parameters, which are:
		/// 1) SourceWorkspaceArtifactID,
		/// 2) FieldMappingsJSON,
		/// 3) ImportNativeFileCopyModeEnum - DoNotImportNativeFiles, SetFileLinks, CopyFiles
		/// 4) ImageImport - True, False
		/// 5) ImportNativeFile - True, False
		/// </summary>
		[Category("PerformanceTest")]
		[Test]
		public void PerformanceTest()
		{
			double elapsedTime = -1;
			try
			{
				//Arrange
				if (_enableDataGrid)
				{
					Workspace.EnableDataGrid(TargetWorkspaceArtifactId);
				}

				IntegrationPointModel integrationPointModel = PrepareIntegrationPointsModel(TargetWorkspaceArtifactId);
				IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationPointModel);
				var testDurationStopWatch = new Stopwatch();

				//Act
				_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID);
				Status.WaitForIntegrationPointToLeavePendingState(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);

				//start the timer only after the job is picked up by an agent
				testDurationStopWatch.Start();
				Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);
				testDurationStopWatch.Stop();

				elapsedTime = testDurationStopWatch.Elapsed.TotalSeconds;

				var queryRequest = new QueryRequest
				{
					Sorts = new[] { new Sort() { Direction = SortEnum.Descending, FieldIdentifier = new FieldRef { Name = "ArtifactID" } } }
				};
				ResultSet<JobHistory> resultSet = ObjectManager.Query<JobHistory>(queryRequest, 0, 1);

				if (resultSet.ResultCount == 0)
				{
					elapsedTime = -1;
				}
				else
				{
					JobHistory jobHistoryObject = resultSet.Items.First();
					Console.WriteLine($"Job status: {jobHistoryObject.JobStatus.Name}");
					if (jobHistoryObject.JobStatus.Name != JobStatusChoices.JobHistoryCompleted.Name)
					{
						elapsedTime = -1;
					}
				}
			}
			catch (Exception ex)
			{
				elapsedTime = -1;
				Console.WriteLine($"Exception occured ({ex.GetType()}) {ex.Message}: {ex.StackTrace}");
				throw;
			}
			finally
			{
				Console.WriteLine($"PerformanceTest - RIP job duration -> {Math.Round(elapsedTime, 2)}s");

				/* <<== IMPORTANT ==>>
				 * This is the place, where we write the result in seconds to stdout.
				 * Grazyna then reads this output and looks for number between '<<<<\t' '\t>>>>' tags.
				 * The code lies in `Grazyna.Core.Utilities.OutputParser.ConsoleRunnerOutputResultsParser`.
				 * The exact regex that the output is matched against is: "<<<<\t([\d\.+-]+)\t>>>>".
				 * Please consider that when changing the code.
				 */
				Console.WriteLine($"<<<<\t{elapsedTime}\t>>>>");
				/* <<==    END    ==>> */
			}
		}

		private IntegrationPointModel PrepareIntegrationPointsModel(int targetWorkspaceId)
		{
			return new IntegrationPointModel
			{
				Map = _fieldMappingsJson,
				SourceConfiguration = SourceConfiguration(targetWorkspaceId, SavedSearchArtifactId),
				Destination = DestinationConfiguration(targetWorkspaceId),

				SourceProvider = RelativityProvider.ArtifactId,
				DestinationProvider = RelativityDestinationProviderArtifactId,
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yyyy-MM-dd HH-mm}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = false,
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private string DestinationConfiguration(int targetWorkspaceId)
		{
			ImportNativeFileCopyModeEnum importNativeFileCopyMode;
			Enum.TryParse(TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("ImportNativeFileCopyModeEnum"), out importNativeFileCopyMode);

			bool imageImport;
			bool.TryParse(TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("ImageImport"), out imageImport);

			bool importNativeFile;
			bool.TryParse(TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("ImportNativeFile"), out importNativeFile);

			var destinationConfiguration = new ImportSettings()
			{
				ArtifactTypeId = 10,
				DestinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
				CaseArtifactId = targetWorkspaceId,
				FederatedInstanceArtifactId = null,
				CreateSavedSearchForTagging = false,
				DestinationFolderArtifactId = 1003697,
				ProductionImport = false,
				Provider = "Relativity",
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				UseDynamicFolderPath = false,
				ImagePrecedence = new Domain.Models.ProductionDTO[0],
				ProductionPrecedence = null,
				IncludeOriginalImages = false,
				IdentifierField = "Control Number",
				MoveExistingDocuments = false,
				ExtractedTextFieldContainsFilePath = false,
				ExtractedTextFileEncoding = "utf-16",
				EntityManagerFieldContainsLink = true,
				FieldOverlayBehavior = "Use Field Settings",
				ImportNativeFile = importNativeFile,
				ImportNativeFileCopyMode = importNativeFileCopyMode,
				ImageImport = imageImport
			};

			var serializer = new JSONSerializer();
			return serializer.Serialize(destinationConfiguration);

		}

		private string SourceConfiguration(int targetWorkspaceId, int savedSearchArtifactId)
		{
			int sourceWorkspaceId;
			int.TryParse(TestContextParametersHelper.GetParameterFromTestContextOrAuxilaryFile("SourceWorkspaceArtifactID"), out sourceWorkspaceId);

			var sourceConfiguration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = sourceWorkspaceId,
				TargetWorkspaceArtifactId = targetWorkspaceId,
				SavedSearchArtifactId = savedSearchArtifactId,
				FederatedInstanceArtifactId = null,
				TypeOfExport = Core.Contracts.Configuration.SourceConfiguration.ExportType.SavedSearch
			};

			var serializer = new JSONSerializer();
			return serializer.Serialize(sourceConfiguration);
		}
	}
}