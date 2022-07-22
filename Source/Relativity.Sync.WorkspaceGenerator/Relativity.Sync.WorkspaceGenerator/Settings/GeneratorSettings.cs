using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Relativity.Sync.WorkspaceGenerator.Settings
{
    public class GeneratorSettings
    {
        private const string DEFAULT_RELATIVITY_URI = "https://host.name/Relativity";
        private const string DEFAULT_RELATIVITY_SERVICES_URI = "https://host.name/Relativity.Services";
        private const string DEFAULT_RELATIVITY_USER_NAME = "relativity.admin@kcura.com";
        private const string DEFAULT_RELATIVITY_PASSWORD = "Test1234!";
        private const string DEFAULT_DESIRED_WORKSPACE_NAME = "My Test Workspace";
        private const string DEFAULT_TEMPLATE_WORKSPACE_NAME = "Functional Tests Template";
        private const string DEFAULT_TEST_DATA_DIRECTORY_PATH = @"C:\Data\WorkspaceGenerator";
        private const int DEFAULT_BATCH_SIZE = 10000;

        private const string DEFAULT_TEST_CASE_NAME = "TC1";
        private const int DEFAULT_TEST_CASE_NUMBER_OF_DOCUMENTS = 10;
        private const int DEFAULT_TEST_CASE_NUMBER_OF_FIELDS = 15;
        private const int DEFAULT_TEST_CASE_TOTAL_EXTRACTED_TEXT_SIZE_IN_MB = 5;
        private const int DEFAULT_TEST_CASE_TOTAL_NATIVES_SIZE_IN_MB = 8;

        private Uri _relativityUri;
        // Relativity URL e.g. https://host.name/Relativity
        public Uri RelativityUri
        {
            get => _relativityUri;
            set
            {
                _relativityUri = value;
                RelativityWebApiUri = new Uri(value, "/RelativityWebAPI");
                RelativityRestApiUri = new Uri(value, "/Relativity.Rest/api");
            }
        }

        // Relativity Services URL e.g. https://host.name/Relativity/Relativity.Services
        public Uri RelativityServicesUri { get; set; }

        // Relativity user name
        public string RelativityUserName { get; set; }

        // Relativity user password
        public string RelativityPassword { get; set; }

        // Name of the template workspace
        public string TemplateWorkspaceName { get; set; }

        // Name of the workspace to be created
        public string DesiredWorkspaceName { get; set; }

        // Directory path where test data (natives and extracted text) will be stored
        public string TestDataDirectoryPath { get; set; }

        // Size of batch for documents import
        public int BatchSize { get; set; }

        // Should Extracted Text be store in DataGrid (if False, it will be in SQL)
        public bool EnabledDataGridForExtractedText { get; set; }

        public List<TestCase> TestCases { get; set; } = new List<TestCase>();

        [JsonIgnore]
        public Uri RelativityWebApiUri { get; private set; }

        [JsonIgnore]
        public Uri RelativityRestApiUri { get; private set; }

        [JsonIgnore] public bool Append { get; private set; } = false;

        public GeneratorSettings SetDefaultSettings()
        {
            RelativityUri = new Uri(DEFAULT_RELATIVITY_URI);
            RelativityServicesUri = new Uri(DEFAULT_RELATIVITY_SERVICES_URI);
            RelativityUserName = DEFAULT_RELATIVITY_USER_NAME;
            RelativityPassword = DEFAULT_RELATIVITY_PASSWORD;
            DesiredWorkspaceName = DEFAULT_DESIRED_WORKSPACE_NAME;
            TemplateWorkspaceName = DEFAULT_TEMPLATE_WORKSPACE_NAME;
            TestDataDirectoryPath = DEFAULT_TEST_DATA_DIRECTORY_PATH;
            BatchSize = DEFAULT_BATCH_SIZE;

            TestCases = new List<TestCase>
            {
                new TestCase
                {
                    Name = DEFAULT_TEST_CASE_NAME,
                    NumberOfDocuments = DEFAULT_TEST_CASE_NUMBER_OF_DOCUMENTS,
                    NumberOfFields = DEFAULT_TEST_CASE_NUMBER_OF_FIELDS,
                    TotalExtractedTextSizeInMB = DEFAULT_TEST_CASE_TOTAL_EXTRACTED_TEXT_SIZE_IN_MB,
                    TotalNativesSizeInMB = DEFAULT_TEST_CASE_TOTAL_NATIVES_SIZE_IN_MB
                }
            };

            return this;
        }

        public void ToJsonFile(string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static GeneratorSettings FromJsonFile(string filePath, bool append = false)
        {
            GeneratorSettings settings = JsonConvert.DeserializeObject<GeneratorSettings>(File.ReadAllText(filePath));

            settings.Append = append;
            if (settings.BatchSize == 0)
            {
                settings.BatchSize = DEFAULT_BATCH_SIZE;
            }

            return settings;
        }
    }
}