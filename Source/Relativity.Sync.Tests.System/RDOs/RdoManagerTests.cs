using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.Common.RDOs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.RDOs
{
    internal class RdoManagerTests : SystemTest
    {
        const long BigLongValue = long.MaxValue;

        private RdoManager _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new RdoManager(Logger, new ServicesManagerStub(), new RdoGuidProvider());
        }

        [IdentifiedTest("5CDA93A5-F9D2-4C8A-B4CC-EFB832D8746D")]
        public async Task EnsureTypeExist_ShouldCreateType_WhenTypeDoesNotExist()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(workspace.ArtifactID);

            // Assert
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var typeQueryResult = await objectManager.QueryAsync(workspace.ArtifactID, new QueryRequest
                {
                    Condition = $"'Name' == '{SampleRdo.ExpectedRdoInfo.Name}'",
                    Fields = new[]
                    {
                        new FieldRef {Name = "Artifact Type ID"}
                    },
                    ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.ObjectType}
                }, 0, 1).ConfigureAwait(false);

                var existingFieldsQueryResult = await objectManager.QueryAsync(workspace.ArtifactID, new QueryRequest()
                {
                    Condition = $"'FieldArtifactTypeID' == {typeQueryResult.Objects.First().FieldValues.First().Value}",
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int) ArtifactType.Field
                    }
                }, 0, int.MaxValue).ConfigureAwait(false);

                foreach (var fieldInfo in SampleRdo.ExpectedRdoInfo.Fields.Values)
                {
                    existingFieldsQueryResult.Objects.Any(x => x.Guids.Contains(fieldInfo.Guid)).Should().BeTrue();
                }
            }
        }

        [IdentifiedTest("2180C723-8911-4645-AB95-D92883037939")]
        public async Task EnsureTypeExists_ShouldCreateMissingFields()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            // Act
            await _sut.EnsureTypeExistsAsync<ExtendedSampleRdo>(workspace.ArtifactID).ConfigureAwait(false);

            // Assert
            using (var guidManager = ServiceFactory.CreateProxy<IArtifactGuidManager>())
            {
                foreach (var fieldInfo in ExtendedSampleRdo.ExpectedRdoInfo.Fields.Values)
                {
                    var newFieldExists = await guidManager
                        .GuidExistsAsync(workspace.ArtifactID, fieldInfo.Guid)
                        .ConfigureAwait(false);

                    newFieldExists.Should().BeTrue($"Field {fieldInfo.Name} with GUID: {fieldInfo.Guid} should be created");
                }
            }
        }

        [IdentifiedTest("3F41E416-5462-41FA-ADEF-F47BC1C74EA0")]
        public async Task CreateAsync_ShouldCreateRdoInstance()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            var sampleRdo = new SampleRdo
            {
                SomeField = 5,
                OptionalTextField = "Enova (not) Rocks"
            };

            // Act
            await _sut.CreateAsync(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);

            // Assert
            sampleRdo.ArtifactId.Should().NotBe(0);

            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var result = await objectManager.QuerySlimAsync(workspace.ArtifactID, new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = new Guid("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED")
                    },
                    Fields = new[]
                    {
                        new FieldRef {Name = "SomeField"},
                        new FieldRef {Name = "OptionalTextField"}
                    }
                }, 0, 1);

                var createdObject = result.Objects.First();

                createdObject.ArtifactID.Should().Be(sampleRdo.ArtifactId);
                createdObject.Values[0].Should().Be(sampleRdo.SomeField);
                createdObject.Values[1].Should().Be(sampleRdo.OptionalTextField);
            }
        }

        [IdentifiedTest("3B9FA78E-36C2-465D-8792-523C92F08250")]
        public async Task GetAsync_ShouldRetrieveAllFields()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            var sampleRdo = new SampleRdo
            {
                SomeField = 5,
                OptionalTextField = "Enova (not) Rocks"
            };
            var artifactId = await CreateSampleRdoObject(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);

            // Act
            var result = await _sut.GetAsync<SampleRdo>(workspace.ArtifactID, artifactId).ConfigureAwait(false);

            result.ArtifactId.Should().Be(artifactId);
            result.SomeField.Should().Be(sampleRdo.SomeField);
            result.OptionalTextField.Should().Be(sampleRdo.OptionalTextField);
        }

        [IdentifiedTest("3B9FA78E-36C2-465D-8792-523C92F08250")]
        public async Task GetAsync_ShouldRetrieveOnlySpecifiedFields()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            var sampleRdo = new SampleRdo
            {
                SomeField = 5,
                OptionalTextField = "Enova (not) Rocks"
            };
            var artifactId = await CreateSampleRdoObject(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);

            // Act
            var result = await _sut.GetAsync<SampleRdo>(workspace.ArtifactID, artifactId, x => x.SomeField)
                .ConfigureAwait(false);

            result.ArtifactId.Should().Be(artifactId);
            result.SomeField.Should().Be(sampleRdo.SomeField);
            result.OptionalTextField.Should().BeNull();
        }

        [IdentifiedTest("C518602D-47CA-4D58-8AC1-A9CAC3F8B065")]
        public async Task GetAsync_ShouldStreamLongTextField_AndHandleUnicode_WhenFrameworkCreatedObjectType()
        {
	        // Arrange
	        var workspace = await Environment.CreateWorkspaceAsync();

	        await _sut.EnsureTypeExistsAsync<ExtendedSampleRdo>(workspace.ArtifactID).ConfigureAwait(false);

	        string longText = string.Join("", Enumerable.Repeat("ą", 10000));
	        string shortText = "Żółw";

	        var extendedSampleRdo = new ExtendedSampleRdo()
	        {
		        SomeField = 5,
		        LongTextField = longText,
                OptionalTextField = shortText
	        };

	        await _sut.CreateAsync(workspace.ArtifactID, extendedSampleRdo).ConfigureAwait(false);

	        // Act
	        var result = await _sut.GetAsync<ExtendedSampleRdo>(workspace.ArtifactID, extendedSampleRdo.ArtifactId, x => x.LongTextField, x=> x.OptionalTextField)
		        .ConfigureAwait(false);

	        result.ArtifactId.Should().Be(extendedSampleRdo.ArtifactId);
	        result.LongTextField.Should().Be(longText);
	        result.OptionalTextField.Should().Be(shortText);
        }

        [IdentifiedTest("BF1A2F1B-9217-4D57-AEAD-D98877343FDB")]
        public async Task SetValuesAsync_ShouldUpdateAllValues()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            var sampleRdo = new SampleRdo
            {
                SomeField = 5,
                OptionalTextField = "Enova (not) Rocks"
            };
            var artifactId = await CreateSampleRdoObject(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);

            sampleRdo.SomeField = 1;
            sampleRdo.OptionalTextField = "Adler Sieben";
            sampleRdo.ArtifactId = artifactId;

            // Act
            await _sut.SetValuesAsync(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);

            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var result = await objectManager.QuerySlimAsync(workspace.ArtifactID, new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = new Guid("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED")
                    },
                    Fields = new[]
                    {
                        new FieldRef {Name = "SomeField"},
                        new FieldRef {Name = "OptionalTextField"}
                    }
                }, 0, 1);

                var createdObject = result.Objects.First();

                createdObject.ArtifactID.Should().Be(sampleRdo.ArtifactId);
                createdObject.Values[0].Should().Be(sampleRdo.SomeField);
                createdObject.Values[1].Should().Be(sampleRdo.OptionalTextField);
            }
        }
        
        [IdentifiedTest("FEE486E9-C37C-437F-811D-DC9D81CF300C")]
        public async Task SetValuesAsync_ShouldUpdateOnlySpecifiedValue()
        {
            // Arrange
            var workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            const string originalText = "Enova (not) Rocks";
            var sampleRdo = new SampleRdo
            {
                SomeField = 5,
                OptionalTextField = originalText
            };
            var artifactId = await CreateSampleRdoObject(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);

            const int someFieldNewValue = 1;
            sampleRdo.OptionalTextField = "Adler Sieben";
            sampleRdo.ArtifactId = artifactId;

            // Act
            await _sut.SetValueAsync(workspace.ArtifactID, sampleRdo, x => x.SomeField, someFieldNewValue).ConfigureAwait(false);

            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var result = await objectManager.QuerySlimAsync(workspace.ArtifactID, new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = new Guid("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED")
                    },
                    Fields = new[]
                    {
                        new FieldRef {Name = "SomeField"},
                        new FieldRef {Name = "OptionalTextField"}
                    }
                }, 0, 1);

                var queriedObject = result.Objects.First();

                sampleRdo.SomeField.Should().Be(someFieldNewValue);
                queriedObject.ArtifactID.Should().Be(sampleRdo.ArtifactId);
                queriedObject.Values[0].Should().Be(someFieldNewValue);
                queriedObject.Values[1].Should().Be(originalText);
            }
        }

        [IdentifiedTest("D2E8DBEA-9F15-4D8E-AFE2-2C3784ED06D5")]
        public async Task CreateAsync_ShouldHandleBigNumbersInLongFields()
        {
            // Arrange
            WorkspaceRef workspace = await Environment.CreateWorkspaceAsync();
            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            SampleRdo sampleRdo = new SampleRdo
            {
                LongField = BigLongValue
            };
            
            // Act
            await _sut.CreateAsync(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);
            
            // Assert
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var result = await objectManager.QuerySlimAsync(workspace.ArtifactID, new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = new Guid("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED")
                    },
                    Fields = new[]
                    {
                        new FieldRef { Name = "LongField" },
                    }
                }, 0, 1).ConfigureAwait(false);

                var queriedObject = result.Objects.First();

                sampleRdo.LongField.Should().Be(BigLongValue);
                queriedObject.ArtifactID.Should().Be(sampleRdo.ArtifactId);
                queriedObject.Values[0].Should().Be(BigLongValue.ToString());
            }
        }

        [IdentifiedTest("CBD452A1-8743-470A-8DDD-64545FAFB68C")]
        public async Task SetValuesAsync_ShouldHandleBigNumbersInLongFields()
        {
            // Arrange
            WorkspaceRef workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);

            SampleRdo sampleRdo = new SampleRdo
            {
                SomeField = 5,
            };
            
            int artifactId = await CreateSampleRdoObject(workspace.ArtifactID, sampleRdo).ConfigureAwait(false);
            sampleRdo.ArtifactId = artifactId;
            
            // Act
            await _sut.SetValueAsync(workspace.ArtifactID, sampleRdo, x => x.LongField, BigLongValue).ConfigureAwait(false);
            
            // Assert
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var result = await objectManager.QuerySlimAsync(workspace.ArtifactID, new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = new Guid("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED")
                    },
                    Fields = new[]
                    {
                        new FieldRef { Name = "LongField" },
                    }
                }, 0, 1).ConfigureAwait(false);

                var queriedObject = result.Objects.First();

                sampleRdo.LongField.Should().Be(BigLongValue);
                queriedObject.ArtifactID.Should().Be(sampleRdo.ArtifactId);
                queriedObject.Values[0].Should().Be(BigLongValue.ToString());
            }
        }

        [IdentifiedTest("CC7A75D9-C415-4BE6-9B78-4CDB07A6CD93")]
        public async Task GetAsync_ShouldHandleBigNumbersInLongFields()
        {
            // Arrange
            WorkspaceRef workspace = await Environment.CreateWorkspaceAsync();

            await CreaSampleRdoTypeInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);
            
            int artifactId = await CreateSampleRdoObject(workspace.ArtifactID, new SampleRdo
            {
                LongField = BigLongValue
            }).ConfigureAwait(false);

            // Act
            SampleRdo sampleRdo = await _sut.GetAsync<SampleRdo>(workspace.ArtifactID, artifactId).ConfigureAwait(false);
            
            // Assert
            sampleRdo.LongField.Should().Be(BigLongValue);
        }

        private async Task CreaSampleRdoTypeInWorkspaceAsync(int workspaceId)
        {
            using (IObjectTypeManager objectTypeManager =
                ServiceFactory.CreateProxy<IObjectTypeManager>())
            using (IArtifactGuidManager guidManager =
                ServiceFactory.CreateProxy<IArtifactGuidManager>())
            {
                ObjectTypeRequest objectTypeRequest = GetObjectTypeDefinition();

                int objectTypeArtifactId = await objectTypeManager.CreateAsync(workspaceId, objectTypeRequest)
                    .ConfigureAwait(false);

                await guidManager.CreateSingleAsync(workspaceId, objectTypeArtifactId,
                        new List<Guid>() {SampleRdo.ExpectedRdoInfo.TypeGuid})
                    .ConfigureAwait(false);

                using (IFieldManager fieldManager = ServiceFactory.CreateProxy<IFieldManager>())
                {
                    foreach (RdoFieldInfo fieldInfo in SampleRdo.ExpectedRdoInfo.Fields.Values)
                    {
                        int fieldId =
                            await CreateFieldInTypeAsync(fieldInfo, objectTypeArtifactId, workspaceId, fieldManager)
                                .ConfigureAwait(false);

                        await guidManager
                            .CreateSingleAsync(workspaceId, fieldId, new List<Guid>() {fieldInfo.Guid})
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        private ObjectTypeRequest GetObjectTypeDefinition()
        {
            return new ObjectTypeRequest
            {
                CopyInstancesOnCaseCreation = false,
                CopyInstancesOnParentCopy = false,
                EnableSnapshotAuditingOnDelete = true,
                Keywords = null,
                Name = SampleRdo.ExpectedRdoInfo.Name,
                Notes = null,
                ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier
                    {ArtifactTypeID = (int) ArtifactType.Case}),
                PersistentListsEnabled = false,
                PivotEnabled = false,
                RelativityApplications = null,
                SamplingEnabled = false,
                UseRelativityForms = null
            };
        }

        private Task<int> CreateFieldInTypeAsync(RdoFieldInfo fieldInfo, int objectTypeId, int workspaceId,
            IFieldManager fieldManager)
        {
            switch (fieldInfo.Type)
            {
                case RdoFieldType.LongText:
                    return fieldManager.CreateLongTextFieldAsync(workspaceId,
                        new LongTextFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            IsRequired = fieldInfo.IsRequired
                        });
                case RdoFieldType.FixedLengthText:
                    return fieldManager.CreateFixedLengthFieldAsync(workspaceId,
                        new FixedLengthFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            Length = fieldInfo.TextLength,
                            IsRequired = fieldInfo.IsRequired
                        });
                case RdoFieldType.WholeNumber:
                    return fieldManager.CreateWholeNumberFieldAsync(workspaceId,
                        new WholeNumberFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            IsRequired = fieldInfo.IsRequired
                        });

                case RdoFieldType.YesNo:
                    return fieldManager.CreateYesNoFieldAsync(workspaceId,
                        new YesNoFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            IsRequired = fieldInfo.IsRequired
                        });

                default:
                    throw new NotSupportedException($"Sync doesn't support creation of field type: {fieldInfo.Type}");
            }
        }

        private async Task<int> CreateSampleRdoObject(int workspaceId, SampleRdo rdo)
        {
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var request = new CreateRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = SampleRdo.ExpectedRdoInfo.TypeGuid
                    },
                    FieldValues = new[]
                    {
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = SampleRdo.ExpectedRdoInfo.Fields.Values
                                    .First(x => x.Name == nameof(SampleRdo.SomeField)).Guid
                            },
                            Value = rdo.SomeField
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = SampleRdo.ExpectedRdoInfo.Fields.Values
                                    .First(x => x.Name == nameof(SampleRdo.OptionalTextField)).Guid
                            },
                            Value = rdo.OptionalTextField
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = SampleRdo.ExpectedRdoInfo.Fields.Values
                                    .First(x => x.Name == nameof(SampleRdo.LongField)).Guid
                            },
                            Value = rdo.LongField.ToString()
                        },
                    }
                };

                var result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
                return result.Object.ArtifactID;
            }
        }
    }
}