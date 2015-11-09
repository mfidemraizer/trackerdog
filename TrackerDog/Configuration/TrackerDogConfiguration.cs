namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;

    public static class TrackerDogConfiguration
    {
        public static HashSet<Type> TrackableTypes { get; } = new HashSet<Type>();
        public static TrackableCollectionConfiguration CollectionConfiguration { get; } = new TrackableCollectionConfiguration();
    }
}