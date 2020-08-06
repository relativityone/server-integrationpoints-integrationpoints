# Sync Images - Fields Map Validation

## Status

Proposed

## Context

We need to add validatior to ValidationNode:

+ If Copy Images has been selected, fields mapping should contain only Control Number

## Decision

Proposed solutions:

1. Add another step to _FieldMappingsValidator_
2. Analyze existing validators and check which of them are also applicable to Images Sync and create another rule chain specific for images

TBD

## Consequences

Decision is between short and long term solution. Anyway in the future we would need to rethink validation, when we would implement Production Sync
