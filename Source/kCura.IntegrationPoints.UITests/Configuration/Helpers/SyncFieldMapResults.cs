using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.UITests.Configuration.Models;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
    public class SyncFieldMapResults
    {
        public List<FieldMapModel> FieldMap { get; private set; } = new List<FieldMapModel>();
        public List<FieldMapModel> FieldMapSorted { get; private set; }

        public SyncFieldMapResults(List<FieldObject> sourceWorkspaceFields,
            List<FieldObject> destinationWorkspaceFields)
        {
            //Identifier
            FieldObject sourceIdentifier = sourceWorkspaceFields.FirstOrDefault(x => x.IsIdentifier);
            FieldObject destinationIdentifier = destinationWorkspaceFields.FirstOrDefault(x => x.IsIdentifier);

            if (sourceIdentifier != null && destinationIdentifier != null)
            { 
                FieldMap.Add(new FieldMapModel
                {
                    SourceFieldObject = sourceIdentifier, 
                    DestinationFieldObject = destinationIdentifier, 
                    AutoMapMatchType = TestConstants.FieldMapMatchType.IsIdentifier
                }
                );
                sourceWorkspaceFields = sourceWorkspaceFields.Except(FieldMap.Select(x => x.SourceFieldObject)).ToList();
                destinationWorkspaceFields = destinationWorkspaceFields.Except(FieldMap.Select(x => x.DestinationFieldObject)).ToList();
            }

            //ArtifactID
            foreach (var swf in sourceWorkspaceFields)
            {
                foreach (var dwf in destinationWorkspaceFields.Where(dwf => swf.ArtifactID == dwf.ArtifactID && IsFieldTypeMatch(swf, dwf)))
                {
                    FieldMap.Add(new FieldMapModel{ SourceFieldObject = swf, DestinationFieldObject = dwf, AutoMapMatchType = TestConstants.FieldMapMatchType.ArtifactID});
                }
            }

            sourceWorkspaceFields = sourceWorkspaceFields.Except(FieldMap.Select(x =>x.SourceFieldObject)).ToList();
            destinationWorkspaceFields = destinationWorkspaceFields.Except(FieldMap.Select(x => x.DestinationFieldObject)).ToList();

            //Name
            foreach (var swf in sourceWorkspaceFields)
            {
                foreach (var dwf in destinationWorkspaceFields.Where(dwf => swf.Name == dwf.Name && IsFieldTypeMatch(swf, dwf)))
                {
                    FieldMap.Add(new FieldMapModel { SourceFieldObject = swf, DestinationFieldObject = dwf, AutoMapMatchType = TestConstants.FieldMapMatchType.Name});
                }
            }
            
            FieldMapSorted = FieldMap
                .OrderByDescending(x => x.SourceFieldObject.IsIdentifier)
                .ThenBy(x =>x.SourceFieldObject.DisplayName).ToList();
        }

        private static bool IsFieldTypeMatch(FieldObject sourceField, FieldObject destinationField)
        {
            return (sourceField.Type == destinationField.Type)
                   || (sourceField.Type.StartsWith(TestConstants.FieldTypeNames.FIXED_LENGTH_TEXT) &&
                       (destinationField.Type.StartsWith(TestConstants.FieldTypeNames.FIXED_LENGTH_TEXT) || destinationField.Type == TestConstants.FieldTypeNames.LONG_TEXT)
                   );

        }

        public static List<FieldObject> SortFieldObjects(IEnumerable<FieldObject> fieldObjectsList)
        {
            return fieldObjectsList
                .OrderByDescending(f => f.IsIdentifier)
                .ThenBy(f => f.Name)
                .ThenBy(f => f.Type).ToList();
        }
    }
}

