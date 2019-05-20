﻿using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed class DocumentImportJob
	{
		public IReadOnlyDictionary<string, RelativityDataType> Schema { get; }

		public IList<FieldMap> FieldMappings { get; }

		public Document[] Documents { get; }

		private DocumentImportJob(Dictionary<string, RelativityDataType> schema, IList<FieldMap> fieldMappings, Document[] documents)
		{
			Schema = schema;
			FieldMappings = fieldMappings;
			Documents = documents;
		}

		//public IFieldManager CreateFieldManager()
		//{
		//	IDocumentFieldRepository documentFieldRepository = new LiteralDocumentFieldRepository(Schema);
		//	ConfigurationStub configuration = new ConfigurationStub { FieldMappings = FieldMappings };
		//	IFieldManager fieldManager = new FieldManager(configuration, documentFieldRepository, Enumerable.Empty<ISpecialFieldBuilder>());
		//	return fieldManager;
		//}

		public static DocumentImportJob Create(Dictionary<string, RelativityDataType> schema, IList<FieldMap> fieldMaps, Document[] documents)
		{
			HashSet<string> schemaFields = new HashSet<string>(schema.Keys);

			List<Document> invalidDocuments = documents.Where(d => !d.FieldSet.IsSubsetOf(schemaFields)).ToList();
			if (invalidDocuments.Any())
			{
				throw new ArgumentException(
					$"Documents do not conform to the given schema: {string.Join("; ", invalidDocuments.Select(x => x.ArtifactId))}",
					nameof(documents));
			}

			List<FieldMap> invalidFieldMaps = fieldMaps.Where(f => !schemaFields.Contains(f.SourceField.DisplayName)).ToList();
			if (invalidFieldMaps.Any())
			{
				throw new ArgumentException(
					$"Field maps do not conform to the given schema; the following fields are missing from the schema: {string.Join("; ", invalidFieldMaps)}");
			}

			return new DocumentImportJob(schema, fieldMaps, documents);
		}
	}
}
