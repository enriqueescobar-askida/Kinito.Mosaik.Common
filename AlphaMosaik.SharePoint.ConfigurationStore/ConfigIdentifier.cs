namespace AlphaMosaik.SharePoint.ConfigurationStore
{
    /// <summary>
    /// Represents a config item in the config store list.
    /// </summary>
    public class ConfigIdentifier
    {
        /// <summary>
        /// Category of the config item.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Key (name) of the config item.
        /// </summary>
        public string Key { get; set; }

        public ConfigIdentifier(string Category, string Key)
        {
            this.Category = Category;
            this.Key = Key;
        }
    }
}
