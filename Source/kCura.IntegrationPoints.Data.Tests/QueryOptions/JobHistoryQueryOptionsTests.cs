using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.QueryOptions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.QueryOptions
{
    [TestFixture, Category("Unit")]
    public class JobHistoryQueryOptionsTests
    {
        private readonly IDictionary<string, Guid> _jobHistoryFieldsGuids = new Dictionary<string, Guid>
        {
            ["IntegrationPoint"] = new Guid("d3e791d3-2e21-45f4-b403-e7196bd25eea"),
            ["JobStatus"] = new Guid("5c28ce93-c62f-4d25-98c9-9a330a6feb52"),
            ["ItemsTransferred"] = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c"),
            ["ItemsWithErrors"] = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e"),
            ["StartTimeUTC"] = new Guid("25b7c8ef-66d9-41d1-a8de-29a93e47fb11"),
            ["EndTimeUTC"] = new Guid("4736cf49-ad0f-4f02-aaaa-898e07400f22"),
            ["BatchInstance"] = new Guid("08ba2c77-a9cd-4faf-a77a-be35e1ef1517"),
            ["DestinationWorkspace"] = new Guid("ff01a766-b494-4f2c-9cbb-10a5ab163b8d"),
            ["TotalItems"] = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b"),
            ["DestinationWorkspaceInformation"] = new Guid("20a24c4e-55e8-4fc2-abbe-f75c07fad91b"),
            ["JobType"] = new Guid("e809db5e-5e99-4a75-98a1-26129313a3f5"),
            ["DestinationInstance"] = new Guid("6d91ea1e-7b34-46a9-854e-2b018d4e35ef"),
            ["FilesSize"] = new Guid("d81817dc-91cb-44c4-b9b7-7c445da64f5a"),
            ["Overwrite"] = new Guid("42d49f5e-b0e7-4632-8d30-1c6ee1d97fa7"),
            ["JobID"] = new Guid("77d797ef-96c9-4b47-9ef8-33f498b5af0d"),
            ["Name"] = new Guid("07061466-5fab-4581-979c-c801e8207370"),
            ["ItemsRead"] = new Guid("2b76010a-9cf8-4276-9d6d-504d026f0b27")
        };

        [Test]
        public void All_ShouldSetAllFieldsIdsToFieldGuids()
        {
            // act
            JobHistoryQueryOptions sut = JobHistoryQueryOptions.All();

            // assert
            sut.FieldGuids.ShouldBeEquivalentTo(_jobHistoryFieldsGuids.Values);
        }

        [Test]
        public void AllExcept_ShouldSetAllFieldsIdsToFieldGuidsExceptOne()
        {
            // arrange
            Guid exceptOne = _jobHistoryFieldsGuids["Documents"];

            // act
            JobHistoryQueryOptions sut = JobHistoryQueryOptions
                .All()
                .Except(exceptOne.ToString());

            // assert
            sut.FieldGuids
                .ShouldBeEquivalentTo(
                    _jobHistoryFieldsGuids.Values.Except(new[] { exceptOne })
                );
        }

        [Test]
        public void AllExcept_ShouldSetAllFieldsIdsToFieldGuidsExceptTwo()
        {
            // arrange
            Guid exceptOne = _jobHistoryFieldsGuids["Documents"];
            Guid exceptTwo = _jobHistoryFieldsGuids["Name"];

            // act
            JobHistoryQueryOptions sut = JobHistoryQueryOptions
                .All()
                .Except(exceptOne.ToString())
                .Except(exceptTwo.ToString());

            // assert
            sut.FieldGuids
                .ShouldBeEquivalentTo(
                    _jobHistoryFieldsGuids.Values.Except(new[]
                    {
                        exceptOne,
                        exceptTwo
                    })
                );
        }

        [Test]
        public void ConstainsAll_ShouldReturnTrueIfAllFieldsGuidsAreSet()
        {
            // arrange
            JobHistoryQueryOptions options = JobHistoryQueryOptions.All();

            // act
            bool sut = options.ContainsAll();

            // assert
            sut.Should().BeTrue();
        }
    }
}
