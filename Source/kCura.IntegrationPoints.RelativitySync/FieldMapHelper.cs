using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.RelativitySync.Utils;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

using SyncFieldEntry = Relativity.Sync.Storage.FieldEntry;
using SyncFieldMap = Relativity.Sync.Storage.FieldMap;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal static class FieldMapHelper
    {
        public static List<SyncFieldMap> FixedSyncMapping(List<FieldMap> fieldsMapping, IAPILog logger)
        {
            List<FieldMap> fields = FixedMapping(fieldsMapping, logger);

            return fields.Select(x => new SyncFieldMap
            {
                SourceField = new SyncFieldEntry()
                {
                    DisplayName = x.SourceField.DisplayName,
                    FieldIdentifier = int.Parse(x.SourceField.FieldIdentifier),
                    IsIdentifier = x.SourceField.IsIdentifier
                },
                DestinationField = new SyncFieldEntry()
                {
                    DisplayName = x.DestinationField.DisplayName,
                    FieldIdentifier = int.Parse(x.DestinationField.FieldIdentifier),
                    IsIdentifier = x.DestinationField.IsIdentifier
                },
                FieldMapType = x.FieldMapType.ToSyncFieldMapType(),

            }).ToList();
        }

        private static List<FieldMap> FixedMapping(List<FieldMap> fieldsMapping, IAPILog logger)
        {
            if (fieldsMapping == null)
            {
                return new List<FieldMap>();
            }

            fieldsMapping = FixFolderPathMapping(fieldsMapping);
            fieldsMapping = RemoveSpecialFieldMappings(fieldsMapping);
            fieldsMapping = FixControlNumberFieldName(fieldsMapping);
            fieldsMapping = Deduplicate(fieldsMapping, logger);

            return fieldsMapping;
        }

        private static List<FieldMap> FixFolderPathMapping(List<FieldMap> fields)
        {
            FieldMap mappedFolderPathSpecialField = fields.SingleOrDefault(x => x.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
            if (mappedFolderPathSpecialField?.DestinationField?.ActualName != null)
            {
                fields.Add(new FieldMap()
                {
                    SourceField = mappedFolderPathSpecialField.SourceField,
                    DestinationField = mappedFolderPathSpecialField.DestinationField,
                    FieldMapType = FieldMapTypeEnum.None
                });
            }

            return fields;
        }

        private static List<FieldMap> RemoveSpecialFieldMappings(List<FieldMap> fields)
        {
            var redundantMapTypes = new[] { FieldMapTypeEnum.NativeFilePath, FieldMapTypeEnum.FolderPathInformation };
            return fields.Where(f => !redundantMapTypes.Contains(f.FieldMapType)).ToList();
        }

        private static List<FieldMap> FixControlNumberFieldName(List<FieldMap> fields)
        {
            FieldMap identifier = fields.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.Identifier);
            if (identifier != null)
            {
                FixControlNumberField(identifier.SourceField);
                FixControlNumberField(identifier.DestinationField);
            }

            return fields;
        }

        private static List<FieldMap> Deduplicate(List<FieldMap> fields, IAPILog logger)
        {
            List<FieldMap> deduplicatedList = new List<FieldMap>();

            foreach (FieldMap fieldMap in fields)
            {
                if (deduplicatedList.Any(x => x.SourceField.FieldIdentifier == fieldMap.SourceField.FieldIdentifier ||
                                              x.DestinationField.FieldIdentifier == fieldMap.DestinationField.FieldIdentifier))
                {
                    logger.LogWarning("Field mapping contains duplicated field. Source field artifact ID: {sourceID}" +
                                      " Destination field artifact ID: {destinationID}",
                        fieldMap.SourceField.FieldIdentifier, fieldMap.DestinationField.FieldIdentifier);

                    continue;
                }

                deduplicatedList.Add(fieldMap);
            }

            return deduplicatedList;
        }

        private static void FixControlNumberField(FieldEntry fieldEntry)
        {
            fieldEntry.DisplayName = fieldEntry.DisplayName.Replace("[Object Identifier]", string.Empty).Trim();
        }
    }
}
