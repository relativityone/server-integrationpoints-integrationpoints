using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Transformer
{
	[TestFixture]
	public class JobHistoryErrorTransformerTests
	{
	    private IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> _testInstance;
        private IRepositoryFactory _repositoryFactory;
	    private IArtifactGuidRepository _artifactGuidRepository;

        private const int _workspaceArtifactId = 1687354;
	    private const int _errorStatusNewChoiceId = 4687356;
		private const int _errorStatusExpiredChoiceId = 7638354;
        private const int _errorTypeJobChoiceId = 8796347;
		private const int _errorTypeItemChoiceId = 7138574;
        private readonly Guid _errorStatusNewChoiceGuid = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");
		private readonly Guid _errorStatusExpiredChoiceGuid = new Guid("AF01A8FA-B419-49B1-BD71-25296E221E57");
		private readonly Guid _errorTypeJobChoiceGuid = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");
		private readonly Guid _errorTypeItemChoiceGuid = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");

		[TestFixtureSetUp]
		public void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
            _testInstance = new JobHistoryErrorTransformer(_repositoryFactory, _workspaceArtifactId);
        }

        [Test]
        public void ConvertToDtoTest()
        {
			// ARRANGE
			string expectedName = "My Job History Error";
			JobHistoryError expectedJobHistoryError = CreateMockedJobHistoryError(expectedName);

            _repositoryFactory.GetArtifactGuidRepository(_workspaceArtifactId).Returns(_artifactGuidRepository);

            var errorStatuschoiceDictionary = new Dictionary<int, Guid> {{ _errorStatusNewChoiceId, _errorStatusNewChoiceGuid } };
            _artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorStatusNewChoiceId))
                .Returns(errorStatuschoiceDictionary);
			var errorTypechoiceDictionary = new Dictionary<int, Guid> { { _errorTypeJobChoiceId, _errorTypeJobChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorTypeJobChoiceId))
				.Returns(errorTypechoiceDictionary);

			// ACT
			JobHistoryErrorDTO resultDto = _testInstance.ConvertToDto(expectedJobHistoryError);

            // ASSERT
            Assert.IsNotNull(resultDto);
            Assert.AreEqual(expectedJobHistoryError.ArtifactId, resultDto.ArtifactId);
            Assert.AreEqual(expectedJobHistoryError.Error, resultDto.Error);
            Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorStatus.Values.New, resultDto.ErrorStatus);
            Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorType.Values.Job, resultDto.ErrorType);
            Assert.AreEqual(expectedJobHistoryError.JobHistory, resultDto.JobHistory);
            Assert.AreEqual(expectedJobHistoryError.Name, resultDto.Name);
            Assert.AreEqual(expectedJobHistoryError.SourceUniqueID, resultDto.SourceUniqueID);
            Assert.AreEqual(expectedJobHistoryError.JobHistory, resultDto.JobHistory);
            Assert.AreEqual(expectedJobHistoryError.StackTrace, resultDto.StackTrace);
            Assert.AreEqual(expectedJobHistoryError.TimestampUTC, resultDto.TimestampUTC);
        }

	    [Test]
	    public void ConvertMultipleToDtoTest()
	    {
            // ARRANGE
            string expectedName1 = "My Job History Error 1";
            string expectedName2 = "My Job History Error  2";

            JobHistoryError expectedJobHistoryError1 = CreateMockedJobHistoryError(expectedName1, _errorTypeJobChoiceId, _errorStatusNewChoiceId);
			JobHistoryError expectedJobHistoryError2 = CreateMockedJobHistoryError(expectedName2, _errorTypeItemChoiceId, _errorStatusExpiredChoiceId);

            _repositoryFactory.GetArtifactGuidRepository(_workspaceArtifactId).Returns(_artifactGuidRepository);

			var errorStatuschoiceDictionary1 = new Dictionary<int, Guid> { { _errorStatusNewChoiceId, _errorStatusNewChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorStatusNewChoiceId))
				.Returns(errorStatuschoiceDictionary1);
			var errorTypechoiceDictionary1 = new Dictionary<int, Guid> { { _errorTypeJobChoiceId, _errorTypeJobChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorTypeJobChoiceId))
				.Returns(errorTypechoiceDictionary1);
			var errorStatuschoiceDictionary2 = new Dictionary<int, Guid> { { _errorStatusExpiredChoiceId, _errorStatusExpiredChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorStatusExpiredChoiceId))
				.Returns(errorStatuschoiceDictionary2);
			var errorTypechoiceDictionary2 = new Dictionary<int, Guid> { { _errorTypeItemChoiceId, _errorTypeItemChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorTypeItemChoiceId))
				.Returns(errorTypechoiceDictionary2);

			// ACT
			List<JobHistoryErrorDTO> resultDtos = _testInstance.ConvertToDto(new [] { expectedJobHistoryError1, expectedJobHistoryError2 });

            // ASSERT
            Assert.IsNotNull(resultDtos);
            Assert.AreEqual(2, resultDtos.Count);
            //  This test only concentrates on multiple conversion, thus only few fields are tested
            Assert.AreEqual(expectedName1, resultDtos[0].Name);
            Assert.AreEqual(expectedName2, resultDtos[1].Name);
			Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorStatus.Values.New, resultDtos[0].ErrorStatus);
			Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorStatus.Values.Expired, resultDtos[1].ErrorStatus);
			Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorType.Values.Job, resultDtos[0].ErrorType);
			Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorType.Values.Item, resultDtos[1].ErrorType);
		}

        public JobHistoryError CreateMockedJobHistoryError(string name, int errorTypeChoiceId = _errorTypeJobChoiceId, int errorStatusChoiceId = _errorStatusNewChoiceId)
		{
			return new JobHistoryError()
			{
				ArtifactId = 6587693,
				Error = "Just your friendly neighborhood error",
				ErrorStatus = new Choice(errorStatusChoiceId, ""),
				ErrorType = new Choice(errorTypeChoiceId, ""),
				JobHistory = 1041673,
				Name = name,
				SourceUniqueID = "jimCarrey",
				StackTrace = "Superlong stacktrace that you'll never figure out.",
				TimestampUTC = DateTime.UtcNow
			};
		}
	}
}
