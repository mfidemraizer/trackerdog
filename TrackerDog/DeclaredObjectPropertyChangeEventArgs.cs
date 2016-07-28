using System;

namespace TrackerDog
{
    public sealed class DeclaredObjectPropertyChangeEventArgs : ObjectChangeEventArgs
    {
        private readonly Lazy<IObjectGraphTrackingInfo> _graphTrackingInfo;

        public DeclaredObjectPropertyChangeEventArgs(object targetObject, IDeclaredObjectPropertyChangeTracking propertyChangeTracking)
            : base(targetObject, propertyChangeTracking)
        {
            _graphTrackingInfo = new Lazy<IObjectGraphTrackingInfo>
            (
                () => propertyChangeTracking.Tracker.GetTrackingGraphFromProperty(propertyChangeTracking.Property)
            );
        }

        public IObjectGraphTrackingInfo GraphTrackingInfo => _graphTrackingInfo.Value;
    }
}