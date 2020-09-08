﻿namespace kCura.IntegrationPoints.Data.Queries
{
    public interface IJobResourceTracker
    {
        void CreateTrackingEntry(string tableName, long jobId, int workspaceId);

        int RemoveEntryAndCheckStatus(string tableName, long jobId, int workspaceId);
    }
}