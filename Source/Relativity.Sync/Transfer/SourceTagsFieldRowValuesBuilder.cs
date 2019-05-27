using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceTagsFieldRowValuesBuilder : ISpecialFieldRowValuesBuilder
	{
		private readonly ISynchronizationConfiguration _configuration;

		public SourceTagsFieldRowValuesBuilder(ISynchronizationConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[] {SpecialFieldType.SourceJob, SpecialFieldType.SourceWorkspace};

		public object BuildRowValue(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, object initialValue)
		{
			switch (fieldInfoDto.SpecialFieldType)
			{
				case SpecialFieldType.SourceJob:
					return _configuration.SourceJobTagName;
				case SpecialFieldType.SourceWorkspace:
					return _configuration.SourceWorkspaceTagName;
				default:
					throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfoDto.SpecialFieldType.ToString()}.", nameof(fieldInfoDto));
			}
		}
	}
}