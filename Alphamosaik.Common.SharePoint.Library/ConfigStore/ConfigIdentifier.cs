namespace Alphamosaik.Common.SharePoint.Library.ConfigStore
{
    /// <summary>
    /// Represents a config item in the config store list.
    /// </summary>
    public class ConfigIdentifier
    {
        /// <summary>
        /// Gets or Sets Category of the config item.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or Sets Key (name) of the config item.
        /// </summary>
        public string Key { get; set; }

        public ConfigIdentifier(string category, string key)
        {
            Category = category;
            Key = key;
        }
    }
}
