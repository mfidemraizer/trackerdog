using System.Collections.Immutable;

namespace TrackerDog
{
    /// <summary>
    /// Defines a collection that can track changes of itself and its items where the state can be read but not written.
    /// </summary>
    public interface IReadOnlyChangeTrackableCollection
    {
        /// <summary>
        /// Gets added items
        /// </summary>
        IImmutableSet<object> AddedItems { get; }

        /// <summary>
        /// Gets removed items
        /// </summary>
        IImmutableSet<object> RemovedItems { get; }
    }
}