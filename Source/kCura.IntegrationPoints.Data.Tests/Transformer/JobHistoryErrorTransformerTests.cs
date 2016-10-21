using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
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
		private const int _errorStatusInProgressChoiceId = 6981326;
		private const int _errorStatusRetriedChoiceId = 8665454;
		private const int _errorTypeJobChoiceId = 8796347;
		private const int _errorTypeItemChoiceId = 7138574;
		private const int _jobHistoryArtifactId = 1018479;
        private static readonly Guid _errorStatusNewChoiceGuid = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");
		private static readonly Guid _errorStatusInProgressChoiceGuid = new Guid("E5EBD98C-C976-4FA2-936F-434E265EA0AA");
		private static readonly Guid _errorStatusExpiredChoiceGuid = new Guid("AF01A8FA-B419-49B1-BD71-25296E221E57");
		private static readonly Guid _errorStatusRetriedChoiceGuid = new Guid("7D3D393D-384F-434E-9776-F9966550D29A");
		private static readonly Guid _errorTypeJobChoiceGuid = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");
		private static readonly Guid _errorTypeItemChoiceGuid = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");

		[OneTimeSetUp]
		public void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
			_repositoryFactory.GetArtifactGuidRepository(_workspaceArtifactId).Returns(_artifactGuidRepository);
			_testInstance = new JobHistoryErrorTransformer(_repositoryFactory, _workspaceArtifactId);
        }

        [Test]
        public void ConvertToDtoTest()
        {
			// ARRANGE
			string expectedName = "My Job History Error";
			JobHistoryError expectedJobHistoryError = CreateMockedJobHistoryError(expectedName);

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

			var errorStatusChoiceDictionary1 = new Dictionary<int, Guid> { { _errorStatusNewChoiceId, _errorStatusNewChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorStatusNewChoiceId))
				.Returns(errorStatusChoiceDictionary1);
			var errorTypeChoiceDictionary1 = new Dictionary<int, Guid> { { _errorTypeJobChoiceId, _errorTypeJobChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorTypeJobChoiceId))
				.Returns(errorTypeChoiceDictionary1);
			var errorStatusChoiceDictionary2 = new Dictionary<int, Guid> { { _errorStatusExpiredChoiceId, _errorStatusExpiredChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorStatusExpiredChoiceId))
				.Returns(errorStatusChoiceDictionary2);
			var errorTypeChoiceDictionary2 = new Dictionary<int, Guid> { { _errorTypeItemChoiceId, _errorTypeItemChoiceGuid } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == _errorTypeItemChoiceId))
				.Returns(errorTypeChoiceDictionary2);

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

		[Test]
		public void ConvertArtifactDtoToDtoTest()
		{
			// ARRANGE
			string expectedName = "My Job History Error";
			ArtifactDTO expectedArtifactDto = CreateMockedJobHistoryErrorArtifactDto(expectedName);

			Dictionary<Guid, int> errorStatusChoiceDictionary = new Dictionary<Guid, int> {
				{ _errorStatusNewChoiceGuid, _errorStatusNewChoiceId },
				{ _errorStatusExpiredChoiceGuid, _errorStatusExpiredChoiceId },
				{ _errorStatusInProgressChoiceGuid, _errorStatusInProgressChoiceId },
				{ _errorStatusRetriedChoiceGuid, _errorStatusRetriedChoiceId }
			};
			_artifactGuidRepository.GetArtifactIdsForGuids(JobHistoryErrorDTO.Choices.ErrorStatus.GuidList).Returns(errorStatusChoiceDictionary);
			Dictionary<Guid, int> errorTypeChoiceDictionary = new Dictionary<Guid, int>
			{
				{ _errorTypeJobChoiceGuid, _errorTypeJobChoiceId },
				{ _errorTypeItemChoiceGuid, _errorTypeItemChoiceId }
			};
			_artifactGuidRepository.GetArtifactIdsForGuids(JobHistoryErrorDTO.Choices.ErrorType.GuidList).Returns(errorTypeChoiceDictionary);
			IDictionary<string, ArtifactFieldDTO> fieldMapping = expectedArtifactDto.Fields.ToDictionary(k => k.Name, v => v);

			// ACT
			JobHistoryErrorDTO resultDto = _testInstance.ConvertArtifactDtoToDto(expectedArtifactDto);

			// ASSERT
			Assert.IsNotNull(resultDto);
			Assert.AreEqual(expectedArtifactDto.ArtifactId, resultDto.ArtifactId);
			Assert.AreEqual((string)fieldMapping["Error"].Value, resultDto.Error);
			Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorStatus.Values.New, resultDto.ErrorStatus);
			Assert.AreEqual(JobHistoryErrorDTO.Choices.ErrorType.Values.Job, resultDto.ErrorType);
			Assert.AreEqual(_jobHistoryArtifactId, resultDto.JobHistory);
			Assert.AreEqual(expectedName, resultDto.Name);
			Assert.AreEqual("jimCarrey", resultDto.SourceUniqueID);
			Assert.AreEqual("Superlong stacktrace that you'll never figure out.", resultDto.StackTrace);
			Assert.AreEqual(new DateTime(2016, 5, 2, 22, 7, 50), resultDto.TimestampUTC);
		}

		[Test]
		public void ConvertMultipleArtifactDtoToDtoTest()
		{
			// ARRANGE
			string expectedName1 = "My Job History Error 1";
			string expectedName2 = "My Job History Error  2";

			ArtifactDTO expectedArtifactDto1 = CreateMockedJobHistoryErrorArtifactDto(expectedName1, _errorTypeJobChoiceId, _errorStatusNewChoiceId);
			ArtifactDTO expectedArtifactDto2 = CreateMockedJobHistoryErrorArtifactDto(expectedName2, _errorTypeItemChoiceId, _errorStatusExpiredChoiceId);

			Dictionary<Guid, int> errorStatusChoiceDictionary = new Dictionary<Guid, int> {
				{ _errorStatusNewChoiceGuid, _errorStatusNewChoiceId },
				{ _errorStatusExpiredChoiceGuid, _errorStatusExpiredChoiceId },
				{ _errorStatusInProgressChoiceGuid, _errorStatusInProgressChoiceId },
				{ _errorStatusRetriedChoiceGuid, _errorStatusRetriedChoiceId }
			};
			_artifactGuidRepository.GetArtifactIdsForGuids(JobHistoryErrorDTO.Choices.ErrorStatus.GuidList).Returns(errorStatusChoiceDictionary);
			Dictionary<Guid, int> errorTypeChoiceDictionary = new Dictionary<Guid, int>
			{
				{ _errorTypeJobChoiceGuid, _errorTypeJobChoiceId },
				{ _errorTypeItemChoiceGuid, _errorTypeItemChoiceId }
			};
			_artifactGuidRepository.GetArtifactIdsForGuids(JobHistoryErrorDTO.Choices.ErrorType.GuidList).Returns(errorTypeChoiceDictionary);
			IDictionary<string, ArtifactFieldDTO> fieldMapping = expectedArtifactDto1.Fields.ToDictionary(k => k.Name, v => v);

			// ACT
			List<JobHistoryErrorDTO> resultDtos = _testInstance.ConvertArtifactDtoToDto(new[] { expectedArtifactDto1, expectedArtifactDto2 });

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
				ErrorStatus = new Choice(errorStatusChoiceId) {Name=""},
				ErrorType = new Choice(errorTypeChoiceId) {Name=""},
				JobHistory = 1041673,
				Name = name,
				SourceUniqueID = "jimCarrey",
				StackTrace = "Superlong stacktrace that you'll never figure out.",
				TimestampUTC = DateTime.UtcNow
			};
		}

		public ArtifactDTO CreateMockedJobHistoryErrorArtifactDto(string name, int errorTypeChoiceId = _errorTypeJobChoiceId, int errorStatusChoiceId = _errorStatusNewChoiceId)
		{
			return new ArtifactDTO(6587693, 1110055, name, new List<ArtifactFieldDTO>()
			{
				new ArtifactFieldDTO() { ArtifactId= 1005983, FieldType= "WholeNumber", Name= "ArtifactID", Value= Arg.Any<int>()},
				new ArtifactFieldDTO() { ArtifactId= 1005984, FieldType= "LongText", Name= "Error", Value= "Just your friendly neighborhood error"},
				new ArtifactFieldDTO() { ArtifactId= 1005985, FieldType= "SingleChoice", Name= "ErrorStatus", Value= @"{ ""ArtifactID"": " + errorStatusChoiceId + @", ""ArtifactGuids"": [], ""Name"": ""New"" }"},
				new ArtifactFieldDTO() { ArtifactId= 1005986, FieldType= "SingleChoice", Name= "ErrorType", Value= @"{ ""ArtifactID"": " + errorTypeChoiceId + @", ""ArtifactGuids"": [], ""Name"": ""New"" }"},
				new ArtifactFieldDTO() { ArtifactId= 1005987, FieldType= "SingleObject", Name= "JobHistory", Value= @"{ ""ArtifactTypeID"": 1000050, ""ArtifactID"": " + _jobHistoryArtifactId + @", ""ArtifactGuids"": [], ""Name"": ""Name"" }"},
				new ArtifactFieldDTO() { ArtifactId= 1005988, FieldType= "FixedLengthText", Name= "Name", Value= name},
				new ArtifactFieldDTO() { ArtifactId= 1005989, FieldType= "FixedLengthText", Name= "SourceUniqueID", Value= "jimCarrey"},
				new ArtifactFieldDTO() { ArtifactId= 1005990, FieldType= "LongText", Name= "StackTrace", Value= "Superlong stacktrace that you'll never figure out."},
				new ArtifactFieldDTO() { ArtifactId= 1005991, FieldType= "Date", Name= "TimestampUTC", Value= "2016-05-02T22:07:50"}
			});
		}
	}
}
