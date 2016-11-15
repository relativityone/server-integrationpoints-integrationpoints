using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class IntegrationPointManagerTests : TestBase
	{
		private IIntegrationPointManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IIntegrationPointRepository _integrationPointRepository;
		private ISourceProviderRepository _sourceProviderRepository;
		private IDestinationProviderRepository _destinationProviderRepository;
		private IPermissionRepository _sourcePermissionRepository;
		private IPermissionRepository _destinationPermissionRepository;
		private ISavedSearchRepository _savedSearchRepository;
		private Guid _otherProviderGuid;

		private const int _SOURCE_WORKSPACE_ID = 100532;
		private const int _DESTINATION_WORKSPACE_ID = 349234;
		private const int INTEGRATION_POINT_ID = 101323;
		private const int _ARTIFACT_TYPE_ID = 1232;
		private const int _SOURCE_PROVIDER_ID = 39309;
		private const int _DESTINATION_PROVIDER_ID = 42042;
		private const int _SAVED_SEARCH_ID = 9492;
		private Guid _DESTINATION_PROVIDER_GUID = new Guid(ObjectTypeGuids.DestinationProvider);

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_sourceProviderRepository = Substitute.For<ISourceProviderRepository>();
			_sourcePermissionRepository = Substitute.For<IPermissionRepository>();

			_destinationProviderRepository = Substitute.For<IDestinationProviderRepository>();
			_destinationPermissionRepository = Substitute.For<IPermissionRepository>();
			_savedSearchRepository = Substitute.For<ISavedSearchRepository>();

			_otherProviderGuid = Guid.NewGuid();

			_repositoryFactory.GetIntegrationPointRepository(_SOURCE_WORKSPACE_ID).Returns(_integrationPointRepository);
			_repositoryFactory.GetPermissionRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_sourcePermissionRepository);
			_repositoryFactory.GetPermissionRepository(Arg.Is(_DESTINATION_WORKSPACE_ID)).Returns(_destinationPermissionRepository);
			_repositoryFactory.GetSourceProviderRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_sourceProviderRepository);
			_repositoryFactory.GetDestinationProviderRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_destinationProviderRepository);
			_repositoryFactory.GetSavedSearchRepository(Arg.Is(_SOURCE_WORKSPACE_ID), _SAVED_SEARCH_ID).Returns(_savedSearchRepository);

			_testInstance = new IntegrationPointManager(_repositoryFactory);
		}

		[Test]
		public void ReadTest()
		{
			// ARRANGE
			string expectedName = "MyTest";
			var expectedIntegrationPointDto = new IntegrationPointDTO() {Name = expectedName };

			_integrationPointRepository.Read(INTEGRATION_POINT_ID).Returns(expectedIntegrationPointDto);

			// ACT
			IntegrationPointDTO dto = _testInstance.Read(_SOURCE_WORKSPACE_ID, INTEGRATION_POINT_ID);

			// ASSERT
			Assert.IsNotNull(dto);
			Assert.AreEqual(expectedName, dto.Name);
		}

		[Test]
		[Explicit("This tests every possible permission combination but takes a while")]
		public void UserHasPermissionToRunJob_NonRelativityProvider_AllCombinations()
		{
			int paramCount = 10;
			for (int i = 0; i < Math.Pow(2, paramCount); i++)
			{
				this.ClearAllReceivedCalls();
				string stringVal = Convert.ToString(i, 2);
				var numList = new List<char>();
				int difference = paramCount - stringVal.Length;
				for (int k = 0; k < difference; k++)
				{
					numList.Add('0');
				}

				numList.AddRange(stringVal.ToCharArray());

				bool[] inputs = numList.Select(x => x == '1').ToArray();
				try
				{
					this.UserHasPermissionToRunJob_NonRelativityProvider_GoldFlow_Cases(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], inputs[5], inputs[6], inputs[7], inputs[8], inputs[9]);
				}
				catch (Exception e)
				{
					string message = $"UserHasPermissionsFailed with inputs {String.Join(",", inputs)}: {e.Message}";
					throw new Exception(message, e);
				}
			}
		}

		[Test]
		public void UserHasPermissionToRunJob_NonRelativityProvider_AllPermissions()
		{
			this.UserHasPermissionToRunJob_NonRelativityProvider_GoldFlow_Cases(true, true, true, true, true, true, true, true, true, true);
		}

		[Test]
		public void UserHasPermissionToRunJob_NonRelativityProvider_NoPermissions()
		{
			this.UserHasPermissionToRunJob_NonRelativityProvider_GoldFlow_Cases(false, false, false, false, false, false, false, false, false, false);
		}

		[Test]
		[Explicit("This tests every possible permission combination but takes a (very long) while")]
		public void UserHasPermissionToRunJob_RelativityProvider_AllCombinations()
		{
			int paramCount = 15;
			for (int i = 0; i < Math.Pow(2, paramCount); i++)
			{
				this.ClearAllReceivedCalls();
				string stringVal = Convert.ToString(i, 2);
				var numList = new List<char>();
				int difference = paramCount - stringVal.Length;
				for (int k = 0; k < difference; k++)
				{
					numList.Add('0');
				}

				numList.AddRange(stringVal.ToCharArray());

				bool[] inputs = numList.Select(x => x == '1').ToArray();
				try
				{
					this.UserHasPermissionToRunJob_RelativityProvider_GoldFlow(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], inputs[5], inputs[6], inputs[7], inputs[8], inputs[9], inputs[10], inputs[11], inputs[12], inputs[13], inputs[14]);
				}
				catch (Exception e)
				{
					string message = $"UserHasPermissionsFailed with inputs {String.Join(",", inputs)}: {e.Message}";
					throw new Exception(message, e);
				}
			}
		}

		[Test]
		public void UserHasPermissionToRunJob_RelativityProvider_AllPermissions()
		{
			this.UserHasPermissionToRunJob_RelativityProvider_GoldFlow(true, true, true, true, true, true, true, true, true, true, true, true, true, true, true);
		}

		[Test]
		public void UserHasPermissionToRunJob_RelativityProvider_NoPermissions()
		{
			this.UserHasPermissionToRunJob_RelativityProvider_GoldFlow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasPermissionToSaveIntegrationPoint_NonRelativityProvider_AllPermissions(bool isNew)
		{
			UserHasPermissionToSaveIntegrationPoint_NonRelativityProvider_GoldFlow_Cases(isNew, true, true, true, true, true, true, true, true, true, true, true, true);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasPermissionToSaveIntegrationPoint_NonRelativityProvider_NoPermissions(bool isNew)
		{
			UserHasPermissionToSaveIntegrationPoint_NonRelativityProvider_GoldFlow_Cases(isNew, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasPermissionToSaveIntegrationPoint_RelativityProvider_AllPermissions(bool isNew)
		{
			UserHasPermissionToSaveIntegrationPoint_RelativityProvider_GoldFlow_Cases(isNew, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasPermissionToSaveIntegrationPoint_RelativityProvider_NoPermissions(bool isNew)
		{
			UserHasPermissionToSaveIntegrationPoint_RelativityProvider_GoldFlow_Cases(isNew, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		[TestCase(true, null)]
		[TestCase(true, Constants.SourceProvider.Relativity)]
		[TestCase(true, Constants.SourceProvider.Other)]
		[TestCase(false, null)]
		[TestCase(false, Constants.SourceProvider.Relativity)]
		[TestCase(false, Constants.SourceProvider.Other)]
		public void UserHasPermissionToSaveIntegrationPoint_CheckCorrectPermissions(bool isNew, Constants.SourceProvider? provider)
		{
			// arrange
			var integrationPointObjectTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint);
			var integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = isNew ? 0 : INTEGRATION_POINT_ID,
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceProvider = _SOURCE_PROVIDER_ID,
				DestinationProvider = _DESTINATION_PROVIDER_ID
			};

			var sourceProvider = new SourceProviderDTO()
			{
				Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)
			};
			_sourceProviderRepository.Read(_SOURCE_PROVIDER_ID).Returns(sourceProvider);
			var destinationProvider = new DestinationProviderDTO()
			{
				Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID)
			};
			_destinationProviderRepository.Read(_DESTINATION_PROVIDER_ID).Returns(destinationProvider);

			var permissionCheckDto = new PermissionCheckDTO();

			IIntegrationPointManager integrationPointManager = Substitute.ForPartsOf<IntegrationPointManager>(_repositoryFactory);
			integrationPointManager.When( manager => manager.UserHasPermissionToRunJob(_SOURCE_WORKSPACE_ID, integrationPointDto, provider)).DoNotCallBase();
			integrationPointManager.UserHasPermissionToRunJob(_SOURCE_WORKSPACE_ID, integrationPointDto, provider).Returns(permissionCheckDto);

			// act
			integrationPointManager.UserHasPermissionToSaveIntegrationPoint(_SOURCE_WORKSPACE_ID, integrationPointDto, provider);

			// assert
			if (isNew)
			{
				_sourcePermissionRepository.DidNotReceive().UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Create);
				_sourcePermissionRepository.DidNotReceive().UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Edit);

				_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create);
			}
			else
			{
				_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Edit);
				_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit);
			}
			integrationPointManager.Received(1).UserHasPermissionToRunJob(_SOURCE_WORKSPACE_ID, integrationPointDto, provider);
		}

		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void UserHasPermissionToStopJob_CheckPermissions(bool canEditIntegrationPoint, bool canEditJobHistory)
		{
			// arrange
			_sourcePermissionRepository.UserHasArtifactInstancePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, INTEGRATION_POINT_ID, ArtifactPermission.Edit).Returns(canEditIntegrationPoint);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is<Guid>(guid => guid == new Guid(ObjectTypeGuids.JobHistory)), ArtifactPermission.Edit).Returns(canEditJobHistory);

			// act
			PermissionCheckDTO result =	_testInstance.UserHasPermissionToStopJob(_SOURCE_WORKSPACE_ID, INTEGRATION_POINT_ID);

			// assert
			Assert.AreEqual(canEditIntegrationPoint && canEditJobHistory, result.Success);
			if (!canEditIntegrationPoint)
			{
				Assert.Contains(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_INTEGRATIONPOINT, result.ErrorMessages);
			}
			if (!canEditJobHistory)
			{
				Assert.Contains(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_EDIT, result.ErrorMessages);
			}
		}

		private void ClearAllReceivedCalls()
		{
			_repositoryFactory.ClearReceivedCalls();
			_integrationPointRepository.ClearReceivedCalls();
			_sourceProviderRepository.ClearReceivedCalls();
			_sourcePermissionRepository.ClearReceivedCalls();
			_destinationPermissionRepository.ClearReceivedCalls();
			_savedSearchRepository.ClearReceivedCalls();
		}

		private void UserHasPermissionToSaveIntegrationPoint_NonRelativityProvider_GoldFlow_Cases(
			bool isNew,
			bool integrationPointTypeEditOrCreatePermission,
			bool integrationPointInstanceEditOrCreatePermission,
			bool sourceWorkspacePermission,
			bool integrationPointTypeViewPermission,
			bool integrationPointInstanceViewPermission,
			bool jobHistoryAddPermission,
			bool sourceImportPermission,
			bool destinationRdoPermissions,
			bool sourceProviderIsProvided,
			bool sourceProviderTypeViewPermission,
			bool sourceProviderInstanceViewPermission,
			bool destinationProviderViewPermission)
		{
			var integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = isNew ? 0 : INTEGRATION_POINT_ID,
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceProvider = _SOURCE_PROVIDER_ID,
				DestinationProvider = _DESTINATION_PROVIDER_ID
			};

			var integrationPointObjectTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint);
			if (isNew)
			{
				_sourcePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create).Returns(integrationPointTypeEditOrCreatePermission);
			}
			else
			{
				_sourcePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit).Returns(integrationPointTypeEditOrCreatePermission);
				_sourcePermissionRepository.UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Edit).Returns(integrationPointInstanceEditOrCreatePermission);
			}

			_sourcePermissionRepository.UserHasPermissionToAccessWorkspace().Returns(sourceWorkspacePermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View).Returns(integrationPointTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create)).Returns(jobHistoryAddPermission);
			_sourcePermissionRepository.UserCanImport().Returns(sourceImportPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })))
				.Returns(destinationRdoPermissions);

			var sourceProviderGuid = new Guid(ObjectTypeGuids.SourceProvider);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is(sourceProviderGuid), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(Arg.Is(sourceProviderGuid), Arg.Is(_SOURCE_PROVIDER_ID), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderInstanceViewPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(_DESTINATION_PROVIDER_GUID, ArtifactPermission.View).Returns(destinationProviderViewPermission);

			if (!sourceProviderIsProvided)
			{
				_sourceProviderRepository.Read(Arg.Is(_SOURCE_PROVIDER_ID))
					.Returns(new SourceProviderDTO() { Identifier = _otherProviderGuid });
			}

			var destinationProvider = new DestinationProviderDTO()
			{
				Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID)
			};
			_destinationProviderRepository.Read(_DESTINATION_PROVIDER_ID).Returns(destinationProvider);

			// ACT
			PermissionCheckDTO result = _testInstance.UserHasPermissionToSaveIntegrationPoint(_SOURCE_WORKSPACE_ID, integrationPointDto, sourceProviderIsProvided ? Constants.SourceProvider.Other : (Constants.SourceProvider?)null);

			// ASSERT	
			bool expectedSuccessValue =
				sourceWorkspacePermission &&
				integrationPointTypeViewPermission &&
				integrationPointInstanceViewPermission &&
				jobHistoryAddPermission &&
				sourceImportPermission &&
				destinationRdoPermissions &&
				sourceProviderTypeViewPermission &&
				sourceProviderInstanceViewPermission &&
				integrationPointTypeEditOrCreatePermission &&
				integrationPointInstanceEditOrCreatePermission;

			var errorMessages = new List<string>();
			if (isNew)
			{
				if (!integrationPointTypeEditOrCreatePermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_CREATE);
				}
			}
			else
			{
				if (!integrationPointTypeEditOrCreatePermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_EDIT);
				}
				if (!integrationPointInstanceEditOrCreatePermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_EDIT);
				}
			}

			if (!sourceWorkspacePermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.CURRENT_WORKSPACE_NO_ACCESS);
			}

			if (!integrationPointTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_VIEW);
			}

			if (!integrationPointInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_VIEW);
			}

			if (!destinationProviderViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_INSTANCE_VIEW);
			}

			if (!jobHistoryAddPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_TYPE_NO_ADD);
			}

			if (!sourceImportPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);
			}

			if (!destinationRdoPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}


			Assert.AreEqual(expectedSuccessValue, result.Success, $"The result success should be {expectedSuccessValue}.");
			Assert.AreEqual(errorMessages, result.ErrorMessages, "The error messages should match.");

			_sourcePermissionRepository.Received(isNew ? 0 : 1).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit);
			_sourcePermissionRepository.Received(isNew ? 0 : 1).UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Edit);
			_sourcePermissionRepository.Received(isNew ? 1 : 0).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create);
			_sourcePermissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create));
			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(sourceProviderGuid, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(sourceProviderGuid, integrationPointDto.SourceProvider.Value, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserCanImport();
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })));
			_sourceProviderRepository.Received(sourceProviderIsProvided ? 0 : 1).Read(Arg.Is(_SOURCE_PROVIDER_ID));
		}


		private void UserHasPermissionToSaveIntegrationPoint_RelativityProvider_GoldFlow_Cases(
			bool isNew,
			bool integrationPointTypeEditOrCreatePermission,
			bool integrationPointInstanceEditOrCreatePermission,
			bool sourceWorkspacePermission,
			bool integrationPointTypeViewPermission,
			bool integrationPointInstanceViewPermission,
			bool jobHistoryAddPermission,
			bool destinationRdoPermissions,
			bool destinationWorkspacePermission,
			bool destinationImportPermission,
			bool exportPermission,
			bool sourceDocumentEditPermissions,
			bool savedSearchPermissions,
			bool savedSearchIsPublic,
			bool sourceProviderIsProvided,
			bool sourceProviderTypeViewPermission,
			bool sourceProviderInstanceViewPermission,
			bool destinationProviderViewPermission)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = isNew ? 0 : INTEGRATION_POINT_ID,
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceConfiguration = $"{{ \"SavedSearchArtifactId\":{_SAVED_SEARCH_ID}, \"SourceWorkspaceArtifactId\":{_SOURCE_WORKSPACE_ID}, \"TargetWorkspaceArtifactId\":{_DESTINATION_WORKSPACE_ID} }}",
				SourceProvider = _SOURCE_PROVIDER_ID,
				DestinationProvider = _DESTINATION_PROVIDER_ID
			};

			var integrationPointObjectTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint);
			if (isNew)
			{
				_sourcePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create).Returns(integrationPointTypeEditOrCreatePermission);
			}
			else
			{
				_sourcePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit).Returns(integrationPointTypeEditOrCreatePermission);
				_sourcePermissionRepository.UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Edit).Returns(integrationPointInstanceEditOrCreatePermission);
			}

			_sourcePermissionRepository.UserHasPermissionToAccessWorkspace().Returns(sourceWorkspacePermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View).Returns(integrationPointTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			var sourceProviderGuid = new Guid(ObjectTypeGuids.SourceProvider);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is(sourceProviderGuid), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(Arg.Is(sourceProviderGuid), Arg.Is(_SOURCE_PROVIDER_ID), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderInstanceViewPermission);
			_destinationPermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })))
				.Returns(destinationRdoPermissions);
			_sourcePermissionRepository.UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create)).Returns(jobHistoryAddPermission);
			_sourcePermissionRepository.UserCanExport().Returns(exportPermission);
			_destinationPermissionRepository.UserHasPermissionToAccessWorkspace().Returns(destinationWorkspacePermission);
			_destinationPermissionRepository.UserCanImport().Returns(destinationImportPermission);
			_sourcePermissionRepository.UserCanEditDocuments().Returns(sourceDocumentEditPermissions);
			_sourcePermissionRepository.UserHasArtifactTypePermission(_DESTINATION_PROVIDER_GUID, ArtifactPermission.View).Returns(destinationProviderViewPermission);

			SavedSearchDTO savedSearchDto = null;
			if (savedSearchPermissions)
			{
				savedSearchDto = new SavedSearchDTO() { Owner = savedSearchIsPublic ? String.Empty : "KWUUUUUU" };
			}
			_savedSearchRepository.RetrieveSavedSearch().Returns(savedSearchDto);

			if (!sourceProviderIsProvided)
			{
				_sourceProviderRepository.Read(Arg.Is(_SOURCE_PROVIDER_ID))
					.Returns(new SourceProviderDTO() { Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID) });
				_destinationProviderRepository.Read(Arg.Is(_DESTINATION_PROVIDER_ID))
					.Returns(new DestinationProviderDTO() { Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID) });
			}

			var destinationProvider = new DestinationProviderDTO()
			{
				Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID)
			};
			_destinationProviderRepository.Read(_DESTINATION_PROVIDER_ID).Returns(destinationProvider);

			// ACT
			PermissionCheckDTO result = _testInstance.UserHasPermissionToSaveIntegrationPoint(_SOURCE_WORKSPACE_ID, integrationPointDto, sourceProviderIsProvided ? Constants.SourceProvider.Relativity : (Constants.SourceProvider?)null);

			// ASSERT	
			bool expectedSuccessValue =
				sourceWorkspacePermission &&
				integrationPointTypeViewPermission &&
				integrationPointInstanceViewPermission &&
				jobHistoryAddPermission &&
				destinationRdoPermissions &&
				destinationWorkspacePermission &&
				destinationImportPermission &&
				exportPermission &&
				sourceDocumentEditPermissions &&
				savedSearchPermissions &&
				savedSearchIsPublic &&
				integrationPointTypeEditOrCreatePermission &&
				integrationPointInstanceEditOrCreatePermission;

			var errorMessages = new List<string>();
			if (isNew)
			{
				if (!integrationPointTypeEditOrCreatePermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_CREATE);
				}
			}
			else
			{
				if (!integrationPointTypeEditOrCreatePermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_EDIT);
				}
				if (!integrationPointInstanceEditOrCreatePermission)
				{
					errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_EDIT);
				}
			}

			if (!sourceWorkspacePermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.CURRENT_WORKSPACE_NO_ACCESS);
			}

			if (!integrationPointTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_VIEW);
			}

			if (!integrationPointInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_VIEW);
			}

			if (!destinationProviderViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_INSTANCE_VIEW);
			}

			if (!jobHistoryAddPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_TYPE_NO_ADD);
			}

			if (!destinationRdoPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			if (!destinationWorkspacePermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS);
			}

			if (!destinationImportPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
			}

			if (!exportPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
			}

			if (!sourceDocumentEditPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			if (!savedSearchPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
			}
			else if (!savedSearchIsPublic)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NOT_PUBLIC);
			}


			Assert.AreEqual(expectedSuccessValue, result.Success, $"The result success should be {expectedSuccessValue}.");
			Assert.AreEqual(errorMessages, result.ErrorMessages, "The error messages should match.");

			_sourcePermissionRepository.Received(isNew ? 0 : 1).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit);
			_sourcePermissionRepository.Received(isNew ? 0 : 1).UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.Edit);
			_sourcePermissionRepository.Received(isNew ? 1 : 0).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create);
			_sourcePermissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_sourcePermissionRepository.Received(1)
				.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(sourceProviderGuid, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(sourceProviderGuid, integrationPointDto.SourceProvider.Value, ArtifactPermission.View);
			_destinationPermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })));
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create));
			_sourcePermissionRepository.Received(1).UserCanExport();
			_destinationPermissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_destinationPermissionRepository.Received(1).UserCanImport();
			_sourcePermissionRepository.Received(1).UserCanEditDocuments();
			_savedSearchRepository.Received(1).RetrieveSavedSearch();
			_sourceProviderRepository.Received(sourceProviderIsProvided ? 0 : 1).Read(Arg.Is(_SOURCE_PROVIDER_ID));
		}

		private void UserHasPermissionToRunJob_NonRelativityProvider_GoldFlow_Cases(
			bool sourceWorkspacePermission, 
			bool integrationPointTypeViewPermission, 
			bool integrationPointInstanceViewPermission,
			bool jobHistoryAddPermission,
			bool sourceImportPermission,
			bool destinationRdoPermissions,
			bool sourceProviderIsProvided,
			bool sourceProviderTypeViewPermission,
			bool sourceProviderInstanceViewPermission,
			bool destinationProviderRdoPermission)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = INTEGRATION_POINT_ID,
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceProvider = _SOURCE_PROVIDER_ID,
				DestinationProvider = _DESTINATION_PROVIDER_ID
			};

			_sourcePermissionRepository.UserHasPermissionToAccessWorkspace().Returns(sourceWorkspacePermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View).Returns(integrationPointTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create)).Returns(jobHistoryAddPermission);
			_sourcePermissionRepository.UserCanImport().Returns(sourceImportPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })))
				.Returns(destinationRdoPermissions);

			var sourceProviderGuid = new Guid(ObjectTypeGuids.SourceProvider);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is(sourceProviderGuid), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(Arg.Is(sourceProviderGuid), Arg.Is(_SOURCE_PROVIDER_ID), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderInstanceViewPermission);

			_sourcePermissionRepository.UserHasArtifactTypePermission(_DESTINATION_PROVIDER_GUID, ArtifactPermission.View).Returns(destinationProviderRdoPermission);

			if (!sourceProviderIsProvided)
			{
				_sourceProviderRepository.Read(Arg.Is(_SOURCE_PROVIDER_ID))
					.Returns(new SourceProviderDTO() {Identifier = _otherProviderGuid});
			}

			var destinationProvider = new DestinationProviderDTO()
			{
				Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID)
			};
			_destinationProviderRepository.Read(_DESTINATION_PROVIDER_ID).Returns(destinationProvider);

			// ACT
			PermissionCheckDTO result = _testInstance.UserHasPermissionToRunJob(_SOURCE_WORKSPACE_ID, integrationPointDto, sourceProviderIsProvided ? Constants.SourceProvider.Other : (Constants.SourceProvider?) null);

			// ASSERT	
			bool expectedSuccessValue = 
				sourceWorkspacePermission && 
				integrationPointTypeViewPermission &&
			    integrationPointInstanceViewPermission &&
				jobHistoryAddPermission &&
				sourceImportPermission &&
				destinationRdoPermissions && 
				sourceProviderTypeViewPermission &&
				sourceProviderInstanceViewPermission;

			var errorMessages = new List<string>();
			if (!sourceWorkspacePermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.CURRENT_WORKSPACE_NO_ACCESS);
			}

			if (!integrationPointTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_VIEW);
			}

			if (!integrationPointInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_VIEW);
			}

			if (!destinationProviderRdoPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_INSTANCE_VIEW);
			}

			if (!jobHistoryAddPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_TYPE_NO_ADD);
			}

			if (!sourceImportPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);
			}

			if (!destinationRdoPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}


			Assert.AreEqual(expectedSuccessValue, result.Success, $"The result success should be {expectedSuccessValue}.");
			Assert.AreEqual(errorMessages, result.ErrorMessages, "The error messages should match.");

			_sourcePermissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create));
			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(sourceProviderGuid, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(sourceProviderGuid, integrationPointDto.SourceProvider.Value, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserCanImport();
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })));
			_sourceProviderRepository.Received(sourceProviderIsProvided ? 0 : 1).Read(Arg.Is(_SOURCE_PROVIDER_ID));
			_destinationProviderRepository.Received(sourceProviderIsProvided ? 1 : 2).Read(Arg.Is(_DESTINATION_PROVIDER_ID));
		}

		private void UserHasPermissionToRunJob_RelativityProvider_GoldFlow(
			bool sourceWorkspacePermission,
			bool integrationPointTypeViewPermission,
			bool integrationPointInstanceViewPermission,
			bool jobHistoryAddPermission,
			bool destinationRdoPermissions,
			bool destinationWorkspacePermission,
			bool destinationImportPermission,
			bool exportPermission,
			bool sourceDocumentEditPermissions,
			bool savedSearchPermissions,
			bool savedSearchIsPublic,
			bool sourceProviderIsProvided,
			bool sourceProviderTypeViewPermission,
			bool sourceProviderInstanceViewPermission,
			bool destinationProviderViewPermission)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = INTEGRATION_POINT_ID,
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceConfiguration = $"{{ \"SavedSearchArtifactId\":{_SAVED_SEARCH_ID}, \"SourceWorkspaceArtifactId\":{_SOURCE_WORKSPACE_ID}, \"TargetWorkspaceArtifactId\":{_DESTINATION_WORKSPACE_ID} }}",
				SourceProvider = _SOURCE_PROVIDER_ID,
				DestinationProvider = _DESTINATION_PROVIDER_ID
			};

			_sourcePermissionRepository.UserHasPermissionToAccessWorkspace().Returns(sourceWorkspacePermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View).Returns(integrationPointTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			var sourceProviderGuid = new Guid(ObjectTypeGuids.SourceProvider);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is(sourceProviderGuid), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(Arg.Is(sourceProviderGuid), Arg.Is(_SOURCE_PROVIDER_ID), Arg.Is(ArtifactPermission.View)).Returns(sourceProviderInstanceViewPermission);
			_destinationPermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })))
				.Returns(destinationRdoPermissions);
			_sourcePermissionRepository.UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create)).Returns(jobHistoryAddPermission);
			_sourcePermissionRepository.UserCanExport().Returns(exportPermission);
			_destinationPermissionRepository.UserHasPermissionToAccessWorkspace().Returns(destinationWorkspacePermission);
			_destinationPermissionRepository.UserCanImport().Returns(destinationImportPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(_DESTINATION_PROVIDER_GUID, ArtifactPermission.View).Returns(destinationProviderViewPermission);
			_sourcePermissionRepository.UserCanEditDocuments().Returns(sourceDocumentEditPermissions);

			SavedSearchDTO savedSearchDto = null;
			if (savedSearchPermissions)
			{
				savedSearchDto = new SavedSearchDTO() {Owner = savedSearchIsPublic ? String.Empty : "KWUUUUUU"};
			}
			_savedSearchRepository.RetrieveSavedSearch().Returns(savedSearchDto);

			if (!sourceProviderIsProvided)
			{
				_sourceProviderRepository.Read(Arg.Is(_SOURCE_PROVIDER_ID))
					.Returns(new SourceProviderDTO() { Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID) });
				_destinationProviderRepository.Read(Arg.Is(_DESTINATION_PROVIDER_ID))
					.Returns(new DestinationProviderDTO() { Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID) });
			}

			var destinationProvider = new DestinationProviderDTO()
			{
				Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID)
			};
			_destinationProviderRepository.Read(_DESTINATION_PROVIDER_ID).Returns(destinationProvider);

			// ACT
			PermissionCheckDTO result = _testInstance.UserHasPermissionToRunJob(_SOURCE_WORKSPACE_ID, integrationPointDto, sourceProviderIsProvided ? Constants.SourceProvider.Relativity : (Constants.SourceProvider?)null);

			// ASSERT	
			bool expectedSuccessValue =
				sourceWorkspacePermission &&
				integrationPointTypeViewPermission &&
				integrationPointInstanceViewPermission &&
				jobHistoryAddPermission &&
				destinationRdoPermissions &&
				destinationWorkspacePermission &&
				destinationImportPermission &&
				exportPermission &&
				sourceDocumentEditPermissions &&
				savedSearchPermissions &&
				savedSearchIsPublic;

			var errorMessages = new List<string>();
			if (!sourceWorkspacePermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.CURRENT_WORKSPACE_NO_ACCESS);
			}

			if (!integrationPointTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_TYPE_NO_VIEW);
			}

			if (!integrationPointInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_INSTANCE_NO_VIEW);
			}

			if (!destinationProviderViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderTypeViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_VIEW);
			}

			if (!sourceProviderInstanceViewPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_PROVIDER_NO_INSTANCE_VIEW);
			}

			if (!jobHistoryAddPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_TYPE_NO_ADD);
			}

			if (!destinationRdoPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			if (!destinationWorkspacePermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS);
			}

			if (!destinationImportPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
			}

			if (!exportPermission)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
			}

			if (!sourceDocumentEditPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			if (!savedSearchPermissions)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
			}
			else if (!savedSearchIsPublic)
			{
				errorMessages.Add(Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NOT_PUBLIC);
			}


			Assert.AreEqual(expectedSuccessValue, result.Success, $"The result success should be {expectedSuccessValue}.");
			Assert.AreEqual(errorMessages, result.ErrorMessages, "The error messages should match.");

			_sourcePermissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_sourcePermissionRepository.Received(1)
				.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(sourceProviderGuid, ArtifactPermission.View);
			_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(sourceProviderGuid, integrationPointDto.SourceProvider.Value, ArtifactPermission.View);
			_destinationPermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create })));
			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), Arg.Is(ArtifactPermission.Create));
			_sourcePermissionRepository.Received(1).UserCanExport();
			_destinationPermissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_destinationPermissionRepository.Received(1).UserCanImport();
			_sourcePermissionRepository.Received(1).UserCanEditDocuments();
			_savedSearchRepository.Received(1).RetrieveSavedSearch();
			_sourceProviderRepository.Received(sourceProviderIsProvided ? 0 : 1).Read(Arg.Is(_SOURCE_PROVIDER_ID));
			_destinationProviderRepository.Received(sourceProviderIsProvided ? 1 : 2).Read(Arg.Is(_DESTINATION_PROVIDER_ID));
		}

		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void UserHasPermissionToViewErrors_GoldFlow(bool hasJobHistoryViewPermission, bool hasJobHistoryErrorViewPermission)
		{
			// Arrange
			_sourcePermissionRepository.UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)),
				Arg.Is(ArtifactPermission.View)).
				Returns(hasJobHistoryViewPermission);

			_sourcePermissionRepository.UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistoryError)),
				Arg.Is(ArtifactPermission.View)).
				Returns(hasJobHistoryErrorViewPermission);

			// Act
			PermissionCheckDTO result = _testInstance.UserHasPermissionToViewErrors(_SOURCE_WORKSPACE_ID);

			// Assert	
			bool userHasAllPermissions = hasJobHistoryViewPermission && hasJobHistoryErrorViewPermission;
			Assert.AreEqual(userHasAllPermissions, result.Success, $"The result Success should be {userHasAllPermissions}");

			int errorCount = 0;
			if (!hasJobHistoryViewPermission)
			{
				errorCount++;
				Assert.IsTrue(result.ErrorMessages.Contains(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW), $"The error messages should contain \"{Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW}\"");
            }

			if (!hasJobHistoryErrorViewPermission)
			{
				errorCount++;
				Assert.IsTrue(result.ErrorMessages.Contains(Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_ERROR_NO_VIEW), $"The error messages should contain \"{Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_ERROR_NO_VIEW}\"");
			}

			Assert.AreEqual(errorCount, result.ErrorMessages.Length);

			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistory)),
				Arg.Is(ArtifactPermission.View));

			_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(
				Arg.Is(new Guid(ObjectTypeGuids.JobHistoryError)),
				Arg.Is(ArtifactPermission.View));
		}
	}
}
