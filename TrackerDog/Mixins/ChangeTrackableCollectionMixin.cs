namespace TrackerDog.Mixins
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.Specialized;
    using System.Reflection;

    internal class ChangeTrackableCollectionMixin : IChangeTrackableCollection, IReadOnlyChangeTrackableCollection
    {
        private readonly HashSet<object> _addedItems = new HashSet<object>();
        private readonly HashSet<object> _removedItems = new HashSet<object>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IChangeTrackableObject ParentObject { get; set; }
        public PropertyInfo ParentObjectProperty { get; set; }
        public IImmutableSet<object> AddedItems => _addedItems.ToImmutableHashSet();
        public IImmutableSet<object> RemovedItems => _removedItems.ToImmutableHashSet();
        HashSet<object> IChangeTrackableCollection.AddedItems => _addedItems;
        HashSet<object> IChangeTrackableCollection.RemovedItems => _removedItems;

        public void RaiseCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<object> changedItems)
        {
            if (CollectionChanged != null)
                CollectionChanged
                (
                    this,
                    new NotifyCollectionChangedEventArgs(action, changedItems.ToImmutableList())
                );
        }
    }
}