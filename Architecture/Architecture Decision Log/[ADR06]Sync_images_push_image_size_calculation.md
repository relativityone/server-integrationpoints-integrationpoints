# Sync Images - DataSourceSnapshotExecutor and Images size calculation

## Status

Approved

## Context

In our DataSourceSnapshot step, we currently have 2 versions executors:

+ DataSourceSnapshotExecutor
+ RetryDataSourceSnapshotNode

For images, we can only reuse 2 or 3 lines of those classes (different query, different fields, different requested size calculation, different logging messages).

## Decision

Since the process of creating the snapshot is different for images and natives, completely seperate classes should be created:

+ DocumentDataSourceSnapshotExecutor
+ DocumentRetryDataSourceSnapshotNode
+ ImageDataSourceSnapshotExecutor
+ ImageRetryDataSourceSnapshotNode

No shared base class should be creaetd, since the amount of common code is very little and it makes it easier for Autofac to resolve the node.

*JobStatisticsContainer* should have new field *ImagesBytesRequested* to hold the task calculating size. The calculation should be done using new class *ImageFileRepository*.

## Consequences

+ (+) Clear structure of exeuctors and nodes
+ (-) 2 new classes very similar to each other
    + it is still better to have 4 almost identical classes than 3 level hierarhy adding single line at each level