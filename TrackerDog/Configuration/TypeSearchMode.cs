namespace TrackerDog.Configuration
{
    /// <summary>
    /// Defines how type search must be performed during configuration stage
    /// </summary>
    public enum TypeSearchMode
    {
        /// <summary>
        /// All types are elegible to be configured
        /// </summary>
        All,

        /// <summary>
        /// Only types with <see cref="ChangeTrackableAttribute"/> are eleigble to be configured
        /// </summary>
        AttributeConfigurationOnly
    }
}
