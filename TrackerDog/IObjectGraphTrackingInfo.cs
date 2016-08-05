using System.Collections.Immutable;

namespace TrackerDog
{
    /// <summary>
    /// Defines an aggregation hierarchy of a given declared property change tracking
    /// </summary>
    public interface IObjectGraphTrackingInfo
    {
        /// <summary>
        /// Gets the aggregate hierarchy ordered from the deepest to top-most excluding the aggregate root.
        /// </summary>
        IImmutableList<IDeclaredObjectPropertyChangeTracking> AggregateHierarchy { get; }

        /// <summary>
        /// Gets aggregate root of the whole hierarchy
        /// </summary>
        object Parent { get; }
    }
}