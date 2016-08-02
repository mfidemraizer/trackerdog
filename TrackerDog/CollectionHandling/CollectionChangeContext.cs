using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace TrackerDog.CollectionHandling
{
    /// <summary>
    /// Represents a set of available information to collection change handling participants like 
    /// collection change interceptors.
    /// </summary>
    public sealed class CollectionChangeContext
    {
        private readonly IEnumerable<object> _collection;
        private readonly IEnumerable<object> _itemsBefore;
        private PropertyInfo _parentObjectProperty;
        private readonly ISet<object> _addedItems;
        private readonly ISet<object> _removedItems;

        public CollectionChangeContext(IEnumerable<object> collection, IEnumerable<object> itemsBefore, PropertyInfo parentObjectProperty, ISet<object> addedItems, ISet<object> removedItems)
        {
            Contract.Requires(collection != null, "Given source collection must not be null");
            Contract.Requires(itemsBefore != null, "Given items before collection was changed cannot be null");
            Contract.Requires(parentObjectProperty != null, "Given parent object property owning the collection cannot be null");
            Contract.Requires(addedItems != null, "Given set of added item tracking cannot be null");
            Contract.Requires(removedItems != null, "Given set of removed item tracking cannot be null");

            _collection = collection;
            _itemsBefore = itemsBefore;
            _parentObjectProperty = parentObjectProperty;
            _addedItems = addedItems;
            _removedItems = removedItems;
        }

        /// <summary>
        /// Gets the collection subject of changes
        /// </summary>
        public IEnumerable<object> Collection => _collection;

        /// <summary>
        /// Gets a copy of collection items before the collection was changed
        /// </summary>
        public IEnumerable<object> ItemsBefore => _itemsBefore;

        /// <summary>
        /// Gets a reference to the property which holds the collection instance
        /// </summary>
        public PropertyInfo ParentObjectProperty => _parentObjectProperty;

        /// <summary>
        /// Gets the set of added item tracking
        /// </summary>
        public ISet<object> AddedItems => _addedItems;

        /// <summary>
        /// Gets the set of removed item tracking
        /// </summary>
        public ISet<object> RemovedItems => _removedItems;

        /// <summary>
        /// Gets or sets the change produced by a collection change handler like a collection change interceptor
        /// </summary>
        public CollectionChange Change { get; set; }
    }
}