namespace kCura.IntegrationPoints.Core.Models
{
    /// <summary>
    /// Temp class to support the ability to toggle off Image and Production types for Import Provider
    /// ToDo: Remove once toggle is no longer needed and populate import types in javaScript
    /// </summary>
    public struct ImportType
    {
        public enum ImportTypeValue
        {
            Document = 0,
            Image = 1,
            Production = 2
        }

        public string Name { get; set; }
        public ImportTypeValue? Value { get; set; }

        public ImportType(string name, ImportTypeValue value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
