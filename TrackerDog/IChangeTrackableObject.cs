namespace TrackerDog
{
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines an object that allows its changes to be tracked.
    /// </summary>
    [ContractClass(typeof(IChangeTrackableObjectContract))]
    internal interface IChangeTrackableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the change tracker tracking this object
        /// </summary>
        ObjectChangeTracker ChangeTracker { get; }

        /// <summary>
        /// Starts tracking this object.
        /// </summary>
        /// <param name="trackableObject">The object to be tracked</param>
        /// <param name="currentTracker">A different change tracker than the associated to this object to track current object</param>
        void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null);
        void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName);
    }
}