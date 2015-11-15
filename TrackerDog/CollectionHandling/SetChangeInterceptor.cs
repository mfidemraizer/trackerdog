namespace TrackerDog.CollectionHandling
{
    using System.Collections.Generic;
    using System.Linq;

    public class SetChangeInterceptor<T> : DefaultCollectionChangeInterceptor<T>, ISet<T>
    {
        public SetChangeInterceptor(CollectionChangeContext changeContext)
            : base(changeContext)
        {
        }

        bool ISet<T>.Add(T item)
        {
            Add(item);

            return false;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            IDeclaredObjectPropertyChangeTracking tracking = ChangeContext.Collection.GetPropertyTracking(ChangeContext.ParentObjectProperty);

            IEnumerable<object> itemsToRemove = (IEnumerable<object>)other;

            ChangeContext.AddedItems.ExceptWith(itemsToRemove);

            foreach (object item in itemsToRemove.Intersect((IEnumerable<object>)tracking.OldValue))
                ChangeContext.AddedItems.Add(item);

            EvalCollectionChange();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            IDeclaredObjectPropertyChangeTracking tracking = ChangeContext.Collection.GetPropertyTracking(ChangeContext.ParentObjectProperty);
            ChangeContext.AddedItems.IntersectWith(ChangeContext.ItemsBefore);

            IEnumerable<object> itemsToRemove = ((IEnumerable<object>)tracking.OldValue).Except(ChangeContext.Collection).ToList();
            ChangeContext.RemovedItems.ExceptWith(ChangeContext.AddedItems);

            foreach (object item in itemsToRemove)
                ChangeContext.RemovedItems.Add(item);

            EvalCollectionChange();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) => false;

        public bool IsProperSupersetOf(IEnumerable<T> other) => false;

        public bool IsSubsetOf(IEnumerable<T> other) => false;

        public bool IsSupersetOf(IEnumerable<T> other) => false;

        public bool Overlaps(IEnumerable<T> other) => false;

        public bool SetEquals(IEnumerable<T> other) => false;

        public void SymmetricExceptWith(IEnumerable<T> other) { }

        public void UnionWith(IEnumerable<T> other) { }

        private void EvalCollectionChange()
        {
            int beforeCount = ChangeContext.ItemsBefore.Count();
            int afterCount = ChangeContext.Collection.Count();

            if (beforeCount < afterCount) ChangeContext.Change = CollectionChange.Add;
            else if (beforeCount > afterCount) ChangeContext.Change = CollectionChange.Remove;
            else ChangeContext.Change = CollectionChange.None;
        }
    }
}