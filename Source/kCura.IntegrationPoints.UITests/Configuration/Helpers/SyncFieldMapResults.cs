using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
    public class SyncFieldMapResults
    {
        internal List<FieldObject> sourceFieldMapping = new List<FieldObject>();
        internal List<FieldObject> destinationFieldMapping = new List<FieldObject>();

        public SyncFieldMapResults(List<FieldObject> sourceWorkspaceFields,
            List<FieldObject> destinationWorkspaceFields)
        {
            foreach (var swf in sourceWorkspaceFields)
            {
                foreach (var dwf in destinationWorkspaceFields.Where(dwf => swf.ArtifactID == dwf.ArtifactID && swf.Type == dwf.Type))
                {
                    sourceFieldMapping.Add(swf);
                    destinationFieldMapping.Add(dwf);
                }
            }

            sourceWorkspaceFields = sourceWorkspaceFields.Except(sourceFieldMapping).ToList();
            destinationWorkspaceFields = destinationWorkspaceFields.Except(destinationFieldMapping).ToList();

            foreach (var swf in sourceWorkspaceFields)
            {
                foreach (var dwf in destinationWorkspaceFields.Where(dwf => swf.Name == dwf.Name && swf.Type == dwf.Type))
                {
                    sourceFieldMapping.Add(swf);
                    destinationFieldMapping.Add(dwf);
                }
            }
            
        }
    }
}

