namespace TrackerDog
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    [ContractClassFor(typeof(IChangeTrackableCollection))]
    internal abstract class IChangeTrackableCollectionContract : IChangeTrackableCollection
    {
        public HashSet<object> AddedItems { get; set; }

        public IChangeTrackableObject ParentObject { get; set; }

        public PropertyInfo ParentObjectProperty { get; set; }

        public HashSet<object> RemovedItems { get; set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void RaiseCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<object> changedItems)
        {
            Contract.Requires(changedItems != null && changedItems.Count() > 0);
            Contract.Assert(CollectionChanged != null);
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(AddedItems != null);
            Contract.Invariant(RemovedItems != null);
            Contract.Invariant(AddedItems.Intersect(RemovedItems).Count() == 0);
        }
    }
}