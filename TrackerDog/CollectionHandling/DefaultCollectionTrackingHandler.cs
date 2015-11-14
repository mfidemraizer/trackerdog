namespace TrackerDog.CollectionHandling
{
    using System.Collections.Generic;
    using System.Linq;

    public class DefaultCollectionTrackingHandler : ICollectionTrackingHandler
    {
        public virtual CollectionChange HandleTracking(CollectionChangeContext changeContext, HashSet<object> addedItems, HashSet<object> removedItems)
        {
            object item = changeContext.CallArguments.First();

            switch (changeContext.CalledMember.Name)
            {
                case "Add":
                    addedItems.Add(changeContext.CallArguments.First());
                    removedItems.Remove(item);
                    return CollectionChange.Add;

                case "Remove":
                    addedItems.Remove(item);
                    removedItems.Add(item);
                    return CollectionChange.Remove;
                default:
                    return CollectionChange.None;
            }
        }
    }
}