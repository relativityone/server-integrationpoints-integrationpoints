using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations.CommonTests
{
    internal class MassUpdateTests
    {
        private readonly IRepositoryWithMassUpdate _sut;
        private readonly Mock<IRelativityObjectManager> _objectManagerMock;

        public MassUpdateTests(
            IRepositoryWithMassUpdate sut,
            Mock<IRelativityObjectManager> objectManagerMock)
        {
            _sut = sut;
            _objectManagerMock = objectManagerMock;
        }

        public async Task ShouldBuildProperRequest()
        {
            // arrange
            var documentsIDs = new List<int> { 43, 21, 132, 8430, 587 };

            var fieldUpdateRequests = new List<FieldUpdateRequestDto>
            {
                new FieldUpdateRequestDto(
                    Guid.NewGuid(),
                    CreateFieldValueDtoMock(8)),
                new FieldUpdateRequestDto(
                    Guid.NewGuid(),
                    CreateFieldValueDtoMock(3))
            };

            _objectManagerMock
                .Setup(x => x.MassUpdateAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<FieldRefValuePair>>(),
                    It.IsAny<FieldUpdateBehavior>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(true));

            Func<IEnumerable<FieldRefValuePair>, bool> fieldsValidator = fields =>
            {
                bool resultIsValid = true;
                foreach (FieldUpdateRequestDto fieldUpdateRequestDto in fieldUpdateRequests)
                {
                    FieldRefValuePair matchingField = fields.Single(x => x.Field.Guid == fieldUpdateRequestDto.FieldIdentifier);
                    resultIsValid &= matchingField.Value == fieldUpdateRequestDto.NewValue.Value;
                }
                return resultIsValid;
            };

            // act
            await _sut
                .MassUpdateAsync(documentsIDs, fieldUpdateRequests)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(
                x => x.MassUpdateAsync(
                    It.Is<IEnumerable<int>>(receivedIDs => receivedIDs.SequenceEqual(documentsIDs)),
                    It.Is<IEnumerable<FieldRefValuePair>>(fields => fieldsValidator(fields)),
                    FieldUpdateBehavior.Merge,
                    ExecutionIdentity.CurrentUser));
        }

        public async Task ShouldReturnCorrectResult(bool expectedResult)
        {
            // arrange
            _objectManagerMock
                .Setup(x => x.MassUpdateAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<FieldRefValuePair>>(),
                    It.IsAny<FieldUpdateBehavior>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(expectedResult));

            // act
            bool result = await _sut
                .MassUpdateAsync(
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<FieldUpdateRequestDto>())
                .ConfigureAwait(false);

            // assert
            result.Should().Be(expectedResult);
        }

        public void ShouldRethrowObjectManagerException()
        {
            // arrange
            IntegrationPointsException exceptionToThrow = new IntegrationPointsException();

            _objectManagerMock
                .Setup(x => x.MassUpdateAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<FieldRefValuePair>>(),
                    It.IsAny<FieldUpdateBehavior>(),
                    It.IsAny<ExecutionIdentity>()))
                .Throws(exceptionToThrow);

            // act
            Func<Task> massUpdateAction = () => _sut
                .MassUpdateAsync(
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<FieldUpdateRequestDto>());

            // assert
            massUpdateAction.ShouldThrow<IntegrationPointsException>()
                .Which.Should().Be(exceptionToThrow);
        }

        private IFieldValueDto CreateFieldValueDtoMock(int value)
        {
            var fieldValueMock = new Mock<IFieldValueDto>();
            fieldValueMock.Setup(x => x.Value).Returns(value);
            return fieldValueMock.Object;
        }
    }
}
