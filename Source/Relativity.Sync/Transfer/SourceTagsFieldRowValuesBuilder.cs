﻿using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceTagsFieldRowValuesBuilder : ISpecialFieldRowValuesBuilder
	{
		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[] {SpecialFieldType.SourceJob, SpecialFieldType.SourceWorkspace};

		public object BuildRowValue(FieldInfo fieldInfo, RelativityObjectSlim document, object initialValue)
		{
			switch (fieldInfo.SpecialFieldType)
			{
				case SpecialFieldType.SourceJob:
				case SpecialFieldType.SourceWorkspace:
					return initialValue;
				default:
					throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfo.SpecialFieldType.ToString()}.", nameof(fieldInfo));
			}
		}
	}
}