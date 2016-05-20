﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class IntegrationPointManagerTests
	{
		private IIntegrationPointManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IIntegrationPointRepository _integrationPointRepository;
		private ISourceProviderRepository _sourceProviderRepository;
		private IPermissionRepository _sourcePermissionRepository;
		private IPermissionRepository _destinationPermissionRepository;
		private ISavedSearchRepository _savedSearchRepository;
		private Guid _otherProviderGuid;

		private const int _SOURCE_WORKSPACE_ID = 100532;
		private const int _DESTINATION_WORKSPACE_ID = 349234;
		private const int INTEGRATION_POINT_ID = 101323;
		private const int _ARTIFACT_TYPE_ID = 1232;
		private const int _SOURCE_PROVIDER_ID = 39309;
		private const int _SAVED_SEARCH_ID = 9492;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_sourceProviderRepository = Substitute.For<ISourceProviderRepository>();
			_sourcePermissionRepository = Substitute.For<IPermissionRepository>();
			_destinationPermissionRepository = Substitute.For<IPermissionRepository>();
			_savedSearchRepository = Substitute.For<ISavedSearchRepository>();

			_otherProviderGuid = Guid.NewGuid();

			_repositoryFactory.GetIntegrationPointRepository(_SOURCE_WORKSPACE_ID).Returns(_integrationPointRepository);
			_repositoryFactory.GetPermissionRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_sourcePermissionRepository);
			_repositoryFactory.GetPermissionRepository(Arg.Is(_DESTINATION_WORKSPACE_ID)).Returns(_destinationPermissionRepository);
			_repositoryFactory.GetSourceProviderRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_sourceProviderRepository);
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
		public void UserHasPermissionToRunJob_NonRelativityProvider_AllCombinations()
		{
			int paramCount = 6;
			for (int i = 0; i < Math.Pow(2, paramCount); i++)
			{
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
					this.UserHasPermissionToRunJob_NonRelativityProvider_GoldFlow_Cases(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], inputs[5]);
				}
				catch (Exception e)
				{
					string message = $"UserHasPermissionsFailed with inputs {String.Join(",", inputs)}: {e.Message}";
					throw new Exception(message, e);
				}
			}
		}

		[Test]
		public void UserHasPermissionToRunJob_RelativityProvider_AllCombinations()
		{
			int paramCount = 11;
			for (int i = 0; i < Math.Pow(2, paramCount); i++)
			{
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
					this.UserHasPermissionToRunJob_RelativityProvider_GoldFlow(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], inputs[5], inputs[6], inputs[7], inputs[8], inputs[9], inputs[10]);
				}
				catch (Exception e)
				{
					string message = $"UserHasPermissionsFailed with intputs {String.Join(",", inputs)}: {e.Message}";
					throw new Exception(message, e);
				}
			}
		}

		private void UserHasPermissionToRunJob_NonRelativityProvider_GoldFlow_Cases(
			bool sourceWorkspacePermission, 
			bool integrationPointTypeViewPermission, 
			bool integrationPointInstanceViewPermission,
			bool sourceImportPermission,
			bool destinationRdoPermissions,
			bool sourceProviderIsProvided)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = INTEGRATION_POINT_ID,
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceProvider = _SOURCE_PROVIDER_ID
			};

			_sourcePermissionRepository.UserHasPermissionToAccessWorkspace().Returns(sourceWorkspacePermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View).Returns(integrationPointTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			_sourcePermissionRepository.UserCanImport().Returns(sourceImportPermission);
			_sourcePermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Add })))
				.Returns(destinationRdoPermissions);

			if (!sourceProviderIsProvided)
			{
				_sourceProviderRepository.Read(Arg.Is(_SOURCE_PROVIDER_ID))
					.Returns(new SourceProviderDTO() {Identifier = _otherProviderGuid});
			}

			// ACT
			PermissionCheckDTO result = _testInstance.UserHasPermissionToRunJob(_SOURCE_WORKSPACE_ID, integrationPointDto, sourceProviderIsProvided ? Constants.SourceProvider.Other : (Constants.SourceProvider?) null);

			// ASSERT	
			bool expectedSuccessValue = 
				sourceWorkspacePermission && 
				integrationPointTypeViewPermission &&
			    integrationPointInstanceViewPermission && 
				sourceImportPermission &&
				destinationRdoPermissions;

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
		}

		private void UserHasPermissionToRunJob_RelativityProvider_GoldFlow(
			bool sourceWorkspacePermission,
			bool integrationPointTypeViewPermission,
			bool integrationPointInstanceViewPermission,
			bool destinationRdoPermissions,
			bool destinationWorkspacePermission,
			bool destinationImportPermission,
			bool exportPermission,
			bool sourceDocumentEditPermissions,
			bool savedSearchPermissions,
			bool savedSearchIsPublic,
			bool sourceProviderIsProvided)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = INTEGRATION_POINT_ID,
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceConfiguration = $"{{ \"SavedSearchArtifactId\":{_SAVED_SEARCH_ID}, \"SourceWorkspaceArtifactId\":{_SOURCE_WORKSPACE_ID}, \"TargetWorkspaceArtifactId\":{_DESTINATION_WORKSPACE_ID} }}",
				SourceProvider = _SOURCE_PROVIDER_ID
			};

			_sourcePermissionRepository.UserHasPermissionToAccessWorkspace().Returns(sourceWorkspacePermission);
			_sourcePermissionRepository.UserHasArtifactTypePermission(Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, ArtifactPermission.View).Returns(integrationPointTypeViewPermission);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(
				Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, integrationPointDto.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
			_destinationPermissionRepository.UserHasArtifactTypePermissions(
				Arg.Is(_ARTIFACT_TYPE_ID),
				Arg.Is<ArtifactPermission[]>(x => x.SequenceEqual(new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Add })))
				.Returns(destinationRdoPermissions);
			_sourcePermissionRepository.UserCanExport().Returns(exportPermission);
			_destinationPermissionRepository.UserHasPermissionToAccessWorkspace().Returns(destinationWorkspacePermission);
			_destinationPermissionRepository.UserCanImport().Returns(destinationImportPermission);
			_sourcePermissionRepository.UserCanEditDocuments().Returns(sourceDocumentEditPermissions);

			SavedSearchDTO savedSearchDto = null;
			if (savedSearchPermissions)
			{
				savedSearchDto = new SavedSearchDTO() {Owner = savedSearchIsPublic ? 0 : 123};
			}
			_savedSearchRepository.RetrieveSavedSearch().Returns(savedSearchDto);

			if (!sourceProviderIsProvided)
			{
				_sourceProviderRepository.Read(Arg.Is(_SOURCE_PROVIDER_ID))
					.Returns(new SourceProviderDTO() { Identifier = new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID) });
			}

			// ACT
			PermissionCheckDTO result = _testInstance.UserHasPermissionToRunJob(_SOURCE_WORKSPACE_ID, integrationPointDto, sourceProviderIsProvided ? Constants.SourceProvider.Relativity : (Constants.SourceProvider?)null);

			// ASSERT	
			bool expectedSuccessValue =
				sourceWorkspacePermission &&
				integrationPointTypeViewPermission &&
				integrationPointInstanceViewPermission &&
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
		}
	}
}
