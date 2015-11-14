namespace TrackerDog.CollectionHandling
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class SetTrackingHandler : DefaultCollectionTrackingHandler
    {
        public override CollectionChange HandleTracking(CollectionChangeContext changeContext, HashSet<object> addedItems, HashSet<object> removedItems)
        {
            CollectionChange change = base.HandleTracking(changeContext, addedItems, removedItems);

            if (change == CollectionChange.None && changeContext.ParentObjectProperty.IsSet())
            {
                PropertyInfo parentProperty = changeContext.ParentObjectProperty.GetBaseProperty();

                switch (changeContext.CalledMember.Name)
                {
                    case "IntersectWith":
                        return ProcessSetIntersection(changeContext, addedItems, removedItems);

                    case "ExceptWith":
                        return ProcessSetExcept(changeContext, addedItems, removedItems);

                    default:
                        return CollectionChange.None;

                }
            }
            else return change;
        }

        private CollectionChange ProcessSetIntersection(CollectionChangeContext changeContext, HashSet<object> addedItems, HashSet<object> removedItems)
        {
            IDeclaredObjectPropertyChangeTracking tracking = changeContext.Collection.GetPropertyTracking(changeContext.ParentObjectProperty);
            addedItems.IntersectWith(changeContext.ItemsBefore);

            IEnumerable<object> itemsToRemove = ((IEnumerable<object>)tracking.OldValue).Except(changeContext.Collection).ToList();
            removedItems.ExceptWith(addedItems);

            foreach (object item in itemsToRemove)
                removedItems.Add(item);

            return EvalCollectionChange(changeContext);
        }

        private CollectionChange ProcessSetExcept(CollectionChangeContext changeContext, HashSet<object> addedItems, HashSet<object> removedItems)
        {
            IDeclaredObjectPropertyChangeTracking tracking = changeContext.Collection.GetPropertyTracking(changeContext.ParentObjectProperty);
            IEnumerable<object> itemsToRemove = (IEnumerable<object>)changeContext.CallArguments.First();

            addedItems.ExceptWith(itemsToRemove);

            foreach (object item in itemsToRemove.Intersect((IEnumerable<object>)tracking.OldValue))
                addedItems.Add(item);

            return EvalCollectionChange(changeContext);
        }

        private CollectionChange EvalCollectionChange(CollectionChangeContext changeContext)
        {
            int beforeCount = changeContext.ItemsBefore.Count();
            int afterCount = changeContext.Collection.Count();

            if (beforeCount < afterCount) return CollectionChange.Add;
            else if (beforeCount > afterCount) return CollectionChange.Remove;
            else return CollectionChange.None;
        }
    }
}