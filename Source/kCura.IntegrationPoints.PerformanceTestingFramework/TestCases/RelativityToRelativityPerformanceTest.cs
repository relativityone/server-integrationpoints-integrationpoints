using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Factories;
using NUnit.Framework;
using System;
using System.Diagnostics;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.PerformanceTestingFramework
{
	[TestFixture]
	public class RelativityToRelativityPerformanceTest : RelativityProviderTemplate
	{
		private string _sourceConfigurationJson, _destinationConfigurationJson;
		private IIntegrationPointService _integrationPointService;
		private const int _ADMIN_USER_ID = 9;

		private const string _SOURCE_WORKSPACE_TOKEN = "SOURCE_WORKSPACE_ARTIFACT_ID";
		private const string _TARGET_WORKSPACE_TOKEN = "TARGET_WORKSPACE_ARTIFACT_ID";

		//nUnit doesn't allow usage of semicolons in parameters as it is used as delimiter for the actual parameters.
		//The problem is it also is not possible to escape them.
		//This means it may be required to have a backup solution for dealing with semicolon separators in RIP
		//- thus this token has been introduced. It will be replaced with actual semicolon as soon as the parameter is read.
		private const string _SEMICOLON_TOKEN= "{semicolon}";

		private readonly string _fieldMappingsJson;

		public RelativityToRelativityPerformanceTest() : base(Convert.ToInt32(TestContext.Parameters["SourceWorkspaceArtifactID"]), $"RelativityToRelativityPerformanceTest{DateTime.Now:yy-MM-dd HH-mm-ss}")
		{
			_fieldMappingsJson = TestContext.Parameters["FieldMappingsJSON"];

			_sourceConfigurationJson = TestContext.Parameters["SourceConfigurationJSON"]
				.Replace(_SOURCE_WORKSPACE_TOKEN, TestContext.Parameters["SourceWorkspaceArtifactID"])
				.Replace(_SEMICOLON_TOKEN, ";");

			_destinationConfigurationJson = TestContext.Parameters["DestinationConfigurationJSON"]
				.Replace(_SEMICOLON_TOKEN, ";");
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			ResolveServices();
		}

		/// <summary>
		/// This very test is meant to be run via NUnit-Console runner version 3+. It is expecting a number of parameters, which are:
		/// 1) SourceWorkspaceArtifactID,
		/// 2) FieldMappingsJSON,
		/// 3) SourceConfigurationJSON,
		/// 4) DestinationConfigurationJSON
		/// </summary>
		[Category("PerformanceTest")]
		[Test]
		public void PerformanceTest()
		{
			//Arrange
			_sourceConfigurationJson = _sourceConfigurationJson.Replace(_TARGET_WORKSPACE_TOKEN, SourceWorkspaceArtifactId.ToString());
			_destinationConfigurationJson = _destinationConfigurationJson.Replace(_TARGET_WORKSPACE_TOKEN, SourceWorkspaceArtifactId.ToString());

			IntegrationPointModel integrationPointModel = PrepareIntegrationPointsModel();
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationPointModel);
			var testDurationStopWatch = new Stopwatch();

			//Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointToLeavePendingState(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);

			//start the timer only after the job is picked up by an agent
			testDurationStopWatch.Start();
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);
			testDurationStopWatch.Stop();

			//it is yet to be decided on how to return the time it took for the job to finish back to the Performance Testing Framework.
			Console.WriteLine($"PerformanceTestingFramework, transfer duration -> {Math.Round(testDurationStopWatch.Elapsed.TotalSeconds, 2)}s");
		}

		private IntegrationPointModel PrepareIntegrationPointsModel()
		{
			return new IntegrationPointModel
			{

				Map = _fieldMappingsJson,
				SourceConfiguration = _sourceConfigurationJson,
				Destination = _destinationConfigurationJson,

				SourceProvider = RelativityProvider.ArtifactId,
				DestinationProvider = DestinationProvider.ArtifactId,
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = false,
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private void ResolveServices()
		{
			RepositoryFactory = Container.Resolve<IRepositoryFactory>();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
		}

	}
}