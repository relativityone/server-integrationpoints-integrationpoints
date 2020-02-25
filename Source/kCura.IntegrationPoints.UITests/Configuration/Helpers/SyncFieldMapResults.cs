using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
    public class SyncFieldMapResults
    {
        internal List<FieldObject> sourceFieldMapping = new List<FieldObject>();
        internal List<FieldObject> destinationFieldMapping = new List<FieldObject>();
        public List<FieldMapModel> fieldMap = new List<FieldMapModel>();
        public List<FieldMapModel> fieldMapSorted = new List<FieldMapModel>();
        public SyncFieldMapResults(List<FieldObject> sourceWorkspaceFields,
            List<FieldObject> destinationWorkspaceFields)
        {
            foreach (var swf in sourceWorkspaceFields)
            {
                foreach (var dwf in destinationWorkspaceFields.Where(dwf => swf.ArtifactID == dwf.ArtifactID && swf.Type == dwf.Type))
                {
                    fieldMap.Add(new FieldMapModel{ SourceFieldObject = swf, DestinationFieldObject = dwf});
                }
            }

            sourceWorkspaceFields = sourceWorkspaceFields.Except(fieldMap.Select(x =>x.SourceFieldObject)).ToList();
            destinationWorkspaceFields = destinationWorkspaceFields.Except(fieldMap.Select(x => x.DestinationFieldObject)).ToList();

            foreach (var swf in sourceWorkspaceFields)
            {
                foreach (var dwf in destinationWorkspaceFields.Where(dwf => swf.Name == dwf.Name && swf.Type == dwf.Type))
                {
                    fieldMap.Add(new FieldMapModel { SourceFieldObject = swf, DestinationFieldObject = dwf });
                }
            }
            
            fieldMapSorted = fieldMap
                .OrderByDescending(x => x.SourceFieldObject.IsIdentifier)
                .ThenBy(x =>x.SourceFieldObject.DisplayName).ToList();
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

