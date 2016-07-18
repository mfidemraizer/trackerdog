using System;

namespace TrackerDog
{
    public sealed class ObjectChangeEventArgs : EventArgs
    {
        public ObjectChangeEventArgs(IObjectPropertyChangeTracking propertyChangeTracking)
        {
            PropertyChangeTracking = propertyChangeTracking;
        }

        public IObjectPropertyChangeTracking PropertyChangeTracking { get; }
    }
}