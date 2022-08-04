using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ArtifactGuid;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class KeplerArtifactGuidRepositoryTests : TestBase
    {
        private const int _WORKSPACE_ARTIFACT_ID = 937936;
        private IArtifactGuidManager _artifactGuidManager;
        private IServicesMgr _servicesMgr;

        private KeplerArtifactGuidRepository _instance;

        [SetUp]
        public override void SetUp()
        {
            _artifactGuidManager = Substitute.For<IArtifactGuidManager>();

            _servicesMgr = Substitute.For<IServicesMgr>();
            _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System).Returns(_artifactGuidManager);

            _instance = new KeplerArtifactGuidRepository(_WORKSPACE_ARTIFACT_ID, _servicesMgr);
        }

        [TearDown]
        public void TearDown()
        {
            _servicesMgr.Received(1).CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System);
        }

        [Test]
        public void ItShouldInsertArtifactGuidForArtifactId()
        {
            var artifactId = 498784;
            var guid = new Guid("599A7CDA-8FBD-4ABF-8E64-D3889FD6EBCD");

            _instance.InsertArtifactGuidForArtifactId(artifactId, guid);

            _artifactGuidManager.Received(1).CreateSingleAsync(_WORKSPACE_ARTIFACT_ID, artifactId, Arg.Is<List<Guid>>(x => x[0] == guid));
        }

        [Test]
        public void ItShouldInsertArtifactGuidsForArtifactIds()
        {
            var guidToIdDictionary = new Dictionary<Guid, int>
            {
                {new Guid("F2D57E24-DB09-4BB9-9634-AC0D381DDF0A"), 249581},
                {new Guid("D2028D50-10FD-4855-A941-136C6BB5CE0D"), 235828}
            };


            _instance.InsertArtifactGuidsForArtifactIds(guidToIdDictionary);

            foreach (var keyValuePair in guidToIdDictionary)
            {
                _artifactGuidManager.Received(1).CreateSingleAsync(_WORKSPACE_ARTIFACT_ID, keyValuePair.Value, Arg.Is<List<Guid>>(x => x[0] == keyValuePair.Key));
            }
        }

        [Test]
        public void ItShouldCheckIfGuidsExist()
        {
            var existingGuid1 = new Guid("64A26DEB-8A29-4263-8C8D-B87146C3D7FC");
            var existingGuid2 = new Guid("F7261136-A97C-4A87-9821-70141A19F58F");
            var nonExistingGuid = new Guid("8EFE2455-1521-4F76-8F31-8308C92ED198");

            _artifactGuidManager.GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, existingGuid1).Returns(Task.FromResult(true));
            _artifactGuidManager.GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, existingGuid2).Returns(Task.FromResult(true));
            _artifactGuidManager.GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, nonExistingGuid).Returns(Task.FromResult(false));

            var result = _instance.GuidsExist(new[] {existingGuid1, existingGuid2, nonExistingGuid});

            Assert.That(result[existingGuid1], Is.True);
            Assert.That(result[existingGuid2], Is.True);
            Assert.That(result[nonExistingGuid], Is.False);

            _artifactGuidManager.Received(1).GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, existingGuid1);
            _artifactGuidManager.Received(1).GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, existingGuid2);
            _artifactGuidManager.Received(1).GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, nonExistingGuid);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldCheckIfGuidExist(bool guidExists)
        {
            var guid = new Guid("4FADD67C-66F6-4334-AB3A-6160B0C12F69");

            _artifactGuidManager.GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, guid).Returns(Task.FromResult(guidExists));

            var result = _instance.GuidExists(guid);

            Assert.That(result, Is.EqualTo(guidExists));

            _artifactGuidManager.Received(1).GuidExistsAsync(_WORKSPACE_ARTIFACT_ID, guid);
        }

        [Test]
        public void ItShouldGetGuidsForArtifactIds()
        {
            var artifactId1 = 419393;
            var artifactId2 = 687104;

            var guid1 = new Guid("DA4EC2C8-6E0D-47F1-98D4-5B2D4C9BF1F8");
            var guid2 = new Guid("08C39D1C-BA2A-4742-8E66-157BB4E32026");

            _artifactGuidManager.ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId1).Returns(Task.FromResult(new List<Guid> {guid1}));
            _artifactGuidManager.ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId2).Returns(Task.FromResult(new List<Guid> {guid2}));

            var result = _instance.GetGuidsForArtifactIds(new[] {artifactId1, artifactId2});

            Assert.That(result[artifactId1], Is.EqualTo(guid1));
            Assert.That(result[artifactId2], Is.EqualTo(guid2));

            _artifactGuidManager.Received(1).ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId1);
            _artifactGuidManager.Received(1).ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId2);
        }

        [Test]
        public void ItShouldReturnFirstGuidWhenRetrievingGuidsForArtifactId()
        {
            var artifactId = 646329;

            var guid1 = new Guid("3F4A8862-B3A8-4002-A355-224A3894896C");
            var guid2 = new Guid("87A480F5-BBA6-4C6E-94BE-94920360109B");

            _artifactGuidManager.ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId).Returns(Task.FromResult(new List<Guid> {guid1, guid2}));

            var result = _instance.GetGuidsForArtifactIds(new[] {artifactId});

            Assert.That(result[artifactId], Is.EqualTo(guid1));

            _artifactGuidManager.Received(1).ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId);
        }

        [Test]
        public void ItShouldHandleNonExistingGuid()
        {
            var artifactId = 791603;

            _artifactGuidManager.ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId).Returns(Task.FromResult(new List<Guid>()));

            var result = _instance.GetGuidsForArtifactIds(new[] {artifactId});

            Assert.That(result.ContainsKey(artifactId), Is.False);

            _artifactGuidManager.Received(1).ReadSingleGuidsAsync(_WORKSPACE_ARTIFACT_ID, artifactId);
        }

        [Test]
        public void ItShouldGetArtifactIdsForGuids()
        {
            var artifactId1 = 232819;
            var artifactId2 = 642680;

            var guid1 = new Guid("D6A03737-A1C8-4AF1-B357-D35115F8C9BF");
            var guid2 = new Guid("014880FF-631C-439B-A7B3-14E1A54D9C29");

            _artifactGuidManager.ReadSingleArtifactIdAsync(_WORKSPACE_ARTIFACT_ID, guid1).Returns(Task.FromResult(artifactId1));
            _artifactGuidManager.ReadSingleArtifactIdAsync(_WORKSPACE_ARTIFACT_ID, guid2).Returns(Task.FromResult(artifactId2));

            var result = _instance.GetArtifactIdsForGuids(new[] {guid1, guid2});

            Assert.That(result[guid1], Is.EqualTo(artifactId1));
            Assert.That(result[guid2], Is.EqualTo(artifactId2));

            _artifactGuidManager.Received(1).ReadSingleArtifactIdAsync(_WORKSPACE_ARTIFACT_ID, guid1);
            _artifactGuidManager.Received(1).ReadSingleArtifactIdAsync(_WORKSPACE_ARTIFACT_ID, guid2);
        }
    }
}