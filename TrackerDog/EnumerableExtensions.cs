using TrackerDog.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace TrackerDog
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Clones the given <see cref="System.Collections.IEnumerable"/> concrete implementation
        /// </summary>
        /// <param name="enumerable">The <see cref="System.Collections.IEnumerable"/> to clone</param>
        /// <param name="configuration">Configuration to use to determine how to clone the whole enumerable</param>
        /// <returns>The cloned enumerable</returns>
        internal static IEnumerable CloneEnumerable(this IEnumerable enumerable, IObjectChangeTrackingConfiguration configuration)
        {
            Contract.Requires(enumerable != null, "Given enumerable must be a non-null reference");
            Contract.Ensures(Contract.Result<IEnumerable>() != null);

            Type collectionType = configuration.Collections.GetImplementation(enumerable.GetType()).Value.Type;
            Type collectionItemType = enumerable.GetCollectionItemType();

            List<Type> collectionTypeArguments = new List<Type>();

            if (collectionItemType.IsGenericType && collectionItemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                collectionTypeArguments.AddRange(collectionItemType.GenericTypeArguments);
            else
                collectionTypeArguments.Add(enumerable.GetCollectionItemType());

            IEnumerable collectionClone = (IEnumerable)collectionType
                                            .CreateInstanceWithGenericArgs(null, collectionTypeArguments.ToArray());

            Type collectionInterface = collectionClone.GetType()
                                                .GetInterfaces()
                                                .Single(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));

            MethodInfo addMethod = collectionInterface.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);

            foreach (object item in enumerable)
                addMethod.Invoke(collectionClone, new[] { item });

            return collectionClone;
        }

        // TODO: Remove?
        /// <summary>
        /// Turns all objects into change-trackable ones found in the given sequence.
        /// </summary>
        /// <typeparam name="T">The type of objects</typeparam>
        /// <param name="enumerable">The sequence of objects to turn into change-trackable ones</param>
        /// <param name="parentObjectProperty">The collection property representing the association to some object</param>
        /// <param name="parentObject">The parent object that owns the association of the collection</param>
        /// <returns>The already converted objects into change-trackable ones</returns>
        public static IEnumerable<T> MakeAllTrackable<T>(this IEnumerable<T> enumerable, IObjectChangeTrackingConfiguration configuration, ITrackableObjectFactory trackableObjectFactory, PropertyInfo parentObjectProperty, IChangeTrackableObject parentObject)
            where T : class
        {
            Contract.Requires(enumerable != null, "Given enumerable must be a non-null reference to turn its objects into trackable ones");
            Contract.Requires(parentObjectProperty != null, "A reference to property which holds the enumerable is mandatory");
            Contract.Requires(parentObject != null, "The instance of the object where the property holding the enumerable is declared is mandatory");
            Contract.Requires(parentObjectProperty.DeclaringType.GetActualTypeIfTrackable().IsAssignableFrom(parentObject.GetActualTypeIfTrackable()), "Given property holding the enumerable must be declared on the given parent object type");

            if (enumerable.Count() > 0 &&
                configuration.CanTrackType(enumerable.First().GetType()))
            {
                List<T> result = new List<T>();

                foreach (T item in enumerable)
                {
                    IChangeTrackableObject trackableObject = item as IChangeTrackableObject;

                    if (trackableObject == null)
                    {
                        trackableObject = (IChangeTrackableObject)trackableObjectFactory.CreateFrom(item);

                        trackableObject.PropertyChanged += (sender, e) =>
                            parentObject.RaisePropertyChanged(parentObject, parentObjectProperty.Name);
                    }

                    result.Add((T)trackableObject);
                }

                Contract.Assert(result.Count == enumerable.Count(), "New sequence with objects turned into trackable ones must match the count of source sequence");

                return result;
            }
            else return enumerable;
        }
    }
}