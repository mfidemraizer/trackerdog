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
            Contract.Invariant(AddedItems != null);
            Contract.Invariant(RemovedItems != null);
            Contract.Invariant(AddedItems.Intersect(RemovedItems).Count == 0);
        }
    }
}