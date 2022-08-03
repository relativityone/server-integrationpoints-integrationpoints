using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.EventHandlers.Commands;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    internal class NonDocumentObjectMigrationCommandTests : UpdateConfigurationCommandTestsBase
    {
        private NonDocumentObjectMigrationCommand _sut;

        protected override List<string> Names => new List<string>() { "Secured Configuration", "Source Configuration", "Destination Configuration" };

        public override void SetUp()
        {
            base.SetUp();

            _sut = new NonDocumentObjectMigrationCommand(EHHelperFake.Object, RelativityObjectManagerMock.Object);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Execute_ShouldNotProcess_WhenDestinationConfigurationIsNullOrWhiteSpace(string destinationConfiguration)
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(destinationConfiguration);
            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }

        [Test]
        public void Execute_ShouldNotProcess_WhenDestinationProviderIsNotRelativity()
        {
            // Arrange
            JObject destinationConfiguration = new JObject();
            destinationConfiguration["destinationProviderType"] = Core.Constants.IntegrationPoints.DestinationProviders.LOADFILE;
            RelativityObjectSlim objectSlim = PrepareObject(destinationConfiguration.ToString());
            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }

        [TestCase(@"{""artifactTypeID"":10,""DestinationArtifactTypeID"":10,""destinationProviderType"":""74a863b9-00ec-4bb7-9b3e-1e22323010c6"",""EntityManagerFieldContainsLink"":""true"",""CreateSavedSearchForTagging"":""false"",""CaseArtifactId"":1028231,""DestinationFolderArtifactId"":""1003697"",""ProductionImport"":false,""Provider"":""relativity"",""ImportOverwriteMode"":""AppendOnly"",""importNativeFile"":""false"",""importNativeFileCopyMode"":""DoNotImportNativeFiles"",""UseFolderPathInformation"":""false"",""UseDynamicFolderPath"":""false"",""ImageImport"":""false"",""ImagePrecedence"":[],""ProductionPrecedence"":0,""IncludeOriginalImages"":false,""MoveExistingDocuments"":""false"",""ExtractedTextFieldContainsFilePath"":""false"",""ExtractedTextFileEncoding"":""utf-16"",""FieldOverlayBehavior"":""Use Field Settings""}")]
        [TestCase(@"{""artifactTypeID"":10,""DestinationArtifactTypeID"":10,""DestinationProviderType"":""74A863B9-00EC-4BB7-9B3E-1E22323010C6"",""EntityManagerFieldContainsLink"":""true"",""CreateSavedSearchForTagging"":""false"",""CaseArtifactId"":1028231,""DestinationFolderArtifactId"":""1003697"",""ProductionImport"":false,""Provider"":""relativity"",""ImportOverwriteMode"":""AppendOnly"",""importNativeFile"":""false"",""importNativeFileCopyMode"":""DoNotImportNativeFiles"",""UseFolderPathInformation"":""false"",""UseDynamicFolderPath"":""false"",""ImageImport"":""false"",""ImagePrecedence"":[],""ProductionPrecedence"":0,""IncludeOriginalImages"":false,""MoveExistingDocuments"":""false"",""ExtractedTextFieldContainsFilePath"":""false"",""ExtractedTextFileEncoding"":""utf-16"",""FieldOverlayBehavior"":""Use Field Settings""}")]
        public void Execute_ShouldNotProcess_WhenDestinationArtifactTypeIdIsAlreadyThere(string destinationConfiguration)
        {
            RelativityObjectSlim objectSlim = PrepareObject(destinationConfiguration);
            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }

        [TestCase(@"{""artifactTypeID"":10,""destinationProviderType"":""74a863b9-00ec-4bb7-9b3e-1e22323010c6"",""EntityManagerFieldContainsLink"":""true"",""CreateSavedSearchForTagging"":""false"",""CaseArtifactId"":1028231,""DestinationFolderArtifactId"":""1003697"",""ProductionImport"":false,""Provider"":""relativity"",""ImportOverwriteMode"":""AppendOnly"",""importNativeFile"":""false"",""importNativeFileCopyMode"":""DoNotImportNativeFiles"",""UseFolderPathInformation"":""false"",""UseDynamicFolderPath"":""false"",""ImageImport"":""false"",""ImagePrecedence"":[],""ProductionPrecedence"":0,""IncludeOriginalImages"":false,""MoveExistingDocuments"":""false"",""ExtractedTextFieldContainsFilePath"":""false"",""ExtractedTextFileEncoding"":""utf-16"",""FieldOverlayBehavior"":""Use Field Settings""}")]
        [TestCase(@"{""artifactTypeID"":10,""DestinationProviderType"":""74A863B9-00EC-4BB7-9B3E-1E22323010C6"",""EntityManagerFieldContainsLink"":""true"",""CreateSavedSearchForTagging"":""false"",""CaseArtifactId"":1028231,""DestinationFolderArtifactId"":""1003697"",""ProductionImport"":false,""Provider"":""relativity"",""ImportOverwriteMode"":""AppendOnly"",""importNativeFile"":""false"",""importNativeFileCopyMode"":""DoNotImportNativeFiles"",""UseFolderPathInformation"":""false"",""UseDynamicFolderPath"":""false"",""ImageImport"":""false"",""ImagePrecedence"":[],""ProductionPrecedence"":0,""IncludeOriginalImages"":false,""MoveExistingDocuments"":""false"",""ExtractedTextFieldContainsFilePath"":""false"",""ExtractedTextFileEncoding"":""utf-16"",""FieldOverlayBehavior"":""Use Field Settings""}")]
        public void Execute_ShouldAddDestinationArtifactTypeId_WhenMissing(string destinationConfiguration)
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(destinationConfiguration);
            SetupRead(objectSlim);

            JObject destinationConfigurationWithDestinationArtifactTypeId = JObject.Parse(destinationConfiguration);
            destinationConfigurationWithDestinationArtifactTypeId["DestinationArtifactTypeId"] = 10;

            RelativityObjectSlim objectSlimExpected = PrepareObject(destinationConfigurationWithDestinationArtifactTypeId.ToString());

            // Act
            _sut.Execute();

            // Assert
            ShouldBeUpdated(objectSlimExpected);
        }

        private RelativityObjectSlim PrepareObject(string destinationConfiguration = null)
        {
            {
                return new RelativityObjectSlim()
                {
                    ArtifactID = 1,
                    Values = new List<object>()
                    {
                        string.Empty,
                        string.Empty,
                        destinationConfiguration
                    }
                };
            }
        }
    }
}
