namespace TrackerDog.CollectionHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    [ContractClassFor(typeof(ICollectionTrackingHandler))]
    public abstract class ICollectionTrackingHandlerContract : ICollectionTrackingHandler
    {
        public CollectionChange HandleTracking(CollectionChangeContext changeContext, HashSet<object> addedItems, HashSet<object> removedItems)
        {
            Contract.Requires(changeContext != null, "Collection change context cannot be null");
            Contract.Requires(addedItems != null, "Added item tracking cannot be null");
            Contract.Requires(addedItems != null, "Removed item tracking cannot be null");

            throw new NotImplementedException();
        }
    }
}