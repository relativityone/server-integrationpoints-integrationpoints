﻿using System;

namespace kCura.IntegrationPoints.Agent.CustomProvider.DTO
{
    public class CustomProviderBatch
    {
        public Guid BatchGuid { get; set; } = Guid.NewGuid();

        public int BatchID { get; set; }

        public string IDsFilePath { get; set; }

        public string DataFilePath { get; set; }
        
        public bool IsAddedToImportQueue { get; set; }
    }
}
