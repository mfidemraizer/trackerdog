namespace TrackerDog.Interceptors
{
    using Castle.DynamicProxy;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    internal sealed class CollectionPropertyInterceptor : MethodInterceptor
    {
        protected override void InterceptMethod(IInvocation invocation, IHasParent withParent)
        {
            Type collectionType = invocation.Method.DeclaringType;

            if ((collectionType.IsList() || collectionType.IsSet()) && invocation.Arguments.Length == 1)
            {
                IChangeTrackableCollection trackableCollection = (IChangeTrackableCollection)withParent;
                IChangeTrackableObject changedItem =
                    invocation.Arguments[0] as IChangeTrackableObject
                    ?? invocation.Arguments[0].AsTrackable() as IChangeTrackableObject;
                IEnumerable<object> currentItems = ((IEnumerable<object>)withParent).ToList();

                invocation.Arguments[0] = changedItem ?? invocation.Arguments[0];
                invocation.Proceed();

                switch (invocation.Method.Name)
                {
                    case "Add":
                        trackableCollection.AddedItems.Add(changedItem);
                        trackableCollection.RemovedItems.Remove(changedItem);
                        trackableCollection.RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new[] { changedItem });
                        break;

                    case "Remove":
                        trackableCollection.AddedItems.Remove(changedItem);
                        trackableCollection.RemovedItems.Add(changedItem);
                        trackableCollection.RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new[] { changedItem });
                        break;

                    default:
                        if (collectionType.IsSet())
                            InterceptSetSpecificMethod(invocation, currentItems, trackableCollection);
                        break;
                }
            }
            else
            {
                invocation.Proceed();
            }
        }

        private void InterceptSetSpecificMethod(IInvocation invocation, IEnumerable<object> currentItems, IChangeTrackableCollection trackableCollection)
        {
            IObjectPropertyChangeTracking tracking = trackableCollection.GetChangeTracker()
                                                .ChangedProperties
                                                .Single(t => t.Property == trackableCollection.ParentObjectProperty);

            IEnumerable<object> oldItems = (IEnumerable<object>)tracking.OldValue;

            switch (invocation.Method.Name)
            {
                case "IntersectWith":
                    ProcessSetIntersection(currentItems, oldItems, trackableCollection);
                    break;

                case "ExceptWith":
                    ProcessSetExcept(invocation, currentItems, oldItems, trackableCollection);
                    break;

            }
        }

        private void ProcessSetIntersection(IEnumerable<object> currentItems, IEnumerable<object> oldItems, IChangeTrackableCollection trackableCollection)
        {
            trackableCollection.AddedItems.IntersectWith(currentItems);

            IEnumerable<object> itemsToRemove = currentItems.Except((IEnumerable<object>)trackableCollection).ToList();
            trackableCollection.RemovedItems.ExceptWith(trackableCollection.AddedItems);

            foreach (object item in itemsToRemove)
                trackableCollection.RemovedItems.Add(item);
        }

        private void ProcessSetExcept(IInvocation invocation, IEnumerable<object> currentItems, IEnumerable<object> oldItems, IChangeTrackableCollection trackableCollection)
        {
            IEnumerable<object> itemsToRemove = (IEnumerable<object>)invocation.Arguments[0];
            trackableCollection.AddedItems.ExceptWith(itemsToRemove);

            foreach (object item in itemsToRemove.Intersect(currentItems))
                trackableCollection.RemovedItems.Add(item);
        }
    }
}
