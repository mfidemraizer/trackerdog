namespace TrackerDog.CollectionHandling
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines how a certain collection should be tracked for changes
    /// </summary>
    [ContractClass(typeof(ICollectionTrackingHandlerContract))]
    public interface ICollectionTrackingHandler
    {
        /// <summary>
        /// Handles how changes are tracked
        /// </summary>
        /// <param name="changeContext">The change context</param>
        /// <param name="addedItems">Added item tracking</param>
        /// <param name="removedItems">Removed item tracking</param>
        /// <returns>What collection change happened</returns>
        CollectionChange HandleTracking(CollectionChangeContext changeContext, HashSet<object> addedItems, HashSet<object> removedItems);
    }
}