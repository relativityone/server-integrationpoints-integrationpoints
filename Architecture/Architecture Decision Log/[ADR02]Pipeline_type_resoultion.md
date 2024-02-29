# Title

## Status

Approved

## Context

After implementing decision from [ADR01]([ADR01]Creating_alternative_versions_of_Sync_pipeline.md), due to specifics of Autofac some changes had to be made.

We cannot read the configuration to make decision which flow to register before building the container, and we cannot register new flow after building the container.

## Decision

All pipelines are registered in `PipelineBuilder` with the type name as a key.

New class `PipelineSelector` was introduced, which encapsulates the decission which pipeline version should be used. It uses `IPipelineSelectorConfiguration` as means to access the data needed to make that decission. All data needed to make any future decisions should be added to tha t interface.

## Consequences

All flows must be registered, but the decision making is still easy to follow with logs and code.