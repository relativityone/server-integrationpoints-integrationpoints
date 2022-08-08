namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IInstanceSettingRepository
    {
        /// <summary>
        /// Gets the value of an instance setting.
        /// </summary>
        /// <param name="section">The section the instance setting belongs to.</param>
        /// <param name="name">The name of the instance setting.</param>
        /// <returns>The value of the instance setting, as a string. Null otherwise.</returns>
        string GetConfigurationValue(string section, string name);
    }
}
