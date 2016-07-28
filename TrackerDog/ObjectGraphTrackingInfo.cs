using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace TrackerDog
{
    [DebuggerDisplay("{DotSeparatedHierarchy}")]
    internal sealed class ObjectGraphTrackingInfo : IObjectGraphTrackingInfo
    {
        public ObjectGraphTrackingInfo(object parent, IImmutableList<IDeclaredObjectPropertyChangeTracking> aggregateHierarchy)
        {
            Parent = parent;
            AggregateHierarchy = aggregateHierarchy;
        }

        public object Parent { get; }
        public IImmutableList<IDeclaredObjectPropertyChangeTracking> AggregateHierarchy { get; }

        private string DotSeparatedHierarchy => $"{Parent.GetType().Name}.{string.Join(".", AggregateHierarchy.Select(t => t.PropertyName))}";
    }
}
