﻿using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.Transfer
{
	internal interface IExportFieldSanitizer
	{
		RelativityDataType SupportedType { get; }

		Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue);
	}
}