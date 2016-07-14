﻿using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	public class RelativityProviderTemplate : SourceProviderTemplate
	{
		private readonly string _targetWorkspaceName;
		private readonly string _targetWorkspaceTemplate;
		private string _destinationConfig;
		protected SourceProvider RelativityProvider;
		protected SourceProvider LdapProvider;

		public int SourceWorkspaceArtifactId { get; protected set; }
		public int TargetWorkspaceArtifactId { get; protected set; }
		public int SavedSearchArtifactId { get; set; }

		public RelativityProviderTemplate(string sourceWorkspaceName, string targetWorkspaceName,
			string sourceWorkspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE,
			string targetWorkspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE)
			: base(sourceWorkspaceName, sourceWorkspaceTemplate)
		{
			_targetWorkspaceName = targetWorkspaceName;
			_targetWorkspaceTemplate = targetWorkspaceTemplate;
		}

		[TestFixtureSetUp]
		public new void SuiteSetup()
		{
			SourceWorkspaceArtifactId = WorkspaceArtifactId;
	
			Task.Run(async () => await SetupAsync()).Wait();
			
			RelativityProvider = SourceProviders.First(provider => provider.Name == "Relativity");
			LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");
		}

		[TestFixtureTearDown]
		public new void SuiteTeardown()
		{
			Workspace.DeleteWorkspace(TargetWorkspaceArtifactId);
		}

		#region Helper Methods

		protected string CreateDefaultSourceConfig()
		{
			if (_destinationConfig == null)
			{
				_destinationConfig = $"{{\"SavedSearchArtifactId\":{SavedSearchArtifactId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{TargetWorkspaceArtifactId}}}";
			}
			return _destinationConfig;
		}

		protected string CreateDefaultDestinationConfig()
		{
			ImportSettings destinationConfig = new ImportSettings
			{
				ArtifactTypeId = 10,
				CaseArtifactId = SourceWorkspaceArtifactId,
				Provider = "Relativity",
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				FieldOverlayBehavior = "Use Field Settings",
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword
			};
			return Container.Resolve<ISerializer>().Serialize(destinationConfig);
		}

		protected string CreateDefaultFieldMap()
		{
			FieldMap[] map = GetDefaultFieldMap();
			return Container.Resolve<ISerializer>().Serialize(map);
		}

		protected FieldMap[] GetDefaultFieldMap()
		{
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IFieldRepository sourceFieldRepository = repositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId);
			IFieldRepository destinationFieldRepository = repositoryFactory.GetFieldRepository(TargetWorkspaceArtifactId);

			ArtifactDTO sourceDto = sourceFieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);
			ArtifactDTO targetDto = destinationFieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);

			FieldMap[] map = new[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = sourceDto.ArtifactId.ToString(),
						DisplayName = sourceDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true,
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = targetDto.ArtifactId.ToString(),
						DisplayName = targetDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true,
					},
				}
			};
			return map;
		}

		protected async Task SetupAsync()
		{
			TargetWorkspaceArtifactId = String.IsNullOrEmpty(_targetWorkspaceName)
				? SourceWorkspaceArtifactId
				: await Task.Run(() => Workspace.CreateWorkspace(_targetWorkspaceName, _targetWorkspaceTemplate));

			SavedSearchArtifactId = await Task.Run(() => SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, "All documents"));
		}

		#endregion
	}
}
