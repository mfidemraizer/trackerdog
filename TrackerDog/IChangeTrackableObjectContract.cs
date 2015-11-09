namespace TrackerDog
{
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

    [ContractClassFor(typeof(IChangeTrackableObject))]
    internal abstract class IChangeTrackableObjectContract : IChangeTrackableObject
    {
        public ObjectChangeTracker ChangeTracker { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName)
        {
            Contract.Requires(trackableObject != null);
            Contract.Requires(!string.IsNullOrEmpty(propertyName));
            Contract.Assert(PropertyChanged != null);
        }

        public void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null)
        {
            Contract.Requires(trackableObject != null);
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(ChangeTracker != null);
        }
    }
}