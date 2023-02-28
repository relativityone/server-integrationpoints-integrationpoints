using System;
using System.IO;
using Microsoft.Win32;

namespace kCura.IntegrationPoints.Domain
{
    public class RelativityFeaturePathService
    {
        private const string FEATURE_PATH_REGISTRY_KEY__BaseInstallDir = "BaseInstallDir";
        private const string FEATURE_PATH_REGISTRY_KEY__WebProcessingPath = "WebProcessingPath";
        private const string FEATURE_PATH_REGISTRY_KEY__WebPath = "WebPath";
        private const string FEATURE_PATH_REGISTRY_KEY__AgentPath = "AgentPath";
        private const string FEATURE_PATH_REGISTRY_KEY__LibraryPath = "LibraryPath";

        public RelativityFeaturePathService()
        { }

        protected string _baseInstallDir = null;
        private bool _newRegistryStructure = false;

        public bool NewRegistryStructure
        {
            get
            {
                if (_baseInstallDir == null)
                {
                    _newRegistryStructure = false;
                    _baseInstallDir = string.Empty;
                    try
                    {
                        _baseInstallDir = GetRelativityFeaturePathsRegistryValue(FEATURE_PATH_REGISTRY_KEY__BaseInstallDir);
                    }
                    catch
                    {
                    }
                    _newRegistryStructure = !string.IsNullOrWhiteSpace(_baseInstallDir);
                }
                return _newRegistryStructure;
            }
            set { _newRegistryStructure = value; }
        }

        private string _webProcessingPath;

        public string WebProcessingPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_webProcessingPath))
                {
                    _webProcessingPath = string.Empty;
                    try
                    {
                        _webProcessingPath = GetFeaturePathsValue(FEATURE_PATH_REGISTRY_KEY__WebProcessingPath);
                    }
                    catch
                    {
                    }
                }
                return _webProcessingPath;
            }
            set { _webProcessingPath = value; }
        }

        private string _eddsPath;

        public string EddsPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_eddsPath))
                {
                    _eddsPath = string.Empty;
                    try
                    {
                        _eddsPath = GetFeaturePathsValue(FEATURE_PATH_REGISTRY_KEY__WebPath);
                    }
                    catch
                    {
                    }
                }
                return _eddsPath;
            }
            set { _eddsPath = value; }
        }

        private string _agentPath;

        public string AgentPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_agentPath))
                {
                    _agentPath = string.Empty;
                    try
                    {
                        _agentPath = GetFeaturePathsValue(FEATURE_PATH_REGISTRY_KEY__AgentPath);
                    }
                    catch
                    {
                    }
                }
                return _agentPath;
            }
            set { _agentPath = value; }
        }

        private string _libraryPath;

        public string LibraryPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_libraryPath))
                {
                    _libraryPath = string.Empty;
                    try
                    {
                        _libraryPath = GetFeaturePathsValue(FEATURE_PATH_REGISTRY_KEY__LibraryPath);
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrWhiteSpace(_libraryPath))
                {
                    _libraryPath = GetDevEnvironmentLibPath(); // HACK: copied from Relativity Core
                    if (!Directory.Exists(_libraryPath))
                    {
                        throw new Exception("Could not retrieve LibraryPath.");
                    }
                }
                return _libraryPath;
            }
            set { _libraryPath = value; }
        }

        protected virtual RegistryKey GetRelativityFeaturePathsRegistryKey()
        {
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey relativityKey = rk.OpenSubKey("SOFTWARE\\kCura\\Relativity");
            return relativityKey.OpenSubKey("FeaturePaths");
        }

        protected virtual string GetRelativityFeaturePathsRegistryValue(string keyName)
        {
            RegistryKey rk = GetRelativityFeaturePathsRegistryKey();
            object rkval = rk.GetValue(keyName);
            string keyValue = string.Empty;
            if (rkval != null)
            {
                keyValue = rkval.ToString();
            }
            rk.Close();
            return keyValue;
        }

        protected virtual string GetFeaturePathsValue(string keyName)
        {
            string path = string.Empty;
            if (NewRegistryStructure)
            {
                string subDirectory = string.Empty;
                switch (keyName)
                {
                    case FEATURE_PATH_REGISTRY_KEY__AgentPath:
                        subDirectory = "Agents";
                        break;
                    case FEATURE_PATH_REGISTRY_KEY__LibraryPath:
                        subDirectory = "Library";
                        break;
                    case FEATURE_PATH_REGISTRY_KEY__WebPath:
                        subDirectory = "EDDS";
                        break;
                    case FEATURE_PATH_REGISTRY_KEY__WebProcessingPath:
                        subDirectory = "WebProcessing";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(subDirectory))
                {
                    path = Path.Combine(_baseInstallDir, subDirectory);
                }
            }
            else
            {
                path = GetRelativityFeaturePathsRegistryValue(keyName);
            }

            if (!DirectoryExists(path))
            {
                path = string.Empty;
            }
            return path;
        }

        protected virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        protected virtual string GetDevEnvironmentLibPath()
        {
            return Environment.GetEnvironmentVariable("TRUNK")+ @"\lib";
        }
    }
}
