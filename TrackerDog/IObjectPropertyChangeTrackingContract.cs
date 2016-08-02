using System;
using System.Diagnostics.Contracts;

namespace TrackerDog
{
    [ContractClassFor(typeof(IObjectPropertyChangeTracking))]
    public abstract class IObjectPropertyChangeTrackingContract : IObjectPropertyChangeTracking
    {
        public object CurrentValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool HasChanged
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object OldValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string PropertyName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IObjectChangeTracker Tracker
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object TargetObject
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(Tracker != null, "A property tracking must own a change tracker");
            Contract.Invariant(!string.IsNullOrEmpty(PropertyName), "A property tracking must specify which property tracks");
            Contract.Invariant(CurrentValue == OldValue != HasChanged, "If both current and old value as equal, a tracking cannot expose that property has changed");
            Contract.Invariant(CurrentValue != OldValue == !HasChanged, "If both current and old value are not equal, a tracking cannot expose that property has not changed");
        }

        public bool Equals(IObjectPropertyChangeTracking other)
        {
            throw new NotImplementedException();
        }
    }
}