namespace TrackerDog.Configuration
{
    using System;
    using System.Diagnostics.Contracts;
    using TrackerDog.CollectionHandling;

    public sealed class CollectionImplementation
    {
        private readonly ICollectionTrackingHandler _trackingHandler;
        private readonly Type _type;

        public CollectionImplementation(Type type, ICollectionTrackingHandler trackingHandler)
        {
            Contract.Requires(type != null, "A collection implementation type is mandatory");
            Contract.Requires(trackingHandler != null, "A collection change tracking handler is mandatory");

            _type = type;
            _trackingHandler = trackingHandler;
        }

        public Type Type => _type;
        public ICollectionTrackingHandler TrackingHandler => _trackingHandler;
    }
}
