namespace TrackerDog.CollectionHandling
{
    /// <summary>
    /// Represents an enumeration of possible collection changes
    /// </summary>
    public enum CollectionChange
    {
        /// <summary>
        /// No change happened
        /// </summary>
        None,

        /// <summary>
        /// One or more items have been added
        /// </summary>
        Add,

        /// <summary>
        /// One or more items have been removed
        /// </summary>
        Remove
    }
}