namespace TrackerDog
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines a collection that can track changes of itself and its items.
    /// </summary>
    [ContractClass(typeof(IChangeTrackableCollectionContract))]
    internal interface IChangeTrackableCollection : INotifyCollectionChanged, IHasParent
    {
        /// <summary>
        /// Gets added items
        /// </summary>
        HashSet<object> AddedItems { get; }

        /// <summary>
        /// Gets removed items
        /// </summary>
        HashSet<object> RemovedItems { get; }

        /// <summary>
        /// Raises the collection changed event to let a change tracker be aware that the collection has changed.
        /// </summary>
        /// <param name="action">The action that changed the collection</param>
        /// <param name="changedItems">Which items have changed</param>
        void RaiseCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<object> changedItems);
    }
}