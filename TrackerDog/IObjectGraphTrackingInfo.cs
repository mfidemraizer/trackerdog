using System.Collections.Immutable;

namespace TrackerDog
{
    public interface IObjectGraphTrackingInfo
    {
        IImmutableList<IDeclaredObjectPropertyChangeTracking> AggregateHierarchy { get; }
        object Parent { get; }
    }
}