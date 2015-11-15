namespace TrackerDog.CollectionHandling
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;

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

        public IEnumerable<object> Collection => _collection;
        public IEnumerable<object> ItemsBefore => _itemsBefore;
        public PropertyInfo ParentObjectProperty => _parentObjectProperty;
        public ISet<object> AddedItems => _addedItems;
        public ISet<object> RemovedItems => _removedItems;
        public CollectionChange Change { get; set; }
    }
}