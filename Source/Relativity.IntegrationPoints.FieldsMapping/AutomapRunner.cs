using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Metrics;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Search;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class AutomapRunner : IAutomapRunner
	{
		private const string _UNIT_OF_MEASURE = "field(s)";
		private const string _AUTOMAP_ALL_METRIC_NAME = "AutoMapAll";
		private const string _AUTOMAP_SAVED_SEARCH_METRIC_NAME = "AutoMapSavedSearch";
		private const string _AUTOMAPPED_COUNT_METRIC_NAME = "AutoMappedCount";
		private const string _AUTOMAPPED_BY_ID_COUNT_METRIC_NAME = "AutoMappedByIdCount";
		private const string _AUTOMAPPED_BY_NAME_COUNT_METRIC_NAME = "AutoMappedByNameCount";
		private const string _AUTOMAPPED_FIXED_LENGTH_TEXTS_WITH_DIFFERENT_LENGTHS_METRIC_NAME = "AutoMappedByNameCount";

		private readonly IServicesMgr _servicesMgr;
		private readonly IMetricsSender _metrics;

		public AutomapRunner(IServicesMgr servicesMgr, IMetricsSender metrics)
		{
			_servicesMgr = servicesMgr;
			_metrics = metrics;
		}

		public IEnumerable<FieldMap> MapFields(IEnumerable<DocumentFieldInfo> sourceFields,
			IEnumerable<DocumentFieldInfo> destinationFields, bool matchOnlyIdentifiers = false)
		{
			_metrics.CountOperation(_AUTOMAP_ALL_METRIC_NAME);

			AutomapBuilder mappingBuilder = new AutomapBuilder(sourceFields, destinationFields, _metrics).MapByIsIdentifier();

			if (!matchOnlyIdentifiers)
			{
				mappingBuilder = mappingBuilder
					.MapBy(x => x.FieldIdentifier, out int mappedById, out int fixedLengthTextFieldsWithDifferentLengthByIdCount)
					.MapBy(x => x.Name, out int mappedByName, out int fixedLengthTextFieldsWithDifferentLengthByNameCount);

				_metrics.GaugeOperation(_AUTOMAPPED_BY_ID_COUNT_METRIC_NAME, mappedById, _UNIT_OF_MEASURE);
				_metrics.GaugeOperation(_AUTOMAPPED_BY_NAME_COUNT_METRIC_NAME, mappedByName, _UNIT_OF_MEASURE);
				_metrics.GaugeOperation(_AUTOMAPPED_FIXED_LENGTH_TEXTS_WITH_DIFFERENT_LENGTHS_METRIC_NAME, fixedLengthTextFieldsWithDifferentLengthByNameCount + fixedLengthTextFieldsWithDifferentLengthByIdCount, _UNIT_OF_MEASURE);
			}

			_metrics.GaugeOperation(_AUTOMAPPED_COUNT_METRIC_NAME, mappingBuilder.Mapping.Count(), _UNIT_OF_MEASURE);

			return mappingBuilder.Mapping.OrderByDescending(x => x.SourceField.IsIdentifier).ThenBy(x => x.SourceField.DisplayName);
		}

		public async Task<IEnumerable<FieldMap>> MapFieldsFromSavedSearchAsync(IEnumerable<DocumentFieldInfo> sourceFields,
			IEnumerable<DocumentFieldInfo> destinationFields, int sourceWorkspaceArtifactId, int savedSearchArtifactId)
		{
			_metrics.CountOperation(_AUTOMAP_SAVED_SEARCH_METRIC_NAME);

			List<DocumentFieldInfo> sourceFieldsList = sourceFields.ToList();
			List<DocumentFieldInfo> savedSearchFields;

			using (IKeywordSearchManager keywordSearchManager = _servicesMgr.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser))
			{
				KeywordSearch savedSearch = await keywordSearchManager.ReadSingleAsync(sourceWorkspaceArtifactId, savedSearchArtifactId)
					.ConfigureAwait(false);

				savedSearchFields = sourceFieldsList
					.Where(sourceField => savedSearch.Fields.Exists(savedSearchField =>
						savedSearchField.ArtifactID.ToString() == sourceField.FieldIdentifier))
					.ToList();
			}

			if (!savedSearchFields.Exists(x => x.IsIdentifier))
			{
				DocumentFieldInfo identifierField = sourceFieldsList.SingleOrDefault(x => x.IsIdentifier);
				if (identifierField != null)
				{
					savedSearchFields.Add(identifierField);
				}
			}

			return MapFields(savedSearchFields, destinationFields);
		}

		private class AutomapBuilder
		{
			public IEnumerable<FieldMap> Mapping { get; }
			private readonly IEnumerable<DocumentFieldInfo> _sourceFields;
			private readonly IEnumerable<DocumentFieldInfo> _destinationFields;
			private readonly IMetricsSender _metrics;

			public AutomapBuilder(IEnumerable<DocumentFieldInfo> sourceFields,
				IEnumerable<DocumentFieldInfo> destinationFields, IMetricsSender metrics, IEnumerable<FieldMap> mapping = null)
			{
				Mapping = mapping ?? new FieldMap[0];
				_sourceFields = sourceFields;
				_destinationFields = destinationFields;
				_metrics = metrics;
			}

			public AutomapBuilder MapBy<T>(Func<DocumentFieldInfo, T> selector, out int mappedCount, out int fixedLengthTextFieldsWithDifferentLengthCount)
			{
				var typeCompatibleFields = _sourceFields
					.Join(_destinationFields, selector, selector, (SourceField, DestinationField) => new { SourceField, DestinationField })
					.Where(x => x.SourceField.IsTypeCompatible(x.DestinationField))
					.ToArray();

				FieldMap[] newMappings = typeCompatibleFields
					.Select(x => new FieldMap
					{
						SourceField = FieldConvert.ToFieldEntry(x.SourceField),
						DestinationField = FieldConvert.ToFieldEntry(x.DestinationField),
						FieldMapType = (x.SourceField.IsIdentifier && x.DestinationField.IsIdentifier) ? FieldMapTypeEnum.Identifier : FieldMapTypeEnum.None
					})
					.ToArray();
				
				mappedCount = newMappings.Length;
				fixedLengthTextFieldsWithDifferentLengthCount = typeCompatibleFields.Count(x =>
					x.SourceField.Type == FieldTypeName.FIXED_LENGTH_TEXT &&
					x.SourceField.Length != x.DestinationField.Length);

				DocumentFieldInfo[] remainingSourceFields = _sourceFields.Where(x => !newMappings.Any(m => m.SourceField.FieldIdentifier == x.FieldIdentifier.ToString())).ToArray();
				DocumentFieldInfo[] remainingDestinationFields = _destinationFields.Where(x => !newMappings.Any(m => m.DestinationField.FieldIdentifier == x.FieldIdentifier.ToString())).ToArray();

				return new AutomapBuilder(
					remainingSourceFields,
					remainingDestinationFields,
					_metrics,
					Mapping.Concat(newMappings)
					);
			}

			public AutomapBuilder MapByIsIdentifier()
			{
				DocumentFieldInfo sourceIdentifier = _sourceFields.FirstOrDefault(x => x.IsIdentifier);
				DocumentFieldInfo destinationIdentifier = _destinationFields.FirstOrDefault(x => x.IsIdentifier);

				if (sourceIdentifier == null || destinationIdentifier == null || !sourceIdentifier.IsTypeCompatible(destinationIdentifier))
				{
					return new AutomapBuilder(_sourceFields.ToArray(), _destinationFields.ToArray(), _metrics, Mapping.ToArray());
				}

				return new AutomapBuilder(_sourceFields.Where(x => x != sourceIdentifier).ToArray(),
					_destinationFields.Where(x => x != destinationIdentifier).ToArray(),
					_metrics,
					Mapping.Concat(new FieldMap[]
					{
						new FieldMap
						{
							SourceField = FieldConvert.ToFieldEntry(sourceIdentifier),
							DestinationField = FieldConvert.ToFieldEntry(destinationIdentifier),
							FieldMapType = FieldMapTypeEnum.Identifier
						}
					})
				);
			}
		}
	}
}