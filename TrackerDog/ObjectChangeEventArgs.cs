using System;

namespace TrackerDog
{
    public sealed class ObjectChangeEventArgs : EventArgs
    {
        public ObjectChangeEventArgs(object targetObject, IObjectPropertyChangeTracking propertyChangeTracking)
        {
            TargetObject = targetObject;
            PropertyChangeTracking = propertyChangeTracking;
        }

        public object TargetObject { get; }

        public IObjectPropertyChangeTracking PropertyChangeTracking { get; }
    }
}