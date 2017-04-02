using System.Collections.Generic;
using TrackerDog.Contracts;

namespace TrackerDog
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Clears changes tracked by given collection.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="source"></param>
        public static void ClearChanges<TItem>(this ICollection<TItem> source)
        {
            Contract.Requires(() => source is ICanClearChanges, $"Given collection must implement 'TrackerDog.ICanClearChanges'");

            ICanClearChanges canClearChanges = (ICanClearChanges)source;

            canClearChanges.ClearChanges();
        }
    }
}
