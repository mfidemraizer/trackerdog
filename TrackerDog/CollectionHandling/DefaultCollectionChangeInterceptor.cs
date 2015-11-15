namespace TrackerDog.CollectionHandling
{
    using System.Collections;
    using System.Collections.Generic;

    public class DefaultCollectionChangeInterceptor<T> : ICollection<T>
    {
        private readonly CollectionChangeContext _changeContext;

        public DefaultCollectionChangeInterceptor(CollectionChangeContext changeContext)
        {
            _changeContext = changeContext;
        }

        protected CollectionChangeContext ChangeContext => _changeContext;

        public int Count => 0;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            ChangeContext.AddedItems.Add(item);
            ChangeContext.RemovedItems.Remove(item);
            ChangeContext.Change = CollectionChange.Add;
        }

        public bool Remove(T item)
        {
            ChangeContext.AddedItems.Remove(item);
            ChangeContext.RemovedItems.Add(item);

            return true;
        }

        public void Clear()
        {
        }

        public bool Contains(T item) => true;

        public void CopyTo(T[] array, int arrayIndex)
        {
        }

        public IEnumerator<T> GetEnumerator() => null;

        IEnumerator IEnumerable.GetEnumerator() => null;
    }
}