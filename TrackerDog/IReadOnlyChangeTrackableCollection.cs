namespace TrackerDog
{
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines a collection that can track changes of itself and its items where the state can be read but not written.
    /// </summary>
    [ContractClass(typeof(IReadOnlyChangeTrackableCollectionContract))]
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