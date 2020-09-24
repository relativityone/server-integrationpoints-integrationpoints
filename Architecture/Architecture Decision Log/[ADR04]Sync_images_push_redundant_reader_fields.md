# Sync Images -  Batch Data Reader

## Status

Approved

## Context

In Old RIP code DataReader is built with fields:

+ ControlNumber
+ NATIVE_FILE_PATH_001
+ REL_FILE_NAME_001
+ NATIVE_FILE_SIZE_001
+ REL_TYPE_NAME_001
+ REL_TYPE_SUPPORTED_BY_VIEWER_001

Couple of them were empty during testing:

+ NATIVE_FILE_SIZE_001
+ REL_TYPE_NAME_001
+ REL_TYPE_SUPPORTED_BY_VIEWER_001

We should determine if they are needed and if not we could remove them from new BatchDataReader implemented in Sync

Code:

<https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Core/Services/Exporter/Images/ImageTransferDataReader.cs>
<https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Synchronizers.RDO/ImportAPI/RelativityReaderDecorator.cs>

## Decision

The only required special fields for images pushing are:

+ ImageFileName
+ ImageFileLocation

## Consequences

+ Easier implementation and code maintanance
+ Less data send to IAPI
+ Better performance
