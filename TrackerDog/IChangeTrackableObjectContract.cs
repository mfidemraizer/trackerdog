using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace TrackerDog
{
    [ContractClassFor(typeof(IChangeTrackableObject))]
    internal abstract class IChangeTrackableObjectContract : IChangeTrackableObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObjectChangeTrackingInfo GetChangeTrackingInfo()
        {
            throw new NotImplementedException();
        }

        public void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName)
        {
            Contract.Requires(trackableObject != null, "A non null reference to a trackable object is mandatory");
            Contract.Requires(!string.IsNullOrEmpty(propertyName), "Changed property must have a name");
            Contract.Assert(PropertyChanged != null, "This event requires at least an event handler to be raised");
        }

        public void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null)
        {
            Contract.Requires(trackableObject != null, "Given reference must be non-null to be able to track it");
        }
    }
}