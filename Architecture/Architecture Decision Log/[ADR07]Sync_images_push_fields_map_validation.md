# Sync Images - Fields Map Validation

## Status

Approved

## Context

We need to add validatior to ValidationNode:

+ If Copy Images has been selected, fields mapping should contain only Control Number

## Decision

The interface *IValidator* should be rafactor so that every implemenatation can make a decision based on *IValidationConfiguration* whether it should run the validation for given job. 

For *FieldMapValidator*, there should be two seperate implementations: *ImageFieldMapValidator* and *DocumentFieldMapValidator*, each meeting requirements for covered flow.

## Consequences

Decision is between short and long term solution. Anyway in the future we would need to rethink validation, when we would implement Production Sync
