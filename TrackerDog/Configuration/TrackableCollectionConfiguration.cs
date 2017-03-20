using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrackerDog.CollectionHandling;
using TrackerDog.Contracts;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Represents a configuration to setup how collection change tracking will behave.
    /// </summary>
    internal sealed class TrackableCollectionConfiguration : ICollectionChangeTrackingConfiguration
    {
        private readonly static object _syncLock = new object();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TrackableCollectionConfiguration()
        {
            AddOrUpdateImplementation<ISet<string>, HashSet<string>, SetChangeInterceptor<string>>();
            AddOrUpdateImplementation<IList<string>, List<string>, DefaultCollectionChangeInterceptor<string>>();
            AddOrUpdateImplementation<IDictionary<string, string>, Dictionary<string, string>, DefaultCollectionChangeInterceptor<string>>();
            AddOrUpdateImplementation<ICollection<string>, List<string>, DefaultCollectionChangeInterceptor<string>>();
            AddOrUpdateImplementation<IEnumerable<string>, List<string>, DefaultCollectionChangeInterceptor<string>>();
        }

        /// <summary>
        /// Gets a dictionary of implementations to common collection interfaces.
        /// </summary>
        internal Dictionary<Type, CollectionImplementation> Implementations { get; } = new Dictionary<Type, CollectionImplementation>();

        /// <summary>
        /// Determines if given type can be tracked as collection
        /// </summary>
        /// <param name="some">The whole type to check</param>
        /// <returns><literal>true</literal> if it can be tracked as collection, <literal>false if it can't be tracked as collection</literal></returns>
        public bool CanTrack(Type some)
        {
            Contract.Requires(() => some != null, "A non-null reference to a type is mandatory to get its implementation");
            TypeInfo someTypeInfo = some.GetTypeInfo();

            lock (_syncLock)
            {
                if (some == typeof(string))
                    return false;

                Type someGenericTypeDefinition = someTypeInfo.IsGenericType && !someTypeInfo.IsGenericTypeDefinition ? some.GetGenericTypeDefinition() : null;
                IEnumerable<Type> someInterfaces = some.GetInterfaces();

                return Implementations.Any
                (
                    interfaceType =>
                        someGenericTypeDefinition != null && someGenericTypeDefinition == interfaceType.Key
                        ||
                        someInterfaces.Any
                        (
                            i => i.GetTypeInfo().IsGenericType
                                && i.GetGenericTypeDefinition() == interfaceType.Key
                        )
                );
            }
        }

        /// <summary>
        /// Gets the implementation of a given type. The given type can or cannot be a collection interface, but
        /// it can be also an actual collection type. This method will return the most appropiate generic collection
        /// implementation to the given type.
        /// </summary>
        /// <param name="some">The whole collection type</param>
        /// <returns>A pair, where the key is the collection interface and value is the collection implementation</returns>
        public KeyValuePair<Type, CollectionImplementation> GetImplementation(Type some)
        {
            Contract.Requires(() => some != null, "A non-null reference to a type is mandatory to get its implementation");

            TypeInfo someTypeInfo = some.GetTypeInfo();

            lock (_syncLock)
            {
                Type someGenericTypeDefinition = someTypeInfo.IsGenericType && !someTypeInfo.IsGenericTypeDefinition ? some.GetGenericTypeDefinition() : null;

                IEnumerable<Type> someInterfaces = some.GetInterfaces();

                KeyValuePair<Type, CollectionImplementation> result = Implementations.FirstOrDefault
                (
                    interfaceType =>
                        someGenericTypeDefinition != null && someGenericTypeDefinition == interfaceType.Key
                        ||
                        someInterfaces.Any
                        (
                            i => i.GetTypeInfo().IsGenericType
                                && i.GetGenericTypeDefinition() == interfaceType.Key
                        )
                );

                if (result.Key != null)
                    return result;
                else
                    throw new InvalidOperationException($"No implementation found to '{some.AssemblyQualifiedName}'");
            }
        }

        public void AddOrUpdateImplementation<TInterface, TImplementation, TCollectionChangeInterceptor>()
            where TInterface : IEnumerable
            where TImplementation : class, TInterface
            where TCollectionChangeInterceptor : class
        {
            Type interfaceType = typeof(TInterface).GetTypeInfo().GetGenericTypeDefinition();
            Type implementationType = typeof(TImplementation).GetTypeInfo().GetGenericTypeDefinition();
            Type collectionChangeInterceptorType = typeof(TCollectionChangeInterceptor).GetTypeInfo().GetGenericTypeDefinition();

            Contract.Requires(() => interfaceType.GetTypeInfo().IsInterface, $"Given type in generic parameter '{nameof(TInterface)}' must be an interface");

            lock (_syncLock)
            {
                Implementations[interfaceType] = new CollectionImplementation(implementationType, collectionChangeInterceptorType);
            }
        }
    }
}