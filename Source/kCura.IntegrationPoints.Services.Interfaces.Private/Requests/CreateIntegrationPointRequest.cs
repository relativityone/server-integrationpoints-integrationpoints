using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Models;
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
			using (IRSAPIClient rsapiClient = container.Resolve<IRSAPIClient>())
			{
				ICaseServiceContext caseServiceContext = container.Resolve<ICaseServiceContext>();
				SourceProvider sourceProvider = caseServiceContext.RsapiService.SourceProviderLibrary.Read(SourceProviderArtifactId);
				DestinationProvider destinationProvider = caseServiceContext.RsapiService.DestinationProviderLibrary.Read(DestinationProviderArtifactId);
				
				if (sourceProvider == null)
				{
					throw new Exception($"Invalid source provider received : {SourceProviderArtifactId}");
				}

				if (String.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID, sourceProvider.Identifier,
					StringComparison.OrdinalIgnoreCase)
					&& String.Equals(Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID, destinationProvider.Identifier,
					StringComparison.OrdinalIgnoreCase))
				{
					_settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(SourceConfiguration.ToString());
					if (_settings.SourceWorkspaceArtifactId != WorkspaceArtifactId)
					{
						throw new Exception("SourceWorkspaceArtifactId and WorkspaceArtifactId must be the same.");
					}
					GetSavedSearchesQuery query = new GetSavedSearchesQuery(rsapiClient);
					QueryResult result = query.ExecuteQuery();
					Artifact artifact = result.QueryArtifacts.FirstOrDefault(art => art.ArtifactID == _settings.SavedSearchArtifactId);
					if (artifact == null)
					{
						throw new Exception("No permission to see the saved search.");
					}

					if (DestinationConfiguration.ArtifactTypeId != (int) ArtifactType.Document)
					{
						throw new Exception("Relativity source provider only supports documents object.");
					}
				}
				else
				{
					throw new NotImplementedException();
				}
			}
		}

		public virtual void ValidateRequest()
		{
			if (WorkspaceArtifactId < 1)
			{
				throw new ArgumentException("WorkspaceArtifactId must be greater than 1.");
			}
			if (String.IsNullOrWhiteSpace(Name))
			{
				throw new ArgumentException("Name cannot be null or white space.");
			}
			if (SourceConfiguration == null)
			{
				throw new ArgumentException("SourceConfiguration is not set");
			}
			if (DestinationConfiguration == null)
			{
				throw new ArgumentException("DestinationConfiguration is not set");
			}
			if (FieldsMapped.Count == 0)
			{
				throw new ArgumentException("FieldsMapped must contains at least 1 FieldMap.");
			}

			if (FieldsMapped.FirstOrDefault(field => field.FieldMapType == FieldMapTypeEnum.Identifier) == null)
			{
				throw new ArgumentException("FieldsMapped must have an identifier field mapped.");
			}

			if (ScheduleRule == null)
			{
				ScheduleRule = new Scheduler();
				ScheduleRule.EnableScheduler = false;
			}
		}

		public virtual Core.Models.IntegrationPointModel CreateIntegrationPointModel(IWindsorContainer container)
		{
			//TODO this is hardcoded, because Kelper Service doesn't support creating Integration Points other than ECA
			//this should be changed after we extend Kepler Service
			var integrationPointTypeService = container.Resolve<IIntegrationPointTypeService>();
			var type = integrationPointTypeService.GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);

			DestinationConfiguration.Provider = "relativity";

			Core.Models.IntegrationPointModel returnValue = new Core.Models.IntegrationPointModel
			{
				SourceProvider = SourceProviderArtifactId,
				Name = Name,
				DestinationProvider = DestinationProviderArtifactId,
				SelectedOverwrite = SelectedOverwrite,
				SourceConfiguration = JsonConvert.SerializeObject(_settings),
				Destination = JsonConvert.SerializeObject(DestinationConfiguration),
				Map = JsonConvert.SerializeObject(FieldsMapped),
				Scheduler = ScheduleRule,
				Type = type.ArtifactId
			};

			return returnValue;
		}
	}
}