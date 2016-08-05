using System;

namespace TrackerDog
{
    /// <summary>
    /// Represents the arguments for <see cref="IObjectChangeTracker.Changed"/> event for a declared property change.
    /// </summary>
    public sealed class DeclaredObjectPropertyChangeEventArgs : ObjectChangeEventArgs
    {
        private readonly Lazy<IObjectGraphTrackingInfo> _graphTrackingInfo;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="targetObject">The object that changed</param>
        /// <param name="propertyChangeTracking">The property change tracking of the declared property that changed on the target object</param>
        public DeclaredObjectPropertyChangeEventArgs(object targetObject, IDeclaredObjectPropertyChangeTracking propertyChangeTracking)
            : base(targetObject, propertyChangeTracking)
        {
            _graphTrackingInfo = new Lazy<IObjectGraphTrackingInfo>
            (
                () => propertyChangeTracking.Tracker.GetTrackingGraphFromProperty(propertyChangeTracking.Property)
            );
        }
        /// <summary>
        /// Gets an instance of the object graph information to introspect graph's details
        /// </summary>
        public IObjectGraphTrackingInfo GraphTrackingInfo => _graphTrackingInfo.Value;
    }
}