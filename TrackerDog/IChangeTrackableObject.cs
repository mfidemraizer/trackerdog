namespace TrackerDog
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Reflection;

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
        /// Gets a set of current object collection property metadata
        /// </summary>
        ISet<PropertyInfo> CollectionProperties { get; }

        /// <summary>
        /// Starts tracking this object.
        /// </summary>
        /// <param name="trackableObject">The object to be tracked</param>
        /// <param name="currentTracker">A different change tracker than the associated to this object to track current object</param>
        void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null);
        void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName);
    }
}