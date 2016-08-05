using System;

namespace TrackerDog
{
    /// <summary>
    /// Represents the arguments for <see cref="IObjectChangeTracker.Changed"/> event.
    /// </summary>
    public class ObjectChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="targetObject">The object that changed</param>
        /// <param name="propertyChangeTracking">The property change tracking of the property that changed on the target object</param>
        public ObjectChangeEventArgs(object targetObject, IObjectPropertyChangeTracking propertyChangeTracking)
        {
            TargetObject = targetObject;
            PropertyChangeTracking = propertyChangeTracking;
        }

        /// <summary>
        /// Gets the target object that has changed
        /// </summary>
        public object TargetObject { get; }

        /// <summary>
        /// Gets the property change tracking of the property that changed on the target object
        /// </summary>
        public IObjectPropertyChangeTracking PropertyChangeTracking { get; }
    }
}