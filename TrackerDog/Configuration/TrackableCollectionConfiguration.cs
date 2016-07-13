namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using TrackerDog.CollectionHandling;

    /// <summary>
    /// Represents a configuration to setup how collection change tracking will behave.
    /// </summary>
    public sealed class TrackableCollectionConfiguration
    {
        private readonly static object _syncLock = new object();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TrackableCollectionConfiguration()
        {
            AddImplementation(typeof(ISet<>), typeof(HashSet<>), typeof(SetChangeInterceptor<>));
            AddImplementation(typeof(IList<>), typeof(List<>), typeof(DefaultCollectionChangeInterceptor<>));
            AddImplementation(typeof(IDictionary<,>), typeof(Dictionary<,>), typeof(DefaultCollectionChangeInterceptor<>));
            AddImplementation(typeof(ICollection<>), typeof(List<>), typeof(DefaultCollectionChangeInterceptor<>));
            AddImplementation(typeof(IEnumerable<>), typeof(List<>), typeof(DefaultCollectionChangeInterceptor<>));
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
            Contract.Requires(some != null, "A non-null reference to a type is mandatory to get its implementation");

            lock (_syncLock)
            {
                if (some == typeof(string))
                    return false;

                Type someGenericTypeDefinition = some.IsGenericType && !some.IsGenericTypeDefinition ? some.GetGenericTypeDefinition() : null;
                IEnumerable<Type> someInterfaces = some.GetInterfaces();

                return Implementations.Any
                (
                    interfaceType =>
                        someGenericTypeDefinition != null && someGenericTypeDefinition == interfaceType.Key
                        ||
                        someInterfaces.Any
                        (
                            i => i.IsGenericType
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
            Contract.Requires(some != null, "A non-null reference to a type is mandatory to get its implementation");
            Contract.Ensures(Contract.Result<KeyValuePair<Type, CollectionImplementation>>().Key != null);

            lock (_syncLock)
            {
                Type someGenericTypeDefinition = some.IsGenericType && !some.IsGenericTypeDefinition ? some.GetGenericTypeDefinition() : null;

                IEnumerable<Type> someInterfaces = some.GetInterfaces();

                KeyValuePair<Type, CollectionImplementation> result = Implementations.FirstOrDefault
                (
                    interfaceType =>
                        someGenericTypeDefinition != null && someGenericTypeDefinition == interfaceType.Key
                        ||
                        someInterfaces.Any
                        (
                            i => i.IsGenericType
                                && i.GetGenericTypeDefinition() == interfaceType.Key
                        )
                );

                if (result.Key != null)
                    return result;
                else
                    throw new InvalidOperationException($"No implementation found to '{some.AssemblyQualifiedName}'");
            }
        }

        /// <summary>
        /// Adds a new implementation to some collection interface. This method will not support replacing
        /// an already added interface/implementation pair.
        /// </summary>
        /// <param name="interfaceType">The collection interface</param>
        /// <param name="implementationType">The collection implementation</param>
        /// <param name="collectionChangeInterceptor">An implementation to interface type which intercepts calls to the whole collection type to handle changes</param>
        public void AddImplementation(Type interfaceType, Type implementationType, Type collectionChangeInterceptor)
        {
            Contract.Requires(interfaceType != null, "Cannot add an implementation of a null interface");
            Contract.Requires(interfaceType.IsInterface, "Given type must be an interface");
            Contract.Requires(interfaceType.IsGenericTypeDefinition, "Given collection interface must be provided as a generic type definition");
            Contract.Requires(collectionChangeInterceptor.GetInterfaces().Any(i => collectionChangeInterceptor.GetInterfaces().Any(i2 => i2 == i)), "Provided change interceptor type must be assignable to collection implementation type");

            lock (_syncLock)
            {
                Contract.Assert(!Implementations.ContainsKey(interfaceType), "Adding an implementation can be done once");

                Implementations.Add(interfaceType, new CollectionImplementation(implementationType, collectionChangeInterceptor));
            }
        }

        /// <summary>
        /// Replaces an existing collection interface/implementation or adds it.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="collectionChangeInterceptor">An implementation to interface type which intercepts calls to the whole collection type to handle changes</param>
        public void ReplaceImplementation(Type interfaceType, Type implementationType, Type collectionChangeInterceptor)
        {
            Contract.Requires(interfaceType != null, "Cannot add an implementation of a null interface");
            Contract.Requires(interfaceType.IsInterface, "Given type must be an interface");
            Contract.Requires(interfaceType.IsGenericTypeDefinition, "Given collection interface must be provided as a generic type definition");
            Contract.Requires(interfaceType.IsAssignableFrom(collectionChangeInterceptor), "Provided change interceptor type must be assignable to collection implementation type");

            lock (_syncLock)
            {
                if (Implementations.ContainsKey(interfaceType))
                    Implementations[interfaceType] = new CollectionImplementation(implementationType, collectionChangeInterceptor);
                else
                    AddImplementation(interfaceType, implementationType, collectionChangeInterceptor);
            }
        }
    }
}