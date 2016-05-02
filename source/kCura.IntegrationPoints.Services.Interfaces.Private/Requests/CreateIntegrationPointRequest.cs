using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Services
{
	public class CreateIntegrationPointRequest : IRequiredPermission, IValidatable
	{
		private ExportUsingSavedSearchSettings _settings;

		public int WorkspaceArtifactId { get; set; }

		public int SourceProviderArtifactId { get; set; }

		public string Name { get; set; }

		public int DestinationProviderArtifactId { get; set; }

		public string SelectedOverwrite { get; set; }

		public object SourceConfiguration { get; set; }

		public DestinationConfiguration DestinationConfiguration { get; set; }

		public List<FieldMap> FieldsMapped { get; set; }

		public bool EnableScheduler { get; set; }

		public Scheduler ScheduleRule { get; set; }

		public virtual void ValidatePermission(IWindsorContainer container)
		{
			IRSAPIClient raspiClient = container.Resolve<IRSAPIClient>();
			ICaseServiceContext caseServiceContext = container.Resolve<ICaseServiceContext>();
			SourceProvider sourceProvider = caseServiceContext.RsapiService.SourceProviderLibrary.Read(SourceProviderArtifactId);
			if (sourceProvider == null)
			{
				throw new Exception($"Invalid source provider received : {SourceProviderArtifactId}");
			}

			if (String.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID , sourceProvider.Identifier, StringComparison.OrdinalIgnoreCase))
			{
				_settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(SourceConfiguration.ToString());
				IPermissionService permission = container.Resolve<IPermissionService>();
				if (permission.UserCanImport(_settings.TargetWorkspaceArtifactId) == false)
				{
					throw new Exception($"User ain't got the permission to push to the workspace : {_settings.TargetWorkspaceArtifactId}");
				}
				if (_settings.SourceWorkspaceArtifactId != WorkspaceArtifactId)
				{
					throw new Exception("Duh f**k are you trying to do ?");
				}
				GetSavedSearchesQuery query = new GetSavedSearchesQuery(raspiClient);
				QueryResult result = query.ExecuteQuery();
				Artifact artifact = result.QueryArtifacts.FirstOrDefault(art => art.ArtifactID == _settings.SavedSearchArtifactId);
				if (artifact == null)
				{
					throw new Exception("No permission to see saved search");
				}

				if (DestinationConfiguration.ArtifactTypeId != (int)ArtifactType.Document)
				{
					throw new Exception("We only support documents object, remember ?");
				}
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public virtual void ValidateRequest()
		{
			if (WorkspaceArtifactId < 1)
			{
				throw new ArgumentException("WorkspaceArtifactId");
			}
			if (String.IsNullOrWhiteSpace(Name))
			{
				throw new ArgumentException("Name");
			}
			if (SourceConfiguration == null)
			{
				throw new ArgumentException("SourceConfiguration");
			}
			if (DestinationConfiguration == null)
			{
				throw new ArgumentException("DestinationConfiguration");
			}
			if (FieldsMapped.Count == 0)
			{
				throw new ArgumentException("FieldsMapped");
			}

			if (FieldsMapped.FirstOrDefault(field => field.FieldMapType == FieldMapTypeEnum.Identifier) == null)
			{
				throw new ArgumentException("FieldsMapped");
			}

			if (ScheduleRule == null)
			{
				ScheduleRule = new Scheduler();
				ScheduleRule.EnableScheduler = false;
			}
		}

		public virtual IntegrationModel CreateIntegrationPointModel()
		{
			DestinationConfiguration.Provider = "relativity";

			IntegrationModel returnValue = new IntegrationModel
			{
				SourceProvider = SourceProviderArtifactId,
				Name = Name,
				DestinationProvider = DestinationProviderArtifactId,
				SelectedOverwrite = SelectedOverwrite,
				SourceConfiguration = JsonConvert.SerializeObject(_settings),
				Destination = JsonConvert.SerializeObject(DestinationConfiguration),
				Map = JsonConvert.SerializeObject(FieldsMapped),
				Scheduler = ScheduleRule
			};

			return returnValue;
		}
	}
}