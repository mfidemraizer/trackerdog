using TrackerDog.Configuration;

namespace TrackerDog
{
    public static class ObjectChangeTracking
    {
        public static IObjectChangeTrackingConfiguration CreateConfiguration() => new ObjectChangeTrackingConfiguration(new TrackableCollectionConfiguration());
    }
}
