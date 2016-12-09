using TrackerDog.Configuration;

namespace TrackerDog
{
    internal sealed class CollectionChangeTrackingContext
    {
        /// <summary>
        /// Gets associated change-tracking configuration
        /// </summary>
        public IObjectChangeTrackingConfiguration Configuration { get; set; }
    }
}