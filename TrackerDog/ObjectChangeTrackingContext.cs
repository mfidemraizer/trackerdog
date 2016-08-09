using System.Collections.Generic;
using System.Reflection;
using TrackerDog.Configuration;

namespace TrackerDog
{
    internal sealed class ObjectChangeTrackingContext
    {
        /// <summary>
        /// Gets or sets tracked object state
        /// </summary>
        public ChangeTrackableObjectState State { get; set; }

        /// <summary>
        /// Gets associated change-tracking configuration
        /// </summary>
        public IObjectChangeTrackingConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the change tracker tracking this object
        /// </summary>
        public ObjectChangeTracker ChangeTracker { get; set; }

        /// <summary>
        /// Gets a set of current object collection property metadata
        /// </summary>
        public ISet<PropertyInfo> CollectionProperties { get; } = new HashSet<PropertyInfo>();
    }
}