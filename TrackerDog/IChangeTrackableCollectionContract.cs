using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace TrackerDog
{
    [ContractClassFor(typeof(IChangeTrackableCollection))]
    internal abstract class IChangeTrackableCollectionContract : IChangeTrackableCollection
    {
        public HashSet<object> AddedItems { get; set; }

        public IChangeTrackableObject ParentObject { get; set; }

        public PropertyInfo ParentObjectProperty { get; set; }

        public HashSet<object> RemovedItems { get; set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        
        public CollectionChangeTrackingContext GetChangeTrackingContext()
        {
            Contract.Ensures(Contract.Result<CollectionChangeTrackingContext>() != null);

            throw new NotImplementedException();
        }

        public void RaiseCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<object> changedItems)
        {
            Contract.Requires(changedItems != null && changedItems.Count() > 0, "A collection change must change some item");
            Contract.Assert(CollectionChanged != null, "This event cannot be raised with no event handler in the broadcast list");
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(AddedItems != null, "This property cannot be null");
            Contract.Invariant(RemovedItems != null, "This property cannot be null");
            Contract.Invariant(AddedItems.Intersect(RemovedItems).Count() == 0, "This property cannot be null");
        }
    }
}