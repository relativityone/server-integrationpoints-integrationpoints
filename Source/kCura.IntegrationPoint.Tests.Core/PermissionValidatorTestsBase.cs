using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	internal static class ValidationResultExtensions
	{
		internal static void Check(this ValidationResult result, bool expected)
		{
			Assert.That(result.IsValid, Is.EqualTo(expected));

			if (expected)
			{
				Assert.That(result.Messages.Count, Is.Zero);
			}
			else
			{
				Assert.That(result.Messages.Count, Is.Positive);
			}
		}
	}

	public class PermissionValidatorTestsBase
	{
		protected const int _SOURCE_WORKSPACE_ID = 100532;
		protected const int _DESTINATION_WORKSPACE_ID = 349234;
		protected const int INTEGRATION_POINT_ID = 101323;
		protected const int _ARTIFACT_TYPE_ID = 1232;
		protected const int _SOURCE_PROVIDER_ID = 39309;
		protected const int _DESTINATION_PROVIDER_ID = 42042;
		protected const int _SAVED_SEARCH_ID = 9492;
		protected Guid _SOURCE_PROVIDER_GUID = new Guid(ObjectTypeGuids.SourceProvider);
		protected Guid _DESTINATION_PROVIDER_GUID = new Guid(ObjectTypeGuids.DestinationProvider);

		protected IRepositoryFactory _repositoryFactory;
		protected IPermissionRepository _sourcePermissionRepository;
		protected IPermissionRepository _destinationPermissionRepository;
		protected ISerializer _serializer;
		protected IServiceContextHelper ServiceContextHelper;
		protected IntegrationPointProviderValidationModel _validationModel;

		[SetUp]
		public void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_sourcePermissionRepository = Substitute.For<IPermissionRepository>();
			_destinationPermissionRepository = Substitute.For<IPermissionRepository>();
			ServiceContextHelper = Substitute.For<IServiceContextHelper>();
			ServiceContextHelper.WorkspaceID.Returns(_SOURCE_WORKSPACE_ID);
			_repositoryFactory.GetPermissionRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_sourcePermissionRepository);
			_repositoryFactory.GetPermissionRepository(Arg.Is(_DESTINATION_WORKSPACE_ID)).Returns(_destinationPermissionRepository);
			_serializer = Substitute.For<ISerializer>();

			_validationModel = new IntegrationPointProviderValidationModel()
			{
				ArtifactId = INTEGRATION_POINT_ID,
				SourceConfiguration = $"{{ \"SavedSearchArtifactId\":{_SAVED_SEARCH_ID}, \"SourceWorkspaceArtifactId\":{_SOURCE_WORKSPACE_ID}, \"TargetWorkspaceArtifactId\":{_DESTINATION_WORKSPACE_ID} }}",
				DestinationConfiguration = $"{{ \"artifactTypeID\": {_ARTIFACT_TYPE_ID} }}",
				SourceProviderArtifactId = _SOURCE_PROVIDER_ID,
				DestinationProviderArtifactId = _DESTINATION_PROVIDER_ID
			};

			_serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
				.Returns(new SourceConfiguration()
				{
					SavedSearchArtifactId = _SAVED_SEARCH_ID,
					SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
					TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID
				});

			_serializer.Deserialize<DestinationConfiguration>(_validationModel.DestinationConfiguration)
				.Returns(new DestinationConfiguration() { ArtifactTypeId = _ARTIFACT_TYPE_ID });
		}
	}
}
