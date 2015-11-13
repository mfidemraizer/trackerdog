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

        public IDeclaredObjectPropertyChangeTracking GetTrackingByProperty<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            Contract.Requires(propertySelector != null, "Property selector is mandatory to get a property tracking");
            Contract.Ensures(Contract.Result<IDeclaredObjectPropertyChangeTracking>() != null);

            throw new NotImplementedException();
        }

        [ContractInvariantMethod, SuppressMessage("CC", "CC1036", Justification = "ChangedProperties and UnchangedProperties must be intersected to check that they don't have shared items")]
        private void Invariants()
        {
            Contract.Invariant(ChangedProperties != null, "A change tracker must expose its changed properties with a non-null reference to a collection");
            Contract.Invariant(UnchangedProperties != null, "A change tracker must expose its unchanged properties with a non-null reference to a collection");
            Contract.Invariant(ChangedProperties.Intersect(UnchangedProperties).Count == 0, "Changed and unchanged property collection cannot match, because it would mean that one or many properties would be changed and unchanged");
        }

        public IEnumerator<IObjectPropertyChangeTracking> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IObjectPropertyChangeTracking GetDynamicTrackingByProperty(string propertyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(propertyName), "Property name is mandatory to get a property tracking");
            Contract.Ensures(Contract.Result<IObjectPropertyChangeTracking>() != null);

            throw new NotImplementedException();
        }
    }
}