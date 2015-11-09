namespace TrackerDog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    [ContractClassFor(typeof(IObjectChangeTracker))]
    public abstract class IObjectChangeTrackerContract : IObjectChangeTracker
    {
        public IImmutableSet<IObjectPropertyChangeTracking> ChangedProperties { get; set; }

        public IImmutableSet<IObjectPropertyChangeTracking> UnchangedProperties { get; set; }

        public IObjectPropertyChangeTracking GetTrackingByProperty<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IObjectPropertyChangeTracking>() != null);

            throw new NotImplementedException();
        }

        [ContractInvariantMethod, SuppressMessage("CC", "CC1036", Justification = "ChangedProperties and UnchangedProperties must be intersected to check that they don't have shared items")]
        private void Invariants()
        {
            Contract.Invariant(ChangedProperties != null);
            Contract.Invariant(UnchangedProperties != null);
            Contract.Invariant(ChangedProperties.Intersect(UnchangedProperties).Count == 0);
        }

        public IEnumerator<IObjectPropertyChangeTracking> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
