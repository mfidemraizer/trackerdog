namespace TrackerDog.CollectionHandling
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the default collection change interceptor. It intercepts any implementation of
    /// <see cref="ICollection{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of collection items</typeparam>
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
            ChangeContext.Change = CollectionChange.Remove;

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