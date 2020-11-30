﻿using System;
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
		private const string _AUTOMAPPED_COUNT_METRIC_NAME = "AutoMappedCount";
		private const string _AUTOMAPPED_BY_NAME_COUNT_METRIC_NAME = "AutoMappedByNameCount";
		private const string _AUTOMAPPED_FIXED_LENGTH_TEXTS_WITH_DIFFERENT_LENGTHS_METRIC_NAME = "FixedLengthTextTooShortInDestinationCount";

		private readonly IServicesMgr _servicesMgr;
		private readonly IMetricsSender _metrics;
		private readonly IMetricBucketNameGenerator _metricBucketNameGenerator;

		public AutomapRunner(IServicesMgr servicesMgr, IMetricsSender metrics, IMetricBucketNameGenerator metricBucketNameGenerator)
		{
			_servicesMgr = servicesMgr;
			_metrics = metrics;
			_metricBucketNameGenerator = metricBucketNameGenerator;
		}

		public IEnumerable<FieldMap> MapFields(IEnumerable<DocumentFieldInfo> sourceFields, IEnumerable<DocumentFieldInfo> destinationFields,
			string destinationProviderGuid, int sourceWorkspaceArtifactId, bool matchOnlyIdentifiers = false)
		{
			List<DocumentFieldInfo> sourceFieldsList = sourceFields.ToList();
			List<DocumentFieldInfo> destinationFieldsList = destinationFields.ToList();

			AutomapBuilder mappingBuilder = new AutomapBuilder(sourceFieldsList, destinationFieldsList).MapByIsIdentifier();

			if (!matchOnlyIdentifiers)
			{
				mappingBuilder = mappingBuilder
					.MapBy(x => x.Name, out int mappedByName, out int fixedLengthTextFieldsWithDifferentLengthByNameCount);

				string automappedByNameMetricName = _metricBucketNameGenerator.GetBucketNameAsync(_AUTOMAPPED_BY_NAME_COUNT_METRIC_NAME, Guid.Parse(destinationProviderGuid), sourceWorkspaceArtifactId).GetAwaiter().GetResult();
				string fixedLengthTextsMetricName = _metricBucketNameGenerator.GetBucketNameAsync(_AUTOMAPPED_FIXED_LENGTH_TEXTS_WITH_DIFFERENT_LENGTHS_METRIC_NAME, Guid.Parse(destinationProviderGuid), sourceWorkspaceArtifactId).GetAwaiter().GetResult();
				_metrics.GaugeOperation(automappedByNameMetricName, mappedByName, _UNIT_OF_MEASURE);
				_metrics.GaugeOperation(fixedLengthTextsMetricName, fixedLengthTextFieldsWithDifferentLengthByNameCount, _UNIT_OF_MEASURE);
			}

			string automappedCountMetricName = _metricBucketNameGenerator.GetBucketNameAsync(_AUTOMAPPED_COUNT_METRIC_NAME, Guid.Parse(destinationProviderGuid), sourceWorkspaceArtifactId).GetAwaiter().GetResult();
			_metrics.GaugeOperation(automappedCountMetricName, mappingBuilder.Mapping.Count(), _UNIT_OF_MEASURE);

			return mappingBuilder
				.Mapping
				.OrderByDescending(x => x.SourceField.IsIdentifier)
				.ThenBy(x => x.SourceField.DisplayName);
		}

		public async Task<IEnumerable<FieldMap>> MapFieldsFromSavedSearchAsync(IEnumerable<DocumentFieldInfo> sourceFields,
			IEnumerable<DocumentFieldInfo> destinationFields, string destinationProviderGuid, int sourceWorkspaceArtifactId, int savedSearchArtifactId)
		{
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

			List<FieldMap> mappedFields = MapFields(savedSearchFields, destinationFields, destinationProviderGuid, sourceWorkspaceArtifactId).ToList();
			return mappedFields;
		}

		private class AutomapBuilder
		{
			public IEnumerable<FieldMap> Mapping { get; }
			private readonly IEnumerable<DocumentFieldInfo> _sourceFields;
			private readonly IEnumerable<DocumentFieldInfo> _destinationFields;

			public AutomapBuilder(IEnumerable<DocumentFieldInfo> sourceFields,
				IEnumerable<DocumentFieldInfo> destinationFields, IEnumerable<FieldMap> mapping = null)
			{
				Mapping = mapping ?? new FieldMap[0];
				_sourceFields = sourceFields;
				_destinationFields = destinationFields;
			}

			public AutomapBuilder MapBy<T>(Func<DocumentFieldInfo, T> selector, out int mappedCount, out int fixedLengthTextFieldsWithDifferentLengthCount)
			{
				var fieldPairs = _sourceFields
					.Join(_destinationFields, selector, selector, (SourceField, DestinationField) => new { SourceField, DestinationField })
					.ToArray();

				fixedLengthTextFieldsWithDifferentLengthCount = fieldPairs.Count(x =>
					x.SourceField.Type.StartsWith(FieldTypeName.FIXED_LENGTH_TEXT) &&
					x.SourceField.Length != x.DestinationField.Length);

				var typeCompatibleFields = fieldPairs
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

				DocumentFieldInfo[] remainingSourceFields = _sourceFields.Where(x => newMappings.All(m => m.SourceField.FieldIdentifier != x.FieldIdentifier.ToString())).ToArray();
				DocumentFieldInfo[] remainingDestinationFields = _destinationFields.Where(x => newMappings.All(m => m.DestinationField.FieldIdentifier != x.FieldIdentifier.ToString())).ToArray();

				return new AutomapBuilder(
					remainingSourceFields,
					remainingDestinationFields,
					Mapping.Concat(newMappings)
					);
			}

			public AutomapBuilder MapByIsIdentifier()
			{
				DocumentFieldInfo sourceIdentifier = _sourceFields.FirstOrDefault(x => x.IsIdentifier);
				DocumentFieldInfo destinationIdentifier = _destinationFields.FirstOrDefault(x => x.IsIdentifier);

				if (sourceIdentifier == null || destinationIdentifier == null)
				{
					return new AutomapBuilder(_sourceFields.ToArray(), _destinationFields.ToArray(), Mapping.ToArray());
				}

				return new AutomapBuilder(_sourceFields.Where(x => x != sourceIdentifier).ToArray(),
					_destinationFields.Where(x => x != destinationIdentifier).ToArray(),
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