namespace TrackerDog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using TrackerDog.Configuration;

    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Clones the given <see cref="System.Collections.IEnumerable"/> concrete implementation
        /// </summary>
        /// <param name="enumerable">The <see cref="System.Collections.IEnumerable"/> to clone</param>
        /// <returns></returns>
        internal static IEnumerable CloneEnumerable(this IEnumerable enumerable)
        {
            Type collectionType = TrackerDogConfiguration.CollectionConfiguration.GetImplementation(enumerable.GetType()).Value;
            Type collectionItemType = enumerable.GetCollectionItemType();

            IEnumerable collectionClone = (IEnumerable)collectionType.CreateInstanceWithGenericArgs(null, new[] { collectionItemType });
            MethodInfo addMethod = collectionClone.GetType().GetMethod("Add");

            foreach (object item in enumerable)
                addMethod.Invoke(collectionClone, new[] { item });

            return collectionClone;
        }

        /// <summary>
        /// Turns all objects into change-trackable ones found in the given sequence.
        /// </summary>
        /// <typeparam name="T">The type of objects</typeparam>
        /// <param name="enumerable">The sequence of objects to turn into change-trackable ones</param>
        /// <param name="parentObjectProperty">The collection property representing the association to some object</param>
        /// <param name="parentObject">The parent object that owns the association of the collection</param>
        /// <returns>The already converted objects into change-trackable ones</returns>
        public static IEnumerable<T> MakeAllTrackable<T>(this IEnumerable<T> enumerable, PropertyInfo parentObjectProperty, IChangeTrackableObject parentObject)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(parentObjectProperty != null);
            Contract.Requires(parentObject != null);
            Contract.Requires(parentObjectProperty.DeclaringType == parentObject.GetType());

            if (enumerable.Count() > 0 &&
                TrackerDogConfiguration.TrackableTypes.Contains(enumerable.First().GetType()))
            {
                List<T> result = new List<T>();

                foreach (T item in enumerable)
                {
                    IChangeTrackableObject trackableObject = item as IChangeTrackableObject;

                    Contract.Assert(trackableObject == null);

                    trackableObject = (IChangeTrackableObject)item.AsTrackable();

                    trackableObject.PropertyChanged += (sender, e) =>
                        parentObject.RaisePropertyChanged(parentObject, parentObjectProperty.Name);

                    result.Add((T)trackableObject);
                }

                Contract.Assert(result.Count == enumerable.Count());

                return result;
            }
            else return enumerable;
        }
    }
}