using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class FieldMaps
	{
		private static readonly IRepositoryFactory _repositoryFactory;
		public static ITestHelper Helper => _help.Value;
		private static readonly Lazy<ITestHelper> _help;

		static FieldMaps()
		{
			IWindsorContainer container = new WindsorContainer();
			_help = new Lazy<ITestHelper>(() => new TestHelper());

			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			//if (container.Kernel.HasComponent(typeof(IHelper)))
			//{
			container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleSingleton());
			//}

			_repositoryFactory = container.Resolve<IRepositoryFactory>();
		}

		public static FieldMap CreateIdentifierFieldMapForRelativityProvider(int sourceWorkspaceId, int destinationWorkspaceId)
		{
			IFieldQueryRepository sourceFieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(sourceWorkspaceId);
			IFieldQueryRepository destinationFieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(destinationWorkspaceId);

			ArtifactDTO documentIdentifierFieldInSource = sourceFieldQueryRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);
			ArtifactDTO documentIdentifierFieldInDestination = destinationFieldQueryRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);

			FieldMap identifierFieldMap = CreateFieldMapForRelativityProvider(documentIdentifierFieldInSource, documentIdentifierFieldInDestination, FieldMapTypeEnum.Identifier);

			return identifierFieldMap;
		}

		public static FieldMap CreateFieldMapForRelativityProvider(ArtifactDTO sourceField, ArtifactDTO destinationField, FieldMapTypeEnum fieldMapType)
		{
			FieldMap fieldMap = new FieldMap
			{
				FieldMapType = fieldMapType,
				SourceField = new FieldEntry()
				{
					DisplayName = (string)sourceField.Fields[0].Value,
					IsIdentifier = fieldMapType == FieldMapTypeEnum.Identifier,
					FieldIdentifier = sourceField.ArtifactId.ToString(),
				},
				DestinationField = new FieldEntry()
				{
					DisplayName = (string)destinationField.Fields[0].Value,
					IsIdentifier = fieldMapType == FieldMapTypeEnum.Identifier,
					FieldIdentifier = destinationField.ArtifactId.ToString(),
				}
			};

			return fieldMap;
		}
	}
}