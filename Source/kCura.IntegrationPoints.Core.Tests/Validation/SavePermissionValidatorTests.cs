using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class SavePermissionValidatorTests : PermissionValidatorTestsBase
	{
		//CREATE
		[TestCase(true, true, true, true, true)]
		[TestCase(true, false, false, true, true)]
		[TestCase(true, true, true, false, false)]
		//EDIT
		[TestCase(false, true, true, true, true)]
		[TestCase(false, true, true, false, true)]
		[TestCase(false, false, true, true, false)]
		[TestCase(false, true, false, true, false)]
		public void ValidateTest(bool isNew, bool typeEdit, bool instanceEdit, bool typeCreate, bool expected)
		{
			// arrange
			var integrationPointObjectTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint);
			_sourcePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit).Returns(typeEdit);
			_sourcePermissionRepository.UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.Edit).Returns(instanceEdit);
			_sourcePermissionRepository.UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create).Returns(typeCreate);

			var savePermissionValidator = new SavePermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper);

			if (isNew)
			{
				_validationModel.ArtifactId = 0;
			}

			// act
			var validationResult = savePermissionValidator.Validate(_validationModel);

			// assert
			if (isNew)
			{
				_sourcePermissionRepository.DidNotReceive().UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit);
				_sourcePermissionRepository.DidNotReceive().UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.Edit);

				_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create);
			}
			else
			{
				_sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Edit);
				_sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(integrationPointObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.Edit);

				_sourcePermissionRepository.DidNotReceive().UserHasArtifactTypePermission(integrationPointObjectTypeGuid, ArtifactPermission.Create);
			}

			validationResult.Check(expected);
		}
	}
}
