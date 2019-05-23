using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class JobHistoryErrorRepositoryTests : RelativityProviderTemplate
	{
		private IJobHistoryErrorRepository _instance;
		private JobHistory _jobHistory;

		public JobHistoryErrorRepositoryTests() : base("JobHistoryErrorRepositoryTests", null)
		{
		}

		public override void TestSetup()
		{
			var repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_instance = repositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"JobHistoryErrorRepositoryTests{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Create an Integration Point and assign a Job History
			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			_jobHistory = CreateJobHistoryOnIntegrationPoint(
				integrationPointCreated.ArtifactID, 
				batchInstance, 
				JobTypeChoices.JobHistoryRun, 
				JobStatusChoices.JobHistoryCompletedWithErrors, 
				true);
		}

		[IdentifiedTestCase("d725c6a3-f721-41ed-9759-32076e4776dd", JobHistoryErrorDTO.Choices.ErrorType.Values.Item)]
		[IdentifiedTestCase("8c585500-9e75-49fb-aeb3-d2c0d7875e76", JobHistoryErrorDTO.Choices.ErrorType.Values.Job)]
		public void RetrieveJobHistoryErrorArtifactIds_NoError(JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			//act
			ICollection<int> result = _instance.RetrieveJobHistoryErrorArtifactIds(_jobHistory.ArtifactId, errorType);

			//assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
		}


		private static readonly TestCaseData[] RetrieveJobHistoryErrorSources = new TestCaseData[]
		{
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem}).WithId("70BDFBD8-36A4-4CD3-9AA6-DFB16BE6A965"),
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorInProgress, ErrorTypeChoices.JobHistoryErrorItem}).WithId("3AF3DFFB-9A5B-4B20-A454-8E8794996D7F"),
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorItem}).WithId("10177CE1-700C-4100-935A-16EB2F6E77F6"),
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorExpired, ErrorTypeChoices.JobHistoryErrorItem}).WithId("64DC8730-1880-422D-9F0F-7718BA15DEFF"),
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorJob}).WithId("DE3845C3-B73C-4147-8C43-9CDADF93AC88"),
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorInProgress, ErrorTypeChoices.JobHistoryErrorJob}).WithId("B8BACFF1-E44A-4FBC-BD69-AE8CE664EA33"),
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorJob}).WithId("56BAFD0F-7C07-40C3-8C66-82FEA13056F8"),
			new TestCaseData(new object[] {ErrorStatusChoices.JobHistoryErrorExpired, ErrorTypeChoices.JobHistoryErrorJob}).WithId("BCBDD258-0A31-4046-A936-C34057B9880E"),
		};

		[IdentifiedTestCaseSource("B1BC4D8A",nameof(RetrieveJobHistoryErrorSources))]
		public void RetrieveJobHistoryErrorArtifactIds(Choice errorStatus, Choice errorType)
		{
			// arrange
			List<int> jobHistoryArtifactId = CreateJobHistoryError(_jobHistory.ArtifactId, errorStatus, errorType);
			JobHistoryErrorDTO.Choices.ErrorType.Values type = errorType == ErrorTypeChoices.JobHistoryErrorItem
				? JobHistoryErrorDTO.Choices.ErrorType.Values.Item
				: JobHistoryErrorDTO.Choices.ErrorType.Values.Job;

			// act
			ICollection<int> result = _instance.RetrieveJobHistoryErrorArtifactIds(_jobHistory.ArtifactId, type);

			// assert
			Assert.IsNotNull(result);
			CollectionAssert.AreEqual(jobHistoryArtifactId, result);
		}

		[IdentifiedTestCase("690a39d4-f28e-43c7-9e40-bf53016242dd", JobHistoryErrorDTO.Choices.ErrorType.Values.Item)]
		[IdentifiedTestCase("db971f8e-d398-4291-b4cd-4209f54da0c3", JobHistoryErrorDTO.Choices.ErrorType.Values.Job)]
		public void RetrieveJobHistoryErrorIdsAndSourceUniqueIds_NoError(JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			//act
			IDictionary<int, string> result = _instance.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(_jobHistory.ArtifactId, errorType);

			//assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
		}

		[IdentifiedTestCaseSource("6F08B843",nameof(RetrieveJobHistoryErrorSources))]
		public void RetrieveJobHistoryErrorIdsAndSourceUniqueIds(Choice errorStatus, Choice errorType)
		{
			// arrange
			List<int> jobHistoryArtifactId = CreateJobHistoryError(_jobHistory.ArtifactId, errorStatus, errorType);
			JobHistoryErrorDTO.Choices.ErrorType.Values type = errorType == ErrorTypeChoices.JobHistoryErrorItem
				? JobHistoryErrorDTO.Choices.ErrorType.Values.Item
				: JobHistoryErrorDTO.Choices.ErrorType.Values.Job;

			// act
			ICollection<int> result = _instance.RetrieveJobHistoryErrorArtifactIds(_jobHistory.ArtifactId, type);

			// assert
			Assert.IsNotNull(result);
			CollectionAssert.AreEqual(jobHistoryArtifactId, result);
		}
	}
}