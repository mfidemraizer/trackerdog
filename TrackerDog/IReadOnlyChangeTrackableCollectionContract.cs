namespace TrackerDog
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;

    [ContractClassFor(typeof(IReadOnlyChangeTrackableCollection))]
    public abstract class IReadOnlyChangeTrackableCollectionContract : IReadOnlyChangeTrackableCollection
    {
        public IImmutableSet<object> AddedItems
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IImmutableSet<object> RemovedItems
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(AddedItems != null, "Added items cannot be a null reference");
            Contract.Invariant(RemovedItems != null, "Removed items cannot be a null reference");
            Contract.Invariant(AddedItems.Intersect(RemovedItems).Count == 0, "Added and removed item collection cannot match because it would mean that some items have been added and removed");
        }
    }
}