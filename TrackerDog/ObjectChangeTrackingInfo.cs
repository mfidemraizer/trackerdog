using System.Collections.Generic;
using System.Reflection;

namespace TrackerDog
{
    internal sealed class ObjectChangeTrackingInfo
    {
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